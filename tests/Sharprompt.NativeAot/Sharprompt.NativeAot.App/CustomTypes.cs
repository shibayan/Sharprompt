using System;
using System.ComponentModel;
using System.Globalization;

namespace Sharprompt.NativeAot;

// A custom type converted through the converter its [TypeConverter] names. ExternalTemperature is
// the same type declared in a trimmable library.
[TypeConverter(typeof(TemperatureConverter))]
public readonly record struct Temperature(double Celsius);

public sealed class TemperatureConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        => value is string text && text.EndsWith('C') ? new Temperature(double.Parse(text[..^1], culture)) : base.ConvertFrom(context, culture, value);

    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
        => value is Temperature temperature && destinationType == typeof(string) ? temperature.Celsius.ToString(culture) + "C" : base.ConvertTo(context, culture, value, destinationType);
}
