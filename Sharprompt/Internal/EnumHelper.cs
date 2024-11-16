using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Sharprompt.Internal;

internal static class EnumHelper<TEnum> where TEnum : notnull
{
    static EnumHelper()
    {
        var values = (TEnum[])Enum.GetValuesAsUnderlyingType(typeof(TEnum));

        foreach (var value in values)
        {
            s_metadataCache.Add(value, GetEnumMetadata(value));
        }
    }

    private static readonly Dictionary<TEnum, EnumMetadata> s_metadataCache = new();

    private static EnumMetadata GetEnumMetadata(TEnum value)
    {
        var displayAttribute = typeof(TEnum).GetField(value.ToString()!)?.GetCustomAttribute<DisplayAttribute>();

        return new EnumMetadata(displayAttribute?.GetName(), displayAttribute?.GetOrder());
    }

    public static string GetDisplayName(TEnum value) => s_metadataCache[value].DisplayName ?? value.ToString()!;

    public static IEnumerable<TEnum> GetValues() => s_metadataCache.OrderBy(x => x.Value.Order)
                                                                   .Select(x => x.Key)
                                                                   .ToArray();

    private record EnumMetadata(string? DisplayName, int? Order);
}
