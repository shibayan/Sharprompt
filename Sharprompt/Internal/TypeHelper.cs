using System;
using System.ComponentModel;

namespace Sharprompt.Internal;

internal static class TypeHelper<T>
{
    private static readonly Type s_targetType = typeof(T);
    private static readonly Type? s_underlyingType = Nullable.GetUnderlyingType(typeof(T));

    public static bool IsNullable => !s_targetType.IsValueType || s_underlyingType is not null;

    public static T? ConvertTo(string value) => (T?)TypeDescriptor.GetConverter(s_underlyingType ?? s_targetType).ConvertFromInvariantString(value);
}
