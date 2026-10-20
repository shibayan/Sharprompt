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

    public static readonly DiagnosticDescriptor MemberItemsIncompatibleType = new(
        id: "SHARP006",
        title: "[MemberItems] member has an incompatible type",
        messageFormat: "The member '{0}' referenced by [MemberItems] on property '{1}' is of type '{2}', which cannot be used as 'IEnumerable<{3}>'; the items provider is ignored. Return a collection of '{3}' from the member.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InlineItemsIncompatibleValue = new(
        id: "SHARP007",
        title: "[InlineItems] value has an incompatible type",
        messageFormat: "The value {0} in [InlineItems] on property '{1}' is not a non-null value of type '{2}'; the items provider is ignored. Write every item as a constant of type '{2}', for example 1L for long.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ItemsProviderIgnored = new(
        id: "SHARP008",
        title: "Items attribute is ignored",
        messageFormat: "[{0}] on property '{1}' is ignored because {2}. Remove the attribute.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor PasswordRequiresString = new(
        id: "SHARP009",
        title: "Password property must be a string",
        messageFormat: "Property '{0}' of type '{1}' cannot be bound with a Password prompt, which returns string; it is skipped. Change the property type to string, or remove [DataType(DataType.Password)].",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor PropertyNotReadable = new(
        id: "SHARP010",
        title: "Property without an accessible getter cannot be bound",
        messageFormat: "Property '{0}' cannot be bound by [PromptBindable] because the generated binder reads its current value as the default, but it has no public or internal getter; it is skipped. Add a getter, or apply [BindIgnore].",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor UnsupportedPropertyType = new(
        id: "SHARP011",
        title: "Property type cannot be bound",
        messageFormat: "Property '{0}' of type '{1}' cannot be bound by [PromptBindable] because ref struct and pointer types cannot be used as type arguments; it is skipped. Apply [BindIgnore].",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor NoTypeConverter = new(
        id: "SHARP012",
        title: "Type has no converter from string",
        messageFormat: "Property '{0}' is read as text, but type '{1}' has no TypeConverter that converts from string, so every input is rejected. Use a type that has one (apply [TypeConverter] to your own types), provide items with [InlineItems] or [MemberItems], or apply [BindIgnore].",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InaccessibleValidationAttribute = new(
        id: "SHARP013",
        title: "Validation attribute is not accessible",
        messageFormat: "The validation attribute '{0}' on property '{1}' cannot be used by the generated binder because it, or a type in its arguments, is not accessible from other files in the assembly; the validator is skipped. Make those types public or internal.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor NoBindableProperties = new(
        id: "SHARP014",
        title: "Type has no bindable properties",
        messageFormat: "Type '{0}' has no properties that [PromptBindable] can bind, so no binder is generated and Prompt.Bind throws for it. Add a public instance property with a public or internal setter, or remove [PromptBindable].",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);
}
