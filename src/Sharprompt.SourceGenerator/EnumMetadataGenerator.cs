using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Sharprompt.SourceGenerator;

[Generator(LanguageNames.CSharp)]
public sealed class EnumMetadataGenerator : IIncrementalGenerator
{
    private static readonly SymbolDisplayFormat s_fullyQualifiedFormat = SymbolDisplayFormat.FullyQualifiedFormat;

    internal const string TransformTrackingName = "EnumMetadata.Transform";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // The transform returns only strings, so an unrelated edit re-runs it but the result
        // compares equal and the source output stays cached.
        var provider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node is EnumDeclarationSyntax,
                transform: static (syntaxContext, cancellationToken) => Transform(syntaxContext, cancellationToken))
            .Where(static result => result is not null)
            .WithTrackingName(TransformTrackingName);

        context.RegisterSourceOutput(provider, static (productionContext, result) => productionContext.AddSource(result!.HintName, result.Source));
    }

    private static EnumMetadataResult? Transform(GeneratorSyntaxContext syntaxContext, CancellationToken cancellationToken)
    {
        if (syntaxContext.SemanticModel.GetDeclaredSymbol((EnumDeclarationSyntax)syntaxContext.Node, cancellationToken) is not INamedTypeSymbol enumSymbol)
        {
            return null;
        }

        // Enums the generated code cannot name are left to EnumMetadataRegistry.CreateFallbackMetadata.
        if (!IsReferenceable(enumSymbol))
        {
            return null;
        }

        var source = GenerateSource(enumSymbol);
        if (source.Length == 0)
        {
            return null;
        }

        return new EnumMetadataResult(CreateHintName(enumSymbol), source);
    }

    private static bool IsReferenceable(INamedTypeSymbol enumSymbol)
    {
        // The generated top-level file class can only name a type that is accessible from anywhere in the
        // assembly, is not file-local to another file, and has no type parameter in scope (the open
        // generic type parameters of an enclosing type cannot be supplied).
        for (var type = enumSymbol; type is not null; type = type.ContainingType)
        {
            if (type.DeclaredAccessibility is not (Accessibility.Public or Accessibility.Internal or Accessibility.ProtectedOrInternal))
            {
                return false;
            }

            if (type.IsFileLocal || type.TypeParameters.Length > 0)
            {
                return false;
            }
        }

        return true;
    }

    private static string CreateHintName(INamedTypeSymbol enumSymbol)
    {
        // Namespaces are joined with '.' and nested types with '+', neither of which can appear in an
        // identifier, so distinct enums get distinct names. Hint names are compared case-insensitively,
        // so a hash of the case-sensitive name tells apart enums that differ only in case.
        var name = enumSymbol.MetadataName;

        for (var type = enumSymbol.ContainingType; type is not null; type = type.ContainingType)
        {
            name = type.MetadataName + "+" + name;
        }

        for (var ns = enumSymbol.ContainingNamespace; ns is { IsGlobalNamespace: false }; ns = ns.ContainingNamespace)
        {
            name = ns.MetadataName + "." + name;
        }

        return name + "." + ComputeStableHash(name).ToString("x16", CultureInfo.InvariantCulture) + ".EnumMetadata.g.cs";
    }

    private static ulong ComputeStableHash(string text)
    {
        // 64-bit FNV-1a; string.GetHashCode is randomized per process, so it cannot be used for hint names.
        var hash = 14695981039346656037UL;

        foreach (var c in text)
        {
            hash = (hash ^ c) * 1099511628211UL;
        }

        return hash;
    }

    private static string GenerateSource(INamedTypeSymbol enumSymbol)
    {
        // These rules must match EnumMetadataRegistry.CreateFallbackMetadata exactly, so that an enum
        // lists the same items whether or not the generator ran. The base order is the underlying
        // constant value (not declaration order, which reflection cannot observe reliably). Aliased
        // members sharing a value keep one representative, the alias whose name sorts first ordinally;
        // they are indistinguishable at runtime, and duplicate switch arms do not compile (CS8510).
        var analyzedMembers = enumSymbol.GetMembers()
            .OfType<IFieldSymbol>()
            .Where(field => field.HasConstantValue)
            .OrderBy(field => field.ConstantValue, Comparer<object?>.Default)
            .ThenBy(field => field.Name, StringComparer.Ordinal)
            .GroupBy(field => field.ConstantValue)
            .Select(group => group.First())
            .Select((field, index) => AnalyzeMember(field, index))
            .ToArray();

        // A display name the generated code cannot read the same way DisplayAttribute.GetName does is left to
        // the reflection fallback, which then applies to the whole enum, so the result cannot differ.
        if (analyzedMembers.Length == 0 || analyzedMembers.Any(member => member is null))
        {
            return string.Empty;
        }

        var members = analyzedMembers.Select(member => member!).ToArray();

        // Members without [Display(Order)] come last, in base order. Sort on whether an Order is
        // present first, so they also come after members that explicitly use Order = int.MaxValue.
        var orderedMembers = members
            .OrderBy(member => member.Order is null)
            .ThenBy(member => member.Order)
            .ThenBy(member => member.Index)
            .ToArray();

        var enumTypeName = enumSymbol.ToDisplayString(s_fullyQualifiedFormat);
        var typeIdentifier = Sanitize(enumSymbol.ToDisplayString());

        var builder = new StringBuilder();
        builder.AppendLine("// <auto-generated/>");
        builder.AppendLine("#nullable enable");
        builder.AppendLine();
        builder.AppendLine("file static class __SharpromptEnumMetadata_" + typeIdentifier);
        builder.AppendLine("{");
        builder.AppendLine("    [global::System.Runtime.CompilerServices.ModuleInitializer]");
        builder.AppendLine("    internal static void Register()");
        builder.AppendLine("    {");
        builder.AppendLine("        global::Sharprompt.EnumMetadataRegistry.Register<" + enumTypeName + ">(");
        builder.AppendLine("            new " + enumTypeName + "[]");
        builder.AppendLine("            {");

        // Values are emitted as casts of their constants rather than as member access: the representative
        // alias may be [Obsolete] (error-level obsolete references cannot be suppressed, CS0619), and
        // IFieldSymbol.Name drops the verbatim '@' of keyword names such as @default.
        for (var i = 0; i < orderedMembers.Length; i++)
        {
            var suffix = i == orderedMembers.Length - 1 ? string.Empty : ",";
            builder.AppendLine("                (" + enumTypeName + ")(" + orderedMembers[i].Value + ")" + suffix + " // " + orderedMembers[i].Name);
        }

        builder.AppendLine("            },");
        builder.AppendLine("            value => value switch");
        builder.AppendLine("            {");

        foreach (var member in members)
        {
            builder.AppendLine("                (" + enumTypeName + ")(" + member.Value + ") => " + member.DisplayExpression + ", // " + member.Name);
        }

        builder.AppendLine("                _ => value.ToString()");
        builder.AppendLine("            });");
        builder.AppendLine("    }");
        builder.AppendLine("}");

        return builder.ToString();
    }

    private static EnumMemberMetadata? AnalyzeMember(IFieldSymbol field, int index)
    {
        string? displayName = null;
        ITypeSymbol? resourceType = null;
        int? order = null;

        var displayAttribute = field.GetAttributes()
            .FirstOrDefault(attribute => attribute.AttributeClass?.ToDisplayString() == "System.ComponentModel.DataAnnotations.DisplayAttribute");

        if (displayAttribute != null)
        {
            foreach (var namedArgument in displayAttribute.NamedArguments)
            {
                if (namedArgument.Key == "Name" && namedArgument.Value.Value is string name)
                {
                    displayName = name;
                }
                else if (namedArgument.Key == "Order" && namedArgument.Value.Value is int value)
                {
                    order = value;
                }
                else if (namedArgument.Key == "ResourceType" && namedArgument.Value.Value is ITypeSymbol type)
                {
                    // Any non-null type counts, including arrays and pointers, for which GetName throws.
                    resourceType = type;
                }
            }
        }

        // Without a resource type, Name is the display name itself.
        if (displayName is null || resourceType is null)
        {
            return new EnumMemberMetadata(field.Name, FormatConstant(field.ConstantValue!), Quote(displayName ?? field.Name), order, index);
        }

        // With one, Name is a property of the resource type, which DisplayAttribute.GetName reads on every call,
        // so the current UI culture applies. Read the same property each time an item is shown.
        var resourceProperty = resourceType is INamedTypeSymbol namedResourceType ? GetResourceProperty(namedResourceType, displayName) : null;

        if (resourceProperty is null)
        {
            return null;
        }

        var propertyAccess = resourceType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) + "." + EscapeIdentifier(resourceProperty.Name);

        return new EnumMemberMetadata(field.Name, FormatConstant(field.ConstantValue!), "(" + propertyAccess + " ?? " + Quote(field.Name) + ")", order, index);
    }

    // DisplayAttribute only accepts a public static string property with a public getter on a public resource type.
    // Anything else (a missing or inaccessible property, which makes GetName throw) is left to the fallback, and so
    // are properties that reflection can read but the generated code should not name directly: ref returns (whose
    // reflected type is string&, which GetName rejects), static abstract or virtual interface members (CS8926), and
    // obsolete properties (CS0619, or a warning that fails a warnings-as-errors build).
    private static IPropertySymbol? GetResourceProperty(INamedTypeSymbol resourceType, string name)
    {
        for (var current = resourceType; current != null; current = current.ContainingType)
        {
            // An obsolete type, like an obsolete property, makes the generated access warn or fail.
            if (current.DeclaredAccessibility != Accessibility.Public || current.TypeParameters.Length > 0 || IsObsolete(current))
            {
                return null;
            }
        }

        return resourceType.GetMembers(name)
            .OfType<IPropertySymbol>()
            .FirstOrDefault(property => property is { IsStatic: true, IsIndexer: false, IsAbstract: false, IsVirtual: false, ReturnsByRef: false, ReturnsByRefReadonly: false, DeclaredAccessibility: Accessibility.Public, GetMethod.DeclaredAccessibility: Accessibility.Public, Type.SpecialType: SpecialType.System_String } &&
                                        !IsObsolete(property));
    }

    private static bool IsObsolete(ISymbol symbol)
    {
        return symbol.GetAttributes().Any(attribute => attribute.AttributeClass?.ToDisplayString() == "System.ObsoleteAttribute");
    }

    private static string EscapeIdentifier(string name)
    {
        return SyntaxFacts.GetKeywordKind(name) != SyntaxKind.None ? "@" + name : name;
    }

    private static string FormatConstant(object value)
    {
        // The literal must be able to hold the full range of the underlying type, so add the matching suffix.
        return value switch
        {
            uint u => u.ToString(CultureInfo.InvariantCulture) + "U",
            long l => l.ToString(CultureInfo.InvariantCulture) + "L",
            ulong ul => ul.ToString(CultureInfo.InvariantCulture) + "UL",
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString()!
        };
    }

    private static string Quote(string text)
    {
        return "\"" + text.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r") + "\"";
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

    private sealed record EnumMetadataResult(string HintName, string Source);

    private sealed class EnumMemberMetadata
    {
        public EnumMemberMetadata(string name, string value, string displayExpression, int? order, int index)
        {
            Name = name;
            Value = value;
            DisplayExpression = displayExpression;
            Order = order;
            Index = index;
        }

        public string Name { get; }
        public string Value { get; }
        // A C# expression for the display name: a string literal, or a resource property access.
        public string DisplayExpression { get; }
        public int? Order { get; }
        public int Index { get; }
    }
}
