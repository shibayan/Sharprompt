using System;
using System.ComponentModel;
using System.Globalization;

namespace Sharprompt.NativeAot.External;

// Mirrors Temperature in the app, declared in a trimmable library.
[TypeConverter(typeof(ExternalTemperatureConverter))]
public readonly record struct ExternalTemperature(double Celsius);

public sealed class ExternalTemperatureConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        => value is string text && text.EndsWith('C') ? new ExternalTemperature(double.Parse(text[..^1], culture)) : base.ConvertFrom(context, culture, value);

    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
        => value is ExternalTemperature temperature && destinationType == typeof(string) ? temperature.Celsius.ToString(culture) + "C" : base.ConvertTo(context, culture, value, destinationType);
}
