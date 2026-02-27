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
}
