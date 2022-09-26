using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Sharprompt.Internal;

internal static class EnumHelper<T>
{
    static EnumHelper()
    {
        var values = (T[])Enum.GetValues(typeof(T));

        foreach (var value in values)
        {
            s_metadataCache.Add(value, GetEnumMetadata(value));
        }
    }

    private static readonly Dictionary<T, EnumMetadata> s_metadataCache = new();

    private static EnumMetadata GetEnumMetadata(T value)
    {
        var displayAttribute = typeof(T).GetField(value.ToString())?.GetCustomAttribute<DisplayAttribute>();

        return new EnumMetadata
        {
            DisplayName = displayAttribute?.GetName(),
            Order = displayAttribute?.GetOrder()
        };
    }

    public static string GetDisplayName(T value) => s_metadataCache[value]?.DisplayName ?? value.ToString();

    public static IEnumerable<T> GetValues() => s_metadataCache.OrderBy(x => x.Value.Order)
                                                               .Select(x => x.Key)
                                                               .ToArray();

    private class EnumMetadata
    {
        public string DisplayName { get; set; }
        public int? Order { get; set; }
    }
}
