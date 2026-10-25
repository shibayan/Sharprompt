using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Sharprompt.SourceGenerator;

/// <summary>
/// Generates enum values, display names, and ordering metadata for selection prompts.
/// </summary>
/// <remarks>
/// Generated code registers metadata with <c>Sharprompt.EnumMetadataRegistry</c> through a module initializer.
/// Enums that cannot be referenced by the generated code are left to the runtime metadata fallback.
/// </remarks>
[Generator(LanguageNames.CSharp)]
public sealed class EnumMetadataGenerator : IIncrementalGenerator
{
    private static readonly SymbolDisplayFormat s_fullyQualifiedFormat = SymbolDisplayFormat.FullyQualifiedFormat;

    internal const string TransformTrackingName = "EnumMetadata.Transform";

    /// <summary>
    /// Registers the incremental pipeline that discovers enum declarations and emits their metadata.
    /// </summary>
    /// <param name="context">The initialization context supplied by the compiler.</param>
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
        // A localized name ([Display(ResourceType = ...)]) is read through DisplayAttribute.GetName each time it is
        // shown. Generated code could only approximate that lookup, so such enums are left to the reflection fallback.
        if (enumSymbol.GetMembers().OfType<IFieldSymbol>().Any(HasDisplayResourceType))
        {
            return string.Empty;
        }

        // These rules must match EnumMetadataRegistry.CreateFallbackMetadata exactly, so that an enum
        // lists the same items whether or not the generator ran. The base order is the underlying
        // constant value (not declaration order, which reflection cannot observe reliably). Aliased
        // members sharing a value keep one representative, the alias whose name sorts first ordinally;
        // they are indistinguishable at runtime, and duplicate switch arms do not compile (CS8510).
        var members = enumSymbol.GetMembers()
            .OfType<IFieldSymbol>()
            .Where(field => field.HasConstantValue)
            .OrderBy(field => field.ConstantValue, Comparer<object?>.Default)
            .ThenBy(field => field.Name, StringComparer.Ordinal)
            .GroupBy(field => field.ConstantValue)
            .Select(group => group.First())
            .Select((field, index) => AnalyzeMember(field, index))
            .ToArray();

        if (members.Length == 0)
        {
            return string.Empty;
        }

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
            builder.AppendLine("                (" + enumTypeName + ")(" + member.Value + ") => " + Quote(member.DisplayName) + ", // " + member.Name);
        }

        builder.AppendLine("                _ => value.ToString()");
        builder.AppendLine("            });");
        builder.AppendLine("    }");
        builder.AppendLine("}");

        return builder.ToString();
    }

    private static bool HasDisplayResourceType(IFieldSymbol field)
    {
        return field.GetAttributes().Any(attribute =>
            attribute.AttributeClass?.ToDisplayString() == "System.ComponentModel.DataAnnotations.DisplayAttribute" &&
            attribute.NamedArguments.Any(argument => argument.Key == "ResourceType" && !argument.Value.IsNull));
    }

    private static EnumMemberMetadata AnalyzeMember(IFieldSymbol field, int index)
    {
        var displayName = field.Name;
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
            }
        }

        return new EnumMemberMetadata(field.Name, FormatConstant(field.ConstantValue!), displayName, order, index);
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
        public EnumMemberMetadata(string name, string value, string displayName, int? order, int index)
        {
            Name = name;
            Value = value;
            DisplayName = displayName;
            Order = order;
            Index = index;
        }

        public string Name { get; }
        public string Value { get; }
        public string DisplayName { get; }
        public int? Order { get; }
        public int Index { get; }
    }
}
