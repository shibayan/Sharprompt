using System;
using System.Collections.Generic;

using Sharprompt.Forms;

using Xunit;

namespace Sharprompt.Tests;

public class FormInteractionTests
{
    private static (FakeConsoleDriver driver, PromptConfiguration configuration) CreateTestContext()
    {
        var driver = new FakeConsoleDriver();

        var configuration = new PromptConfiguration
        {
            ThrowExceptionOnCancel = true,
            ConsoleDriverFactory = () => driver
        };

        return (driver, configuration);
    }

    [Fact]
    public void SelectForm_ArrowKeysAndEnter_ReturnsSelectedItem()
    {
        var (driver, configuration) = CreateTestContext();

        driver.EnqueueKey(ConsoleKey.DownArrow);
        driver.EnqueueKey(ConsoleKey.DownArrow);
        driver.EnqueueEnter();

        var options = new SelectOptions<string>
        {
            Message = "message",
            Items = ["apple", "banana", "cherry"]
        };

        using var form = new SelectForm<string>(options, configuration);

        Assert.Equal("banana", form.Start());
    }

    [Fact]
    public void SelectForm_FilterKeyword_NarrowsToSingleItem()
    {
        var (driver, configuration) = CreateTestContext();

        driver.EnqueueText("che");
        driver.EnqueueEnter();

        var options = new SelectOptions<string>
        {
            Message = "message",
            Items = ["apple", "banana", "cherry"]
        };

        using var form = new SelectForm<string>(options, configuration);

        Assert.Equal("cherry", form.Start());
    }

    [Fact]
    public void SelectForm_UnregisteredEnum_WorksEndToEnd()
    {
        var (driver, configuration) = CreateTestContext();

        driver.EnqueueKey(ConsoleKey.DownArrow);
        driver.EnqueueKey(ConsoleKey.DownArrow);
        driver.EnqueueEnter();

        var options = new SelectOptions<DayOfWeek>
        {
            Message = "message"
        };

        using var form = new SelectForm<DayOfWeek>(options, configuration);

        Assert.Equal(DayOfWeek.Monday, form.Start());
    }

    [Fact]
    public void SelectForm_EnterWithoutSelection_ShowsErrorAndContinues()
    {
        var (driver, configuration) = CreateTestContext();

        driver.EnqueueEnter();
        driver.EnqueueKey(ConsoleKey.DownArrow);
        driver.EnqueueEnter();

        var options = new SelectOptions<string>
        {
            Message = "message",
            Items = ["apple", "banana", "cherry"]
        };

        using var form = new SelectForm<string>(options, configuration);

        Assert.Equal("apple", form.Start());
    }

    [Fact]
    public void MultiSelectForm_SpacebarToggle_ReturnsCheckedItems()
    {
        var (driver, configuration) = CreateTestContext();

        driver.EnqueueKey(ConsoleKey.DownArrow);
        driver.EnqueueKey(ConsoleKey.Spacebar, keyChar: ' ');
        driver.EnqueueKey(ConsoleKey.DownArrow);
        driver.EnqueueKey(ConsoleKey.Spacebar, keyChar: ' ');
        driver.EnqueueEnter();

        var options = new MultiSelectOptions<string>
        {
            Message = "message",
            Items = ["apple", "banana", "cherry"]
        };

        using var form = new MultiSelectForm<string>(options, configuration);

        Assert.Equal(new[] { "apple", "banana" }, form.Start());
    }

    [Fact]
    public void InputForm_TypedText_ReturnsConvertedValue()
    {
        var (driver, configuration) = CreateTestContext();

        driver.EnqueueText("42");
        driver.EnqueueEnter();

        var options = new InputOptions<int>
        {
            Message = "message"
        };

        using var form = new InputForm<int>(options, configuration);

        Assert.Equal(42, form.Start());
    }

    [Fact]
    public void InputForm_EmptyInputWithDefaultValue_ReturnsDefaultValue()
    {
        var (driver, configuration) = CreateTestContext();

        driver.EnqueueEnter();

        var options = new InputOptions<int>
        {
            Message = "message",
            DefaultValue = 5
        };

        using var form = new InputForm<int>(options, configuration);

        Assert.Equal(5, form.Start());
    }

    [Fact]
    public void InputForm_InvalidInput_ShowsErrorAndContinues()
    {
        var (driver, configuration) = CreateTestContext();

        driver.EnqueueText("abc");
        driver.EnqueueEnter();
        driver.EnqueueKey(ConsoleKey.Backspace);
        driver.EnqueueKey(ConsoleKey.Backspace);
        driver.EnqueueKey(ConsoleKey.Backspace);
        driver.EnqueueText("123");
        driver.EnqueueEnter();

        var options = new InputOptions<int>
        {
            Message = "message"
        };

        using var form = new InputForm<int>(options, configuration);

        Assert.Equal(123, form.Start());
    }

    [Fact]
    public void ConfirmForm_EmptyInputWithDefaultValue_ReturnsDefaultValue()
    {
        var (driver, configuration) = CreateTestContext();

        driver.EnqueueEnter();

        var options = new ConfirmOptions
        {
            Message = "message",
            DefaultValue = true
        };

        using var form = new ConfirmForm(options, configuration);

        Assert.True(form.Start());
    }

    [Fact]
    public void Escape_ThrowsPromptCanceledException()
    {
        var (driver, configuration) = CreateTestContext();

        driver.EnqueueKey(ConsoleKey.Escape);

        var options = new SelectOptions<string>
        {
            Message = "message",
            Items = ["apple", "banana", "cherry"]
        };

        using var form = new SelectForm<string>(options, configuration);

        Assert.Throws<PromptCanceledException>(() => form.Start());
    }

    [Fact]
    public void SelectForm_RendersMessageAndItems()
    {
        var (driver, configuration) = CreateTestContext();

        driver.EnqueueKey(ConsoleKey.DownArrow);
        driver.EnqueueEnter();

        var options = new SelectOptions<string>
        {
            Message = "Pick a fruit",
            Items = ["apple", "banana"]
        };

        using var form = new SelectForm<string>(options, configuration);

        form.Start();

        Assert.Contains("Pick a fruit", driver.Output);
        Assert.Contains("apple", driver.Output);
        Assert.Contains("banana", driver.Output);
    }
}
