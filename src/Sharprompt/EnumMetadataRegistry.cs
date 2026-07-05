using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Sharprompt;

public static class EnumMetadataRegistry
{
    private static readonly ConcurrentDictionary<Type, object> s_metadata = new();

    public static void Register<TEnum>(IReadOnlyList<TEnum> values, Func<TEnum, string> textSelector) where TEnum : notnull
    {
        s_metadata[typeof(TEnum)] = new EnumMetadata<TEnum>(values, textSelector);
    }

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

    internal static EnumMetadata<TEnum> CreateFallbackMetadata<TEnum>() where TEnum : notnull
    {
        var members = typeof(TEnum).GetFields(BindingFlags.Public | BindingFlags.Static)
                                   .Select((field, index) => (value: (TEnum)field.GetValue(null)!, index, displayAttribute: field.GetCustomAttribute<DisplayAttribute>()))
                                   .ToArray();

        var values = members.OrderBy(x => x.displayAttribute?.GetOrder() ?? int.MaxValue)
                            .ThenBy(x => x.index)
                            .Select(x => x.value)
                            .ToArray();

        var displayNames = new Dictionary<TEnum, string>();

        foreach (var member in members)
        {
            var displayName = member.displayAttribute?.GetName();

            if (displayName is not null)
            {
                displayNames.TryAdd(member.value, displayName);
            }
        }

        return new EnumMetadata<TEnum>(values, value => displayNames.TryGetValue(value, out var displayName) ? displayName : value.ToString()!);
    }

    internal sealed record EnumMetadata<TEnum>(IReadOnlyList<TEnum> Values, Func<TEnum, string> TextSelector);
}
