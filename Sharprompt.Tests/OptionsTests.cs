using System;

using Xunit;

namespace Sharprompt.Tests;

public class OptionsTests
{
    [Fact]
    public void InputOptions_EnsureOptions_WithMessage_NoException()
    {
        var options = new InputOptions<string> { Message = "Enter value" };

        var exception = Record.Exception(() => options.EnsureOptions());

        Assert.Null(exception);
    }

    [Fact]
    public void InputOptions_EnsureOptions_WithoutMessage_ThrowsArgumentNullException()
    {
        var options = new InputOptions<string>();

        Assert.Throws<ArgumentNullException>(() => options.EnsureOptions());
    }

    [Fact]
    public void ConfirmOptions_EnsureOptions_WithMessage_NoException()
    {
        var options = new ConfirmOptions { Message = "Are you sure?" };

        var exception = Record.Exception(() => options.EnsureOptions());

        Assert.Null(exception);
    }

    [Fact]
    public void ConfirmOptions_EnsureOptions_WithoutMessage_ThrowsArgumentNullException()
    {
        var options = new ConfirmOptions();

        Assert.Throws<ArgumentNullException>(() => options.EnsureOptions());
    }

    [Fact]
    public void PasswordOptions_EnsureOptions_WithMessage_NoException()
    {
        var options = new PasswordOptions { Message = "Enter password" };

        var exception = Record.Exception(() => options.EnsureOptions());

        Assert.Null(exception);
    }

    [Fact]
    public void PasswordOptions_EnsureOptions_WithoutMessage_ThrowsArgumentNullException()
    {
        var options = new PasswordOptions();

        Assert.Throws<ArgumentNullException>(() => options.EnsureOptions());
    }

    [Fact]
    public void SelectOptions_EnsureOptions_WithRequiredValues_NoException()
    {
        var options = new SelectOptions<string>
        {
            Message = "Select one",
            Items = ["A", "B", "C"]
        };

        var exception = Record.Exception(() => options.EnsureOptions());

        Assert.Null(exception);
    }

    [Fact]
    public void SelectOptions_EnsureOptions_WithoutMessage_ThrowsArgumentNullException()
    {
        var options = new SelectOptions<string>
        {
            Items = ["A", "B"]
        };

        Assert.Throws<ArgumentNullException>(() => options.EnsureOptions());
    }

    [Fact]
    public void SelectOptions_EnsureOptions_WithoutItems_ThrowsArgumentNullException()
    {
        var options = new SelectOptions<string>
        {
            Message = "Select one",
            Items = null!
        };

        Assert.Throws<ArgumentNullException>(() => options.EnsureOptions());
    }

    [Fact]
    public void ListOptions_EnsureOptions_WithRequiredValues_NoException()
    {
        var options = new ListOptions<string> { Message = "Enter items" };

        var exception = Record.Exception(() => options.EnsureOptions());

        Assert.Null(exception);
    }

    [Fact]
    public void ListOptions_EnsureOptions_WithoutMessage_ThrowsArgumentNullException()
    {
        var options = new ListOptions<string>();

        Assert.Throws<ArgumentNullException>(() => options.EnsureOptions());
    }

    [Fact]
    public void ListOptions_EnsureOptions_NegativeMinimum_ThrowsException()
    {
        var options = new ListOptions<string>
        {
            Message = "Enter items",
            Minimum = -1
        };

        Assert.ThrowsAny<Exception>(() => options.EnsureOptions());
    }

    [Fact]
    public void ListOptions_EnsureOptions_MaximumLessThanMinimum_ThrowsException()
    {
        var options = new ListOptions<string>
        {
            Message = "Enter items",
            Minimum = 5,
            Maximum = 3
        };

        Assert.ThrowsAny<Exception>(() => options.EnsureOptions());
    }

    [Fact]
    public void MultiSelectOptions_EnsureOptions_WithRequiredValues_NoException()
    {
        var options = new MultiSelectOptions<string>
        {
            Message = "Select items",
            Items = ["A", "B", "C"]
        };

        var exception = Record.Exception(() => options.EnsureOptions());

        Assert.Null(exception);
    }

    [Fact]
    public void MultiSelectOptions_EnsureOptions_WithoutMessage_ThrowsArgumentNullException()
    {
        var options = new MultiSelectOptions<string>
        {
            Items = ["A", "B"]
        };

        Assert.Throws<ArgumentNullException>(() => options.EnsureOptions());
    }

    [Fact]
    public void MultiSelectOptions_EnsureOptions_NegativeMinimum_ThrowsException()
    {
        var options = new MultiSelectOptions<string>
        {
            Message = "Select items",
            Items = ["A", "B"],
            Minimum = -1
        };

        Assert.ThrowsAny<Exception>(() => options.EnsureOptions());
    }

    [Fact]
    public void MultiSelectOptions_EnsureOptions_MaximumLessThanMinimum_ThrowsException()
    {
        var options = new MultiSelectOptions<string>
        {
            Message = "Select items",
            Items = ["A", "B"],
            Minimum = 5,
            Maximum = 3
        };

        Assert.ThrowsAny<Exception>(() => options.EnsureOptions());
    }
}
