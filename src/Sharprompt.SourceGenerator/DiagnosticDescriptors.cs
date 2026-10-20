using Microsoft.CodeAnalysis;

namespace Sharprompt.SourceGenerator;

internal static class DiagnosticDescriptors
{
    private const string Category = "Sharprompt";

    public static readonly DiagnosticDescriptor MemberItemsNotFound = new(
        id: "SHARP001",
        title: "[MemberItems] member not found",
        messageFormat: "The member '{0}' referenced by [MemberItems] on property '{1}' was not found on type '{2}', or is not an accessible public static property or parameterless method; the items provider is ignored",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InitOnlyProperty = new(
        id: "SHARP002",
        title: "Init-only property cannot be bound",
        messageFormat: "Property '{0}' has an init-only setter and cannot be bound by [PromptBindable]; it is skipped. Use a 'set' accessor or apply [BindIgnore].",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor UnsupportedCollectionType = new(
        id: "SHARP003",
        title: "Collection type cannot be bound",
        messageFormat: "Property '{0}' of type '{1}' cannot be bound by [PromptBindable] because the collection type is not supported; it is skipped. Use T[], List<T>, HashSet<T>, IEnumerable<T> or an interface they implement, or apply [BindIgnore].",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor GenericType = new(
        id: "SHARP004",
        title: "Generic type cannot be bound",
        messageFormat: "Type '{0}' cannot be bound by [PromptBindable] because it is generic or nested in a generic type; no binder is generated",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InaccessibleType = new(
        id: "SHARP005",
        title: "Inaccessible type cannot be bound",
        messageFormat: "Type '{0}' cannot be bound by [PromptBindable] because it is not accessible from other files in the assembly; no binder is generated. Make it and its containing types public or internal.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);
}
