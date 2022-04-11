using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Sharprompt.Internal;

internal static class EnumHelper<TEnum> where TEnum : Enum
{
    static EnumHelper()
    {
        var values = (TEnum[])Enum.GetValues(typeof(TEnum));

        foreach (var value in values)
        {
            s_metadataCache.Add(value, GetEnumMetadata(value));
        }
    }

    private static readonly Dictionary<TEnum, EnumMetadata> s_metadataCache = new();

    private static EnumMetadata GetEnumMetadata(TEnum value)
    {
        var displayAttribute = typeof(TEnum).GetField(value.ToString())?.GetCustomAttribute<DisplayAttribute>();

        return new EnumMetadata
        {
            DisplayName = displayAttribute?.GetName(),
            Order = displayAttribute?.GetOrder()
        };
    }

    public static string? GetDisplayName(TEnum value)
    {
        return s_metadataCache[value].DisplayName ?? value.ToString();
    }

    public static IEnumerable<TEnum> GetValues()
    {
        return s_metadataCache.OrderBy(x => x.Value.Order)
                     .Select(x => x.Key)
                     .ToArray();
    }

    private class EnumMetadata
    {
        public string? DisplayName { get; init; }
        public int? Order { get; init; }
    }
}
