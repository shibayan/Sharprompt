using System;
using System.ComponentModel.DataAnnotations;

using Sharprompt.Fluent;

using Xunit;

namespace Sharprompt.Tests;

public class FluentExtensionsTests
{
    [Fact]
    public void InputOptions_WithMessage()
    {
        var options = new InputOptions<string>().WithMessage("Enter name");

        Assert.Equal("Enter name", options.Message);
    }

    [Fact]
    public void InputOptions_WithPlaceholder()
    {
        var options = new InputOptions<string>().WithPlaceholder("placeholder");

        Assert.Equal("placeholder", options.Placeholder);
    }

    [Fact]
    public void InputOptions_WithDefaultValue()
    {
        var options = new InputOptions<string>().WithDefaultValue("default");

        Assert.Equal("default", options.DefaultValue);
    }

    [Fact]
    public void InputOptions_AddValidators()
    {
        Func<object?, ValidationResult?> validator = _ => ValidationResult.Success;
        var options = new InputOptions<string>().AddValidators(validator);

        Assert.Single(options.Validators);
        Assert.Same(validator, options.Validators[0]);
    }

    [Fact]
    public void InputOptions_FluentChaining()
    {
        var options = new InputOptions<string>()
            .WithMessage("msg")
            .WithPlaceholder("ph")
            .WithDefaultValue("def");

        Assert.Equal("msg", options.Message);
        Assert.Equal("ph", options.Placeholder);
        Assert.Equal("def", options.DefaultValue);
    }

    [Fact]
    public void ConfirmOptions_WithMessage()
    {
        var options = new ConfirmOptions().WithMessage("Are you sure?");

        Assert.Equal("Are you sure?", options.Message);
    }

    [Fact]
    public void ConfirmOptions_WithDefaultValue()
    {
        var options = new ConfirmOptions().WithDefaultValue(true);

        Assert.True(options.DefaultValue);
    }

    [Fact]
    public void PasswordOptions_WithMessage()
    {
        var options = new PasswordOptions().WithMessage("Enter password");

        Assert.Equal("Enter password", options.Message);
    }

    [Fact]
    public void PasswordOptions_WithPlaceholder()
    {
        var options = new PasswordOptions().WithPlaceholder("placeholder");

        Assert.Equal("placeholder", options.Placeholder);
    }

    [Fact]
    public void PasswordOptions_WithPasswordChar()
    {
        var options = new PasswordOptions().WithPasswordChar("#");

        Assert.Equal("#", options.PasswordChar);
    }

    [Fact]
    public void PasswordOptions_AddValidators()
    {
        Func<object?, ValidationResult?> validator = _ => ValidationResult.Success;
        var options = new PasswordOptions().AddValidators(validator);

        Assert.Single(options.Validators);
    }

    [Fact]
    public void SelectOptions_WithMessage()
    {
        var options = new SelectOptions<string>().WithMessage("Select one");

        Assert.Equal("Select one", options.Message);
    }

    [Fact]
    public void SelectOptions_WithItems()
    {
        var items = new[] { "A", "B", "C" };
        var options = new SelectOptions<string>().WithItems(items);

        Assert.Same(items, options.Items);
    }

    [Fact]
    public void SelectOptions_WithDefaultValue()
    {
        var options = new SelectOptions<string>().WithDefaultValue("B");

        Assert.Equal("B", options.DefaultValue);
    }

    [Fact]
    public void SelectOptions_WithPageSize()
    {
        var options = new SelectOptions<string>().WithPageSize(10);

        Assert.Equal(10, options.PageSize);
    }

    [Fact]
    public void SelectOptions_WithTextSelector()
    {
        Func<string, string> selector = x => x.ToUpperInvariant();
        var options = new SelectOptions<string>().WithTextSelector(selector);

        Assert.Same(selector, options.TextSelector);
    }

    [Fact]
    public void SelectOptions_WithPagination()
    {
        Func<int, int, int, string> pagination = (count, current, total) => $"{current}/{total}";
        var options = new SelectOptions<string>().WithPagination(pagination);

        Assert.Same(pagination, options.Pagination);
    }

    [Fact]
    public void MultiSelectOptions_WithMessage()
    {
        var options = new MultiSelectOptions<string>().WithMessage("Select items");

        Assert.Equal("Select items", options.Message);
    }

    [Fact]
    public void MultiSelectOptions_WithItems()
    {
        var items = new[] { "A", "B" };
        var options = new MultiSelectOptions<string>().WithItems(items);

        Assert.Same(items, options.Items);
    }

    [Fact]
    public void MultiSelectOptions_WithDefaultValues()
    {
        var defaults = new[] { "A" };
        var options = new MultiSelectOptions<string>().WithDefaultValues(defaults);

        Assert.Same(defaults, options.DefaultValues);
    }

    [Fact]
    public void MultiSelectOptions_WithPageSize()
    {
        var options = new MultiSelectOptions<string>().WithPageSize(5);

        Assert.Equal(5, options.PageSize);
    }

    [Fact]
    public void MultiSelectOptions_WithMinimum()
    {
        var options = new MultiSelectOptions<string>().WithMinimum(2);

        Assert.Equal(2, options.Minimum);
    }

    [Fact]
    public void MultiSelectOptions_WithMaximum()
    {
        var options = new MultiSelectOptions<string>().WithMaximum(10);

        Assert.Equal(10, options.Maximum);
    }

    [Fact]
    public void MultiSelectOptions_WithTextSelector()
    {
        Func<string, string> selector = x => x.ToUpperInvariant();
        var options = new MultiSelectOptions<string>().WithTextSelector(selector);

        Assert.Same(selector, options.TextSelector);
    }

    [Fact]
    public void MultiSelectOptions_WithPagination()
    {
        Func<int, int, int, string> pagination = (count, current, total) => $"{current}/{total}";
        var options = new MultiSelectOptions<string>().WithPagination(pagination);

        Assert.Same(pagination, options.Pagination);
    }

    [Fact]
    public void ListOptions_WithMessage()
    {
        var options = new ListOptions<string>().WithMessage("Enter items");

        Assert.Equal("Enter items", options.Message);
    }

    [Fact]
    public void ListOptions_WithDefaultValues()
    {
        var defaults = new[] { "A", "B" };
        var options = new ListOptions<string>().WithDefaultValues(defaults);

        Assert.Same(defaults, options.DefaultValues);
    }

    [Fact]
    public void ListOptions_WithMinimum()
    {
        var options = new ListOptions<string>().WithMinimum(2);

        Assert.Equal(2, options.Minimum);
    }

    [Fact]
    public void ListOptions_WithMaximum()
    {
        var options = new ListOptions<string>().WithMaximum(10);

        Assert.Equal(10, options.Maximum);
    }

    [Fact]
    public void ListOptions_AddValidators()
    {
        Func<object?, ValidationResult?> validator = _ => ValidationResult.Success;
        var options = new ListOptions<string>().AddValidators(validator);

        Assert.Single(options.Validators);
    }
}
