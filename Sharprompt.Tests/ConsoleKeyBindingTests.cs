using System;

using Sharprompt.Internal;

using Xunit;

namespace Sharprompt.Tests;

public class ConsoleKeyBindingTests
{
    [Fact]
    public void Constructor_DefaultModifiers_IsNone()
    {
        var binding = new ConsoleKeyBinding(ConsoleKey.Enter);

        Assert.Equal(ConsoleKey.Enter, binding.Key);
        Assert.Equal(default(ConsoleModifiers), binding.Modifiers);
    }

    [Fact]
    public void Constructor_WithModifiers_StoresModifiers()
    {
        var binding = new ConsoleKeyBinding(ConsoleKey.LeftArrow, ConsoleModifiers.Control);

        Assert.Equal(ConsoleKey.LeftArrow, binding.Key);
        Assert.Equal(ConsoleModifiers.Control, binding.Modifiers);
    }

    [Fact]
    public void Equals_SameKeyAndModifiers_ReturnsTrue()
    {
        var a = new ConsoleKeyBinding(ConsoleKey.A, ConsoleModifiers.Control);
        var b = new ConsoleKeyBinding(ConsoleKey.A, ConsoleModifiers.Control);

        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.False(a != b);
    }

    [Fact]
    public void Equals_DifferentKey_ReturnsFalse()
    {
        var a = new ConsoleKeyBinding(ConsoleKey.A);
        var b = new ConsoleKeyBinding(ConsoleKey.B);

        Assert.NotEqual(a, b);
        Assert.False(a == b);
        Assert.True(a != b);
    }

    [Fact]
    public void Equals_SameKeyDifferentModifiers_ReturnsFalse()
    {
        var plain = new ConsoleKeyBinding(ConsoleKey.LeftArrow);
        var ctrl = new ConsoleKeyBinding(ConsoleKey.LeftArrow, ConsoleModifiers.Control);

        Assert.NotEqual(plain, ctrl);
        Assert.False(plain == ctrl);
        Assert.True(plain != ctrl);
    }

    [Fact]
    public void GetHashCode_SameKeyAndModifiers_SameHash()
    {
        var a = new ConsoleKeyBinding(ConsoleKey.Delete, ConsoleModifiers.Control);
        var b = new ConsoleKeyBinding(ConsoleKey.Delete, ConsoleModifiers.Control);

        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void GetHashCode_DifferentBindings_DifferentHash()
    {
        var a = new ConsoleKeyBinding(ConsoleKey.Delete);
        var b = new ConsoleKeyBinding(ConsoleKey.Delete, ConsoleModifiers.Control);

        Assert.NotEqual(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void CanBeUsedAsDictionaryKey()
    {
        var dict = new System.Collections.Generic.Dictionary<ConsoleKeyBinding, string>
        {
            [new ConsoleKeyBinding(ConsoleKey.LeftArrow)] = "plain left",
            [new ConsoleKeyBinding(ConsoleKey.LeftArrow, ConsoleModifiers.Control)] = "ctrl left"
        };

        Assert.Equal("plain left", dict[new ConsoleKeyBinding(ConsoleKey.LeftArrow)]);
        Assert.Equal("ctrl left", dict[new ConsoleKeyBinding(ConsoleKey.LeftArrow, ConsoleModifiers.Control)]);
    }

    [Fact]
    public void Equals_WithObject_ReturnsTrueForSameBinding()
    {
        var a = new ConsoleKeyBinding(ConsoleKey.Enter);
        object b = new ConsoleKeyBinding(ConsoleKey.Enter);

        Assert.True(a.Equals(b));
    }

    [Fact]
    public void Equals_WithDifferentType_ReturnsFalse()
    {
        var a = new ConsoleKeyBinding(ConsoleKey.Enter);

        Assert.False(a.Equals("Enter"));
    }
}
