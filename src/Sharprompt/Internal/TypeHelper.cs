using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Sharprompt.Internal;

internal static class TypeHelper<T>
{
    private static readonly Type s_targetType = typeof(T);
    private static readonly Type? s_underlyingType = Nullable.GetUnderlyingType(typeof(T));

    public static bool IsNullable => !s_targetType.IsValueType || s_underlyingType is not null;

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "TypeDescriptor's intrinsic converters cover the types typically used with prompts (string, numeric types, bool, char, Guid, DateTime, enums, etc.) and do not require unreferenced code. Custom TypeConverter attributes on user-defined types may require the application to preserve the converter type.")]
    [UnconditionalSuppressMessage("Trimming", "IL2077", Justification = "See IL2026 justification: only intrinsic converters are relied upon.")]
    public static T? ConvertTo(string value) => (T?)TypeDescriptor.GetConverter(s_underlyingType ?? s_targetType).ConvertFromInvariantString(value);
}
