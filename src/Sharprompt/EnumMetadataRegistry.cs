using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace Sharprompt;

/// <summary>
/// Holds the items and display names that <c>Select</c> and <c>MultiSelect</c> use for enum types.
/// </summary>
/// <remarks>
/// <para>
/// The Sharprompt source generator registers metadata for each enum at startup. Enums it did not see
/// get metadata built through reflection on first use. Both paths apply the same rules, so an enum
/// lists the same items either way:
/// </para>
/// <list type="number">
/// <item><description>Members are first ordered by their underlying constant value, not by declaration order.</description></item>
/// <item><description>Aliases (members sharing a value) collapse into one item, represented by the alias whose name sorts first ordinally; that member's <see cref="DisplayAttribute"/> provides the display name.</description></item>
/// <item><description>Members with <see cref="DisplayAttribute.Order"/> come first, sorted by it. Members without it come after every ordered member, including one using <see cref="int.MaxValue"/>. Ties keep the value order.</description></item>
/// <item><description>The display name is <see cref="DisplayAttribute.GetName"/>, or the member name without one. It is resolved each time an item is shown, so a name read from <see cref="DisplayAttribute.ResourceType"/> follows the current UI culture.</description></item>
/// </list>
/// </remarks>
public static class EnumMetadataRegistry
{
    private static readonly ConcurrentDictionary<Type, object> s_metadata = new();

    /// <summary>
    /// Registers or replaces the items and display names used by selection prompts for a type.
    /// </summary>
    /// <typeparam name="TEnum">The enum or item type.</typeparam>
    /// <param name="values">The items in display order.</param>
    /// <param name="textSelector">Formats each item for display and filtering.</param>
    public static void Register<TEnum>(IReadOnlyList<TEnum> values, Func<TEnum, string> textSelector) where TEnum : notnull
    {
        s_metadata[typeof(TEnum)] = new EnumMetadata<TEnum>(values, textSelector);
    }
    /// <summary>
    /// Retrieves registered item metadata or creates fallback metadata for an enum on first use.
    /// </summary>
    /// <typeparam name="TEnum">The enum or registered item type.</typeparam>
    /// <param name="values">Receives the ordered items when metadata is available.</param>
    /// <param name="textSelector">Receives the display-text resolver when metadata is available.</param>
    /// <returns>True when registered or enum fallback metadata is available; otherwise false.</returns>
    internal static bool TryGet<TEnum>(out IReadOnlyList<TEnum> values, out Func<TEnum, string> textSelector) where TEnum : notnull
    {
        if (!s_metadata.TryGetValue(typeof(TEnum), out var metadata))
        {
            if (!typeof(TEnum).IsEnum)
            {
                values = null!;
                textSelector = null!;
                return false;
            }

            metadata = s_metadata.GetOrAdd(typeof(TEnum), static _ => CreateFallbackMetadata<TEnum>());
        }

        var item = (EnumMetadata<TEnum>)metadata;

        values = item.Values;
        textSelector = item.TextSelector;

        return true;
    }
    /// <summary>
    /// Builds enum metadata through reflection with deterministic alias selection, ordering, and culture-sensitive display names.
    /// </summary>
    /// <typeparam name="TEnum">The enum or registered item type.</typeparam>
    /// <returns>The enum items and their display-text resolver.</returns>
    [UnconditionalSuppressMessage("Trimming", "IL2090", Justification = "This method is only invoked for enum types, and the trimmer always preserves the fields of enum types it keeps.")]
    internal static EnumMetadata<TEnum> CreateFallbackMetadata<TEnum>() where TEnum : notnull
    {
        // GetFields does not guarantee ordering and MetadataToken is unavailable on
        // Native AOT, so sort by the underlying constant value for a deterministic
        // order. Aliased members sharing the same constant value keep a single
        // representative, chosen by ordinal name order so the result does not depend
        // on the GetFields ordering either. EnumMetadataGenerator applies the same rules.
        var members = typeof(TEnum).GetFields(BindingFlags.Public | BindingFlags.Static)
                                   .Select(field => (value: (TEnum)field.GetValue(null)!, field.Name, displayAttribute: field.GetCustomAttribute<DisplayAttribute>()))
                                   .OrderBy(x => x.value)
                                   .ThenBy(x => x.Name, StringComparer.Ordinal)
                                   .DistinctBy(x => x.value)
                                   .Select((x, index) => (x.value, x.Name, index, x.displayAttribute))
                                   .ToArray();

        // Members without [Display(Order)] come last. Sort on whether an Order is present
        // first, so they also come after members that explicitly use Order = int.MaxValue.
        var values = members.OrderBy(x => x.displayAttribute?.GetOrder() is null)
                            .ThenBy(x => x.displayAttribute?.GetOrder())
                            .ThenBy(x => x.index)
                            .Select(x => x.value)
                            .ToArray();

        // Always map through the representative's field name: Enum.ToString picks
        // an implementation-defined alias, which would defeat the determinism above.
        // GetName is called each time, like the generated code reads a resource property each time,
        // so a name from DisplayAttribute.ResourceType follows the current UI culture.
        var displayMembers = new Dictionary<TEnum, (string Name, DisplayAttribute? DisplayAttribute)>();

        foreach (var member in members)
        {
            displayMembers.TryAdd(member.value, (member.Name, member.displayAttribute));
        }

        return new EnumMetadata<TEnum>(values, value => displayMembers.TryGetValue(value, out var member) ? member.DisplayAttribute?.GetName() ?? member.Name : value.ToString()!);
    }

    /// <summary>
    /// Stores ordered items and the delegate used to resolve each item display name.
    /// </summary>
    /// <typeparam name="TEnum">The enum or registered item type.</typeparam>
    /// <param name="Values">The items in their display order.</param>
    /// <param name="TextSelector">Resolves display text for each item.</param>
    internal sealed record EnumMetadata<TEnum>(IReadOnlyList<TEnum> Values, Func<TEnum, string> TextSelector);
}
