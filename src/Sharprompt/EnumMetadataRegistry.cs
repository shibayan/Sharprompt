using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

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
        if (s_metadata.TryGetValue(typeof(TEnum), out var metadata))
        {
            var item = (EnumMetadata<TEnum>)metadata;
            values = item.Values;
            textSelector = item.TextSelector;
            return true;
        }

        values = null!;
        textSelector = null!;
        return false;
    }

    private sealed record EnumMetadata<TEnum>(IReadOnlyList<TEnum> Values, Func<TEnum, string> TextSelector);
}
