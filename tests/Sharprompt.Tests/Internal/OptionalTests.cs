using System;

using Sharprompt.Internal;

using Xunit;

namespace Sharprompt.Tests;

public class OptionalTests
{
    [Fact]
    public void Empty_HasNoValue()
    {
        var optional = Optional<int>.Empty;

        Assert.False(optional.HasValue);
    }

    [Fact]
    public void Constructor_HasValue()
    {
        var optional = new Optional<int>(42);

        Assert.True(optional.HasValue);
        Assert.Equal(42, optional.Value);
    }

    [Fact]
    public void ImplicitOperator_ReturnsValue()
    {
        var optional = new Optional<string>("hello");

        string value = optional;

        Assert.Equal("hello", value);
    }

    [Fact]
    public void Create_WithNonNull_ReturnsOptionalWithValue()
    {
        var optional = Optional<string>.Create("hello");

        Assert.True(optional.HasValue);
        Assert.Equal("hello", optional.Value);
    }

    [Fact]
    public void Create_WithNull_ReturnsEmpty()
    {
        var optional = Optional<string>.Create((string?)null);

        Assert.False(optional.HasValue);
    }

    [Fact]
    public void Create_FromObject_WithNonNull_ReturnsOptionalWithValue()
    {
        var optional = Optional<int>.Create((object)42);

        Assert.True(optional.HasValue);
        Assert.Equal(42, optional.Value);
    }

    [Fact]
    public void Create_FromObject_WithNull_ReturnsEmpty()
    {
        var optional = Optional<int>.Create((object?)null);

        Assert.False(optional.HasValue);
    }

    [Fact]
    public void Create_FromObject_WithWideningNumeric_ConvertsValue()
    {
        var optional = Optional<long>.Create((object)1);

        Assert.True(optional.HasValue);
        Assert.Equal(1L, optional.Value);
    }

    [Fact]
    public void Create_FromObject_WithNullableTarget_ConvertsToUnderlyingType()
    {
        var optional = Optional<int?>.Create((object)1L);

        Assert.True(optional.HasValue);
        Assert.Equal(1, optional.Value);
    }

    [Fact]
    public void Create_FromObject_WithIntegralValueForEnum_ConvertsValue()
    {
        var optional = Optional<DayOfWeek>.Create((object)1);

        Assert.True(optional.HasValue);
        Assert.Equal(DayOfWeek.Monday, optional.Value);
    }

    [Theory]
    [InlineData(4294967297L)]
    [InlineData(-4294967295L)]
    [InlineData(ulong.MaxValue)]
    public void Create_FromObject_WithOutOfRangeIntegralValueForEnum_ThrowsArgumentException(object value)
    {
        var exception = Assert.Throws<ArgumentException>(() => Optional<DayOfWeek>.Create(value, "DefaultValue"));

        Assert.Equal("DefaultValue", exception.ParamName);
        Assert.IsType<OverflowException>(exception.InnerException);
    }

    [Fact]
    public void Create_FromObject_WithIntegralValueForByteEnum_ConvertsValue()
    {
        var optional = Optional<ByteEnum>.Create((object)2L);

        Assert.True(optional.HasValue);
        Assert.Equal(ByteEnum.B, optional.Value);
    }

    [Fact]
    public void Create_FromObject_WithUnparsableValue_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() => Optional<int>.Create((object)"abc", "DefaultValue"));

        Assert.Equal("DefaultValue", exception.ParamName);
    }

    [Fact]
    public void Create_FromObject_WithIncompatibleType_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() => Optional<int>.Create(new object(), "DefaultValue"));

        Assert.Equal("DefaultValue", exception.ParamName);
    }

    [Fact]
    public void Create_FromObject_WithOtherEnumForEnum_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Optional<DayOfWeek>.Create((object)ConsoleKey.A, "DefaultValue"));
    }

    public enum ByteEnum : byte
    {
        A = 1,
        B = 2
    }
}
