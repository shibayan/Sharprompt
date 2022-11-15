using System;
using System.Collections;
using System.ComponentModel;

namespace Sharprompt.Internal;

internal static class TypeHelper
{
    public static bool IsNullable(Type type) => !type.IsValueType || Nullable.GetUnderlyingType(type) is not null;

    public static bool IsCollection(Type type) => type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type);
}

internal static class TypeHelper<T>
{
    private static readonly Type s_targetType = typeof(T);
    private static readonly Type? s_underlyingType = Nullable.GetUnderlyingType(typeof(T));

    public static bool IsNullable => !s_targetType.IsValueType || s_underlyingType is not null;

    public static T? ConvertTo(string value) => (T?)TypeDescriptor.GetConverter(s_underlyingType ?? s_targetType).ConvertFromInvariantString(value);
}
