using System;

using Xunit;

namespace Sharprompt.Tests;

public class PromptCanceledExceptionTests
{
    [Fact]
    public void DefaultConstructor()
    {
        var exception = new PromptCanceledException();

        Assert.Null(exception.PromptType);
        Assert.NotNull(exception.Message);
    }

    [Fact]
    public void MessageConstructor()
    {
        var exception = new PromptCanceledException("test message");

        Assert.Equal("test message", exception.Message);
        Assert.Null(exception.PromptType);
    }

    [Fact]
    public void MessageAndInnerExceptionConstructor()
    {
        var inner = new InvalidOperationException("inner");
        var exception = new PromptCanceledException("test message", inner);

        Assert.Equal("test message", exception.Message);
        Assert.Same(inner, exception.InnerException);
        Assert.Null(exception.PromptType);
    }

    [Fact]
    public void MessageAndPromptTypeConstructor()
    {
        var exception = new PromptCanceledException("test message", "Input");

        Assert.Equal("test message", exception.Message);
        Assert.Equal("Input", exception.PromptType);
    }
}
