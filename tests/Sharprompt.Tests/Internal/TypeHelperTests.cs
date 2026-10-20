using System;
using System.Globalization;

using Sharprompt.Internal;

using Xunit;

namespace Sharprompt.Tests;

public class TypeHelperTests
{
    [Fact]
    public void IsNullable_ReferenceType_ReturnsTrue()
    {
        Assert.True(TypeHelper<string>.IsNullable);
    }

    [Fact]
    public void IsNullable_ValueType_ReturnsFalse()
    {
        Assert.False(TypeHelper<int>.IsNullable);
    }

    [Fact]
    public void IsNullable_NullableValueType_ReturnsTrue()
    {
        Assert.True(TypeHelper<int?>.IsNullable);
    }

    [Fact]
    public void ConvertTo_Int_ReturnsValue()
    {
        var result = TypeHelper<int>.ConvertTo("42");

        Assert.Equal(42, result);
    }

    [Fact]
    public void ConvertTo_NullableInt_ReturnsValue()
    {
        var result = TypeHelper<int?>.ConvertTo("42");

        Assert.Equal(42, result);
    }

    [Fact]
    public void ConvertTo_Double_ReturnsValue()
    {
        var result = TypeHelper<double>.ConvertTo("3.14");

        Assert.Equal(3.14, result);
    }

    [Fact]
    public void ConvertTo_Bool_ReturnsValue()
    {
        var result = TypeHelper<bool>.ConvertTo("true");

        Assert.True(result);
    }

    [Fact]
    public void ConvertToString_UsesInvariantCulture()
    {
        var previousCulture = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");

            Assert.Equal("1.5", TypeHelper<double>.ConvertToString(1.5));
            Assert.Equal("1.5", TypeHelper<double?>.ConvertToString(1.5));
            Assert.Equal("2026-10-21", TypeHelper<DateOnly>.ConvertToString(new DateOnly(2026, 10, 21)));
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
        }
    }


    [Fact]
    public void ConvertToString_RoundTripsThroughConvertTo()
    {
        // Any value a prompt shows (default value hint, Tab, answer) must parse back to itself.
        AssertRoundTrips((byte)255);
        AssertRoundTrips(-1L);
        AssertRoundTrips(1.25f);
        AssertRoundTrips(0.1 + 0.2);
        AssertRoundTrips(0.1m);
        AssertRoundTrips((Half)1.5);
        AssertRoundTrips(Int128.MaxValue);
        AssertRoundTrips('x');
        AssertRoundTrips(new Guid("8f2c1b9e-3d4a-4e5f-9a6b-7c8d9e0f1a2b"));
        AssertRoundTrips(new DateTime(2026, 10, 21, 9, 30, 0));
        AssertRoundTrips(new DateTimeOffset(2026, 10, 21, 9, 30, 0, TimeSpan.FromHours(9)));
        AssertRoundTrips(new DateOnly(2026, 10, 21));
        AssertRoundTrips(new TimeOnly(9, 30));
        AssertRoundTrips(new TimeSpan(1, 2, 3));
        AssertRoundTrips(new Uri("https://example.com/"));
        AssertRoundTrips(new Version(1, 2, 3));
        AssertRoundTrips(DayOfWeek.Friday);
        AssertRoundTrips<int?>(42);
        AssertRoundTrips((DayOfWeek)7);
        AssertRoundTrips<DayOfWeek?>((DayOfWeek)7);

        // The date and time converters format these as an empty string; TimeOnly.MinValue is midnight.
        AssertRoundTrips(DateTime.MinValue);
        AssertRoundTrips(DateTimeOffset.MinValue);
        AssertRoundTrips(DateOnly.MinValue);
        AssertRoundTrips(TimeOnly.MinValue);
        AssertRoundTrips<TimeOnly?>(TimeOnly.MinValue);
    }

    [Fact]
    public void ConvertToString_UnnamedEnumValue_ReturnsNumber()
    {
        // EnumConverter parses "7" as DayOfWeek but throws when formatting it back.
        Assert.Equal("7", TypeHelper<DayOfWeek>.ConvertToString((DayOfWeek)7));
        Assert.Equal("7", TypeHelper<DayOfWeek?>.ConvertToString((DayOfWeek)7));
        Assert.Equal("Friday", TypeHelper<DayOfWeek>.ConvertToString(DayOfWeek.Friday));
    }

    private static void AssertRoundTrips<T>(T value)
    {
        var text = TypeHelper<T>.ConvertToString(value);

        // An empty text would hide the value and parse back as no input at all.
        Assert.NotEqual("", text);
        Assert.Equal(value, TypeHelper<T>.ConvertTo(text));
    }
}
