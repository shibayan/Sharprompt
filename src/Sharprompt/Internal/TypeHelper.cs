using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Sharprompt.Internal;

internal static class TypeHelper<T>
{
    private static readonly Type s_targetType = typeof(T);
    private static readonly Type? s_underlyingType = Nullable.GetUnderlyingType(typeof(T));

    public static bool IsNullable => !s_targetType.IsValueType || s_underlyingType is not null;

    // Parsing and formatting both use the invariant culture, so every text a prompt shows for a value
    // (a default value hint, the inserted default value, the answer) can be typed back as input.
    public static T? ConvertTo(string value) => (T?)GetConverter().ConvertFromInvariantString(value);

    public static string ConvertToString(T value)
    {
        var converter = GetConverter();

        // EnumConverter parses numeric text into values without a named member (such as "7" for DayOfWeek)
        // but throws when formatting those values back. Enum.ToString formats named values identically and
        // unnamed ones as their number, which parses back. A custom converter on an enum is still honored.
        if (value is Enum && converter.GetType() == typeof(EnumConverter))
        {
            return value.ToString()!;
        }

        var text = converter.ConvertToInvariantString(value);

        // The date and time converters format their MinValue as an empty string (TimeOnly.MinValue is midnight),
        // which would hide the value and parse back as no input. The value's own invariant format parses back.
        if (string.IsNullOrEmpty(text) && value is IFormattable formattable)
        {
            return formattable.ToString(null, CultureInfo.InvariantCulture);
        }

        return text ?? "";
    }

    // T cannot carry DynamicallyAccessedMembers without that annotation flowing into every public Prompt API,
    // so trim analysis cannot prove the converter lookup safe. It holds in practice for the supported types:
    // TypeDescriptor returns intrinsic converters for the BCL types (numeric types, bool, char, string, Guid,
    // date and time types, TimeSpan, Uri, Version, CultureInfo, and enums) without unreferenced code, and
    // TypeConverterAttribute keeps the constructor of the converter a custom type names. Both are verified
    // by publishing and running a Native AOT app against the package in CI (tests/Sharprompt.NativeAot).
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Intrinsic converters and converters named by [TypeConverter] are preserved; verified by the Native AOT tests.")]
    [UnconditionalSuppressMessage("Trimming", "IL2077", Justification = "Intrinsic converters and converters named by [TypeConverter] are preserved; verified by the Native AOT tests.")]
    private static TypeConverter GetConverter() => TypeDescriptor.GetConverter(s_underlyingType ?? s_targetType);
}
