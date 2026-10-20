using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Sharprompt.SourceGenerator;

[Generator(LanguageNames.CSharp)]
public sealed class PromptBindableGenerator : IIncrementalGenerator
{
    private const int DataTypePassword = 11;

    private static readonly SymbolDisplayFormat s_fullyQualifiedFormat = SymbolDisplayFormat.FullyQualifiedFormat;

    internal const string TransformTrackingName = "PromptBindable.Transform";

    internal const string SourceTrackingName = "PromptBindable.Source";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "Sharprompt.PromptBindableAttribute",
                predicate: static (node, _) => node is ClassDeclarationSyntax or RecordDeclarationSyntax,
                transform: static (syntaxContext, cancellationToken) => Transform(syntaxContext, cancellationToken))
            .WithTrackingName(TransformTrackingName);

        var sources = provider
            .Select(static (result, _) => (result.HintName, result.Source))
            .WithTrackingName(SourceTrackingName);

        context.RegisterSourceOutput(sources, static (productionContext, source) =>
        {
            if (source.Source.Length > 0)
            {
                productionContext.AddSource(source.HintName, source.Source);
            }
        });

        // Diagnostics need a location bound to the current syntax tree so that #pragma warning disable
        // and other suppressions apply, so resolve them against the compilation only when reporting.
        // This keeps the source output above independent of the compilation and fully cached.
        var diagnostics = provider
            .Select(static (result, _) => result.Diagnostics)
            .Where(static diagnostics => diagnostics.Length > 0)
            .Collect()
            .Combine(context.CompilationProvider);

        context.RegisterSourceOutput(diagnostics, static (productionContext, pair) =>
        {
            var (diagnosticGroups, compilation) = pair;

            foreach (var diagnostic in diagnosticGroups.SelectMany(static group => group))
            {
                productionContext.ReportDiagnostic(diagnostic.ToDiagnostic(compilation));
            }
        });
    }

    private static BinderResult Transform(GeneratorAttributeSyntaxContext syntaxContext, CancellationToken cancellationToken)
    {
        var typeSymbol = (INamedTypeSymbol)syntaxContext.TargetSymbol;
        var hintName = typeSymbol.ToDisplayString()
            .Replace('<', '_')
            .Replace('>', '_')
            .Replace(',', '_')
            .Replace(' ', '_')
            .Replace('.', '_')
            + ".Binder.g.cs";

        // [PromptBindable] only applies to classes (including record classes); a record struct is already a compile error.
        if (typeSymbol.TypeKind != TypeKind.Class)
        {
            return new BinderResult(hintName, "", new EquatableArray<DiagnosticInfo>([]));
        }

        var diagnostics = new List<DiagnosticInfo>();
        var source = GenerateSource(typeSymbol, syntaxContext.SemanticModel.Compilation, diagnostics, cancellationToken);

        return new BinderResult(hintName, source ?? "", new EquatableArray<DiagnosticInfo>(diagnostics.ToArray()));
    }

    private static string? GenerateSource(INamedTypeSymbol typeSymbol, Compilation compilation, List<DiagnosticInfo> diagnostics, CancellationToken cancellationToken)
    {
        var properties = new List<PropertyMetadata>();

        foreach (var property in GetAllPublicInstanceProperties(typeSymbol))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (HasAttribute(property, "Sharprompt.BindIgnoreAttribute"))
            {
                continue;
            }

            if (property.SetMethod?.DeclaredAccessibility is not (Accessibility.Public or Accessibility.Internal or Accessibility.ProtectedOrInternal))
            {
                continue;
            }

            if (property.SetMethod.IsInitOnly)
            {
                diagnostics.Add(DiagnosticInfo.Create(DiagnosticDescriptors.InitOnlyProperty, property.Locations.FirstOrDefault(), property.Name));
                continue;
            }

            var metadata = Analyze(property, typeSymbol, compilation, diagnostics, cancellationToken);

            if (metadata is null)
            {
                continue;
            }

            metadata.Index = properties.Count;
            properties.Add(metadata);
        }

        if (properties.Count == 0)
        {
            return null;
        }

        // Match EnumMetadataRegistry: members without [Display(Order)] come last, in declaration order.
        properties = properties
            .OrderBy(p => p.Order ?? int.MaxValue)
            .ThenBy(p => p.Index)
            .ToList();

        var fullTypeName = typeSymbol.ToDisplayString(s_fullyQualifiedFormat);

        var builder = new StringBuilder();
        builder.AppendLine("// <auto-generated/>");
        builder.AppendLine("#nullable enable");
        builder.AppendLine();
        builder.AppendLine("file static class __SharpromptBinder_" + Sanitize(typeSymbol.ToDisplayString()));
        builder.AppendLine("{");
        builder.AppendLine("    [global::System.Runtime.CompilerServices.ModuleInitializer]");
        builder.AppendLine("    internal static void Register()");
        builder.AppendLine("    {");
        builder.AppendLine("        global::Sharprompt.ModelBinderRegistry.Register<" + fullTypeName + ">((model, cancellationToken) =>");
        builder.AppendLine("        {");

        for (var i = 0; i < properties.Count; i++)
        {
            if (i > 0)
            {
                builder.AppendLine();
            }

            Emit(builder, properties[i]);
        }

        builder.AppendLine("        });");
        builder.AppendLine("    }");
        builder.AppendLine("}");

        return builder.ToString();
    }

    private static IEnumerable<IPropertySymbol> GetAllPublicInstanceProperties(INamedTypeSymbol type)
    {
        var seen = new HashSet<string>();
        var current = type;

        while (current != null && current.SpecialType != SpecialType.System_Object)
        {
            foreach (var member in current.GetMembers())
            {
                if (member is IPropertySymbol { IsStatic: false, DeclaredAccessibility: Accessibility.Public } property &&
                    seen.Add(property.Name))
                {
                    yield return property;
                }
            }

            current = current.BaseType;
        }
    }

    private static PropertyMetadata? Analyze(IPropertySymbol property, INamedTypeSymbol containingType, Compilation compilation, List<DiagnosticInfo> diagnostics, CancellationToken cancellationToken)
    {
        var metadata = new PropertyMetadata { Name = property.Name };
        var propertyType = property.Type;

        metadata.FullTypeName = propertyType.ToDisplayString(s_fullyQualifiedFormat);

        // Unwrap Nullable<T>
        if (propertyType is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } namedType)
        {
            metadata.UnderlyingType = namedType.TypeArguments[0];
        }
        else
        {
            metadata.UnderlyingType = propertyType;
        }

        metadata.UnderlyingTypeName = metadata.UnderlyingType.ToDisplayString(s_fullyQualifiedFormat);

        // Collection analysis
        var (isCollection, elementType) = AnalyzeCollection(propertyType);
        metadata.IsCollection = isCollection;

        if (elementType != null)
        {
            metadata.ElementTypeName = elementType.ToDisplayString(s_fullyQualifiedFormat);
            metadata.IsElementEnum = elementType.TypeKind == TypeKind.Enum;

            // List and MultiSelect return IEnumerable<T>, so it has to be converted to the property type.
            var conversion = GetCollectionConversion(compilation, propertyType, elementType);

            if (conversion is null)
            {
                diagnostics.Add(DiagnosticInfo.Create(DiagnosticDescriptors.UnsupportedCollectionType, property.Locations.FirstOrDefault(), property.Name, propertyType.ToDisplayString()));
                return null;
            }

            metadata.CollectionConversion = conversion.Value;
        }

        // Enum check
        metadata.IsEnum = metadata.UnderlyingType.TypeKind == TypeKind.Enum;

        // Display attribute
        var displayAttribute = GetAttribute(property, "System.ComponentModel.DataAnnotations.DisplayAttribute");
        if (displayAttribute != null)
        {
            string? name = null, description = null;

            foreach (var arg in displayAttribute.NamedArguments)
            {
                switch (arg.Key)
                {
                    case "Name":
                        name = arg.Value.Value as string;
                        break;
                    case "Description":
                        description = arg.Value.Value as string;
                        break;
                    case "Prompt":
                        metadata.Placeholder = arg.Value.Value as string;
                        break;
                    case "Order":
                        metadata.Order = arg.Value.Value as int?;
                        break;
                }
            }

            metadata.Message = name ?? description ?? property.Name;
        }
        else
        {
            metadata.Message = property.Name;
        }

        // DataType attribute
        var dataTypeAttribute = GetAttribute(property, "System.ComponentModel.DataAnnotations.DataTypeAttribute");
        if (dataTypeAttribute is { ConstructorArguments.Length: > 0 })
        {
            if (dataTypeAttribute.ConstructorArguments[0].Value is int dataTypeValue)
            {
                metadata.IsPassword = dataTypeValue == DataTypePassword;
            }
        }

        // Validation attributes
        foreach (var attribute in property.GetAttributes())
        {
            if (attribute.AttributeClass != null && IsValidationAttribute(attribute.AttributeClass))
            {
                metadata.Validators.Add(GenerateNewExpression(attribute));
            }
        }

        // Items provider: InlineItems
        var inlineAttribute = GetAttribute(property, "Sharprompt.InlineItemsAttribute");
        if (inlineAttribute is { ConstructorArguments.Length: > 0 })
        {
            var arrayConstant = inlineAttribute.ConstructorArguments[0];
            if (arrayConstant.Kind == TypedConstantKind.Array)
            {
                var items = string.Join(", ", arrayConstant.Values.Select(FormatTypedConstant));
                var castType = metadata.ElementTypeName ?? metadata.UnderlyingTypeName;
                metadata.ItemsExpression = "global::System.Linq.Enumerable.Cast<" + castType + ">(new object[] { " + items + " })";
                metadata.HasItemsProvider = true;
            }
        }

        // Items provider: MemberItems
        if (!metadata.HasItemsProvider)
        {
            var memberAttribute = GetAttribute(property, "Sharprompt.MemberItemsAttribute");
            if (memberAttribute is { ConstructorArguments.Length: > 0 })
            {
                var memberName = memberAttribute.ConstructorArguments[0].Value as string;
                ITypeSymbol targetType = containingType;

                if (memberAttribute.ConstructorArguments.Length > 1 &&
                    memberAttribute.ConstructorArguments[1].Value is INamedTypeSymbol specifiedType)
                {
                    targetType = specifiedType;
                }

                if (memberName != null)
                {
                    var targetFullName = targetType.ToDisplayString(s_fullyQualifiedFormat);
                    var firstMember = targetType.GetMembers(memberName)
                        .FirstOrDefault(m => m.IsStatic && m.DeclaredAccessibility == Accessibility.Public &&
                                             m is IPropertySymbol or IMethodSymbol { Parameters.Length: 0, TypeParameters.Length: 0 });

                    if (firstMember is IPropertySymbol)
                    {
                        metadata.ItemsExpression = targetFullName + "." + memberName;
                        metadata.HasItemsProvider = true;
                    }
                    else if (firstMember is IMethodSymbol)
                    {
                        metadata.ItemsExpression = targetFullName + "." + memberName + "()";
                        metadata.HasItemsProvider = true;
                    }
                    else
                    {
                        var location = memberAttribute.ApplicationSyntaxReference?.GetSyntax(cancellationToken).GetLocation() ?? property.Locations.FirstOrDefault();
                        diagnostics.Add(DiagnosticInfo.Create(DiagnosticDescriptors.MemberItemsNotFound, location, memberName, property.Name, targetType.ToDisplayString()));
                    }
                }
            }
        }

        // Determine form type
        metadata.FormType = DetermineFormType(metadata);

        // Determine type argument for generic Prompt methods
        metadata.TypeArg = metadata.FormType switch
        {
            FormKind.Input => metadata.FullTypeName,
            FormKind.Select => metadata.UnderlyingTypeName,
            FormKind.MultiSelect or FormKind.List => metadata.ElementTypeName ?? metadata.UnderlyingTypeName,
            _ => metadata.TypeArg
        };

        return metadata;
    }

    private static FormKind DetermineFormType(PropertyMetadata metadata)
    {
        if (metadata.IsPassword)
        {
            return FormKind.Password;
        }

        if (metadata.UnderlyingType?.SpecialType == SpecialType.System_Boolean)
        {
            return FormKind.Confirm;
        }

        var hasItems = metadata.HasItemsProvider || metadata.IsEnum || metadata.IsElementEnum;

        if (hasItems)
        {
            return metadata.IsCollection ? FormKind.MultiSelect : FormKind.Select;
        }

        return metadata.IsCollection ? FormKind.List : FormKind.Input;
    }

    private static void Emit(StringBuilder builder, PropertyMetadata property)
    {
        const string indent = "            ";

        switch (property.FormType)
        {
            case FormKind.Input:
                EmitInput(builder, property, indent);
                break;
            case FormKind.Password:
                EmitPassword(builder, property, indent);
                break;
            case FormKind.Confirm:
                EmitConfirm(builder, property, indent);
                break;
            case FormKind.Select:
                EmitSelect(builder, property, indent);
                break;
            case FormKind.MultiSelect:
                EmitMultiSelect(builder, property, indent);
                break;
            case FormKind.List:
                EmitList(builder, property, indent);
                break;
        }
    }

    private static void EmitInput(StringBuilder builder, PropertyMetadata property, string indent)
    {
        builder.AppendLine(indent + "model." + property.Name + " = global::Sharprompt.Prompt.Input<" + property.TypeArg + ">(options =>");
        builder.AppendLine(indent + "{");
        builder.AppendLine(indent + "    options.Message = " + Quote(property.Message) + ";");

        if (property.Placeholder != null)
        {
            builder.AppendLine(indent + "    options.Placeholder = " + Quote(property.Placeholder) + ";");
        }

        builder.AppendLine(indent + "    options.DefaultValue = model." + property.Name + ";");
        EmitValidators(builder, property, indent);
        builder.AppendLine(indent + "}, cancellationToken);");
    }

    private static void EmitPassword(StringBuilder builder, PropertyMetadata property, string indent)
    {
        builder.AppendLine(indent + "model." + property.Name + " = global::Sharprompt.Prompt.Password(options =>");
        builder.AppendLine(indent + "{");
        builder.AppendLine(indent + "    options.Message = " + Quote(property.Message) + ";");

        if (property.Placeholder != null)
        {
            builder.AppendLine(indent + "    options.Placeholder = " + Quote(property.Placeholder) + ";");
        }

        EmitValidators(builder, property, indent);
        builder.AppendLine(indent + "}, cancellationToken);");
    }

    private static void EmitConfirm(StringBuilder builder, PropertyMetadata property, string indent)
    {
        builder.AppendLine(indent + "model." + property.Name + " = global::Sharprompt.Prompt.Confirm(options =>");
        builder.AppendLine(indent + "{");
        builder.AppendLine(indent + "    options.Message = " + Quote(property.Message) + ";");
        builder.AppendLine(indent + "    options.DefaultValue = model." + property.Name + ";");
        builder.AppendLine(indent + "}, cancellationToken);");
    }

    private static void EmitSelect(StringBuilder builder, PropertyMetadata property, string indent)
    {
        builder.AppendLine(indent + "model." + property.Name + " = global::Sharprompt.Prompt.Select<" + property.TypeArg + ">(options =>");
        builder.AppendLine(indent + "{");
        builder.AppendLine(indent + "    options.Message = " + Quote(property.Message) + ";");

        if (property.ItemsExpression != null)
        {
            builder.AppendLine(indent + "    options.Items = " + property.ItemsExpression + ";");
        }

        builder.AppendLine(indent + "    options.DefaultValue = model." + property.Name + ";");
        builder.AppendLine(indent + "}, cancellationToken);");
    }

    private static void EmitMultiSelect(StringBuilder builder, PropertyMetadata property, string indent)
    {
        var (prefix, suffix) = GetCollectionConversionWrapper(property);

        builder.AppendLine(indent + "model." + property.Name + " = " + prefix + "global::Sharprompt.Prompt.MultiSelect<" + property.TypeArg + ">(options =>");
        builder.AppendLine(indent + "{");
        builder.AppendLine(indent + "    options.Message = " + Quote(property.Message) + ";");

        if (property.ItemsExpression != null)
        {
            builder.AppendLine(indent + "    options.Items = " + property.ItemsExpression + ";");
        }

        builder.AppendLine(indent + "    options.DefaultValues = " + GetDefaultValuesExpression(property) + ";");
        builder.AppendLine(indent + "}, cancellationToken)" + suffix + ";");
    }

    private static void EmitList(StringBuilder builder, PropertyMetadata property, string indent)
    {
        var (prefix, suffix) = GetCollectionConversionWrapper(property);

        builder.AppendLine(indent + "model." + property.Name + " = " + prefix + "global::Sharprompt.Prompt.List<" + property.TypeArg + ">(options =>");
        builder.AppendLine(indent + "{");
        builder.AppendLine(indent + "    options.Message = " + Quote(property.Message) + ";");
        builder.AppendLine(indent + "    options.DefaultValues = " + GetDefaultValuesExpression(property) + ";");
        EmitValidators(builder, property, indent);
        builder.AppendLine(indent + "}, cancellationToken)" + suffix + ";");
    }

    private static (string Prefix, string Suffix) GetCollectionConversionWrapper(PropertyMetadata property)
    {
        return property.CollectionConversion switch
        {
            CollectionConversion.ToArray => ("global::System.Linq.Enumerable.ToArray(", ")"),
            CollectionConversion.ToList => ("global::System.Linq.Enumerable.ToList(", ")"),
            CollectionConversion.ToHashSet => ("new global::System.Collections.Generic.HashSet<" + property.TypeArg + ">(", ")"),
            _ => ("", "")
        };
    }

    private static string GetDefaultValuesExpression(PropertyMetadata property)
    {
        // Cast concrete collection types to IEnumerable<T> so that '??' has a common type with the empty array.
        var value = property.CollectionConversion == CollectionConversion.None
            ? "model." + property.Name
            : "((global::System.Collections.Generic.IEnumerable<" + property.TypeArg + ">?)model." + property.Name + ")";

        return value + " ?? global::System.Array.Empty<" + property.TypeArg + ">()";
    }

    private static void EmitValidators(StringBuilder builder, PropertyMetadata property, string indent)
    {
        foreach (var validator in property.Validators)
        {
            builder.AppendLine(indent + "    {");
            builder.AppendLine(indent + "        var __v = " + validator + ";");
            builder.AppendLine(indent + "        var __c = new global::System.ComponentModel.DataAnnotations.ValidationContext(model) { DisplayName = " + Quote(property.Name) + ", MemberName = " + Quote(property.Name) + " };");
            builder.AppendLine(indent + "        options.Validators.Add(input => __v.GetValidationResult(input, __c));");
            builder.AppendLine(indent + "    }");
        }
    }

    #region Helpers

    private static bool HasAttribute(ISymbol symbol, string fullName)
    {
        return symbol.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString() == fullName);
    }

    private static AttributeData? GetAttribute(ISymbol symbol, string fullName)
    {
        return symbol.GetAttributes().FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == fullName);
    }

    private static bool IsValidationAttribute(ITypeSymbol type)
    {
        var current = type.BaseType;

        while (current != null)
        {
            if (current.ToDisplayString() == "System.ComponentModel.DataAnnotations.ValidationAttribute")
            {
                return true;
            }

            current = current.BaseType;
        }

        return false;
    }

    private static (bool IsCollection, ITypeSymbol? ElementType) AnalyzeCollection(ITypeSymbol type)
    {
        if (type.SpecialType == SpecialType.System_String)
        {
            return (false, null);
        }

        if (type is IArrayTypeSymbol arrayType)
        {
            return (true, arrayType.ElementType);
        }

        if (type is INamedTypeSymbol namedType)
        {
            if (namedType.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T)
            {
                return (true, namedType.TypeArguments[0]);
            }

            foreach (var interfaceType in namedType.AllInterfaces)
            {
                if (interfaceType.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T)
                {
                    return (true, interfaceType.TypeArguments[0]);
                }
            }
        }

        return (false, null);
    }

    private static CollectionConversion? GetCollectionConversion(Compilation compilation, ITypeSymbol propertyType, ITypeSymbol elementType)
    {
        var enumerableType = compilation.GetSpecialType(SpecialType.System_Collections_Generic_IEnumerable_T).Construct(elementType);

        if (compilation.HasImplicitConversion(enumerableType, propertyType))
        {
            return CollectionConversion.None;
        }

        if (propertyType is IArrayTypeSymbol { IsSZArray: true } arrayType && SymbolEqualityComparer.Default.Equals(arrayType.ElementType, elementType))
        {
            return CollectionConversion.ToArray;
        }

        // List<T> also covers IList<T>, ICollection<T>, IReadOnlyList<T> and IReadOnlyCollection<T>.
        if (IsAssignableFromConstructedType(compilation, "System.Collections.Generic.List`1", elementType, propertyType))
        {
            return CollectionConversion.ToList;
        }

        // HashSet<T> also covers ISet<T> and IReadOnlySet<T>.
        if (IsAssignableFromConstructedType(compilation, "System.Collections.Generic.HashSet`1", elementType, propertyType))
        {
            return CollectionConversion.ToHashSet;
        }

        return null;
    }

    private static bool IsAssignableFromConstructedType(Compilation compilation, string metadataName, ITypeSymbol typeArgument, ITypeSymbol destination)
    {
        var definition = compilation.GetTypeByMetadataName(metadataName);

        return definition != null && compilation.HasImplicitConversion(definition.Construct(typeArgument), destination);
    }

    private static string GenerateNewExpression(AttributeData attribute)
    {
        var builder = new StringBuilder();
        builder.Append("new ");
        builder.Append(attribute.AttributeClass!.ToDisplayString(s_fullyQualifiedFormat));
        builder.Append('(');

        for (var i = 0; i < attribute.ConstructorArguments.Length; i++)
        {
            if (i > 0)
            {
                builder.Append(", ");
            }

            builder.Append(FormatTypedConstant(attribute.ConstructorArguments[i]));
        }

        builder.Append(')');

        if (attribute.NamedArguments.Length > 0)
        {
            builder.Append(" { ");

            for (var i = 0; i < attribute.NamedArguments.Length; i++)
            {
                if (i > 0)
                {
                    builder.Append(", ");
                }

                builder.Append(attribute.NamedArguments[i].Key);
                builder.Append(" = ");
                builder.Append(FormatTypedConstant(attribute.NamedArguments[i].Value));
            }

            builder.Append(" }");
        }

        return builder.ToString();
    }

    private static string FormatTypedConstant(TypedConstant typedConstant)
    {
        if (typedConstant.IsNull)
        {
            return "null";
        }

        switch (typedConstant.Kind)
        {
            case TypedConstantKind.Primitive:
                return typedConstant.Value switch
                {
                    string s => Quote(s),
                    bool b => b ? "true" : "false",
                    char c => "'" + EscapeChar(c) + "'",
                    float f => FormatSingle(f),
                    double d => FormatDouble(d),
                    decimal m => m.ToString(CultureInfo.InvariantCulture) + "m",
                    long l => l.ToString(CultureInfo.InvariantCulture) + "L",
                    ulong ul => ul.ToString(CultureInfo.InvariantCulture) + "UL",
                    _ => FormatInvariant(typedConstant.Value)
                };

            case TypedConstantKind.Enum:
                return "(" + typedConstant.Type!.ToDisplayString(s_fullyQualifiedFormat) + ")(" + FormatInvariant(typedConstant.Value) + ")";

            case TypedConstantKind.Type:
                return "typeof(" + ((ITypeSymbol)typedConstant.Value!).ToDisplayString(s_fullyQualifiedFormat) + ")";

            case TypedConstantKind.Array:
                var elements = string.Join(", ", typedConstant.Values.Select(FormatTypedConstant));
                return "new " + typedConstant.Type!.ToDisplayString(s_fullyQualifiedFormat) + " { " + elements + " }";

            default:
                return typedConstant.Value?.ToString() ?? "null";
        }
    }

    // Numeric literals must not depend on the culture of the build machine (e.g. "0,5" on de-DE).
    private static string FormatInvariant(object? value)
    {
        return value switch
        {
            null => "null",
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString()
        };
    }

    private static string FormatSingle(float value)
    {
        if (float.IsNaN(value))
        {
            return "float.NaN";
        }

        if (float.IsPositiveInfinity(value))
        {
            return "float.PositiveInfinity";
        }

        if (float.IsNegativeInfinity(value))
        {
            return "float.NegativeInfinity";
        }

        return value.ToString("R", CultureInfo.InvariantCulture) + "f";
    }

    private static string FormatDouble(double value)
    {
        if (double.IsNaN(value))
        {
            return "double.NaN";
        }

        if (double.IsPositiveInfinity(value))
        {
            return "double.PositiveInfinity";
        }

        if (double.IsNegativeInfinity(value))
        {
            return "double.NegativeInfinity";
        }

        return value.ToString("R", CultureInfo.InvariantCulture) + "d";
    }

    private static string Quote(string text)
    {
        return "\"" + text.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r") + "\"";
    }

    private static string EscapeChar(char c)
    {
        return c switch
        {
            '\\' => "\\\\",
            '\'' => "\\'",
            '\n' => "\\n",
            '\r' => "\\r",
            '\t' => "\\t",
            _ => c.ToString()
        };
    }

    private static string Sanitize(string name)
    {
        var builder = new StringBuilder(name.Length);

        foreach (var c in name)
        {
            builder.Append(char.IsLetterOrDigit(c) ? c : '_');
        }

        return builder.ToString();
    }

    #endregion

    #region Data structures

    private enum FormKind
    {
        Input,
        Password,
        Confirm,
        Select,
        MultiSelect,
        List
    }

    private enum CollectionConversion
    {
        None,
        ToArray,
        ToList,
        ToHashSet
    }

    private sealed record BinderResult(string HintName, string Source, EquatableArray<DiagnosticInfo> Diagnostics);

    private sealed class PropertyMetadata
    {
        public string Name { get; set; } = "";
        public int Index { get; set; }
        public CollectionConversion CollectionConversion { get; set; }
        public string FullTypeName { get; set; } = "";
        public ITypeSymbol? UnderlyingType { get; set; }
        public string UnderlyingTypeName { get; set; } = "";
        public string? ElementTypeName { get; set; }
        public bool IsCollection { get; set; }
        public bool IsEnum { get; set; }
        public bool IsElementEnum { get; set; }
        public string Message { get; set; } = "";
        public string? Placeholder { get; set; }
        public int? Order { get; set; }
        public bool IsPassword { get; set; }
        public FormKind FormType { get; set; }
        public string? TypeArg { get; set; }
        public List<string> Validators { get; set; } = [];
        public string? ItemsExpression { get; set; }
        public bool HasItemsProvider { get; set; }
    }

    #endregion
}
