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
}
