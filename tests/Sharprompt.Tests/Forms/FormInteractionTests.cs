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
    public void InputForm_Tab_InsertsDefaultValueForEditing()
    {
        var (driver, configuration) = CreateTestContext();

        driver.EnqueueKey(ConsoleKey.Tab, keyChar: '\t');
        driver.EnqueueText("berry");
        driver.EnqueueEnter();

        var options = new InputOptions<string>
        {
            Message = "message",
            DefaultValue = "blue"
        };

        using var form = new InputForm<string>(options, configuration);

        Assert.Equal("blueberry", form.Start());
    }

    [Fact]
    public void InputForm_TabWithTypedText_DoesNothing()
    {
        var (driver, configuration) = CreateTestContext();

        driver.EnqueueText("red");
        driver.EnqueueKey(ConsoleKey.Tab, keyChar: '\t');
        driver.EnqueueEnter();

        var options = new InputOptions<string>
        {
            Message = "message",
            DefaultValue = "blue"
        };

        using var form = new InputForm<string>(options, configuration);

        Assert.Equal("red", form.Start());
    }

    [Fact]
    public void InputForm_BackspaceOnEmptyBuffer_DismissesDefaultValue()
    {
        var (driver, configuration) = CreateTestContext();

        driver.EnqueueKey(ConsoleKey.Backspace);
        driver.EnqueueEnter();

        var options = new InputOptions<string>
        {
            Message = "message",
            DefaultValue = "blue"
        };

        using var form = new InputForm<string>(options, configuration);

        Assert.Null(form.Start());
    }

    [Fact]
    public void InputForm_DismissedDefaultValue_RequiresInputForNonNullable()
    {
        var (driver, configuration) = CreateTestContext();

        driver.EnqueueKey(ConsoleKey.Backspace);
        driver.EnqueueEnter();
        driver.EnqueueText("7");
        driver.EnqueueEnter();

        var options = new InputOptions<int>
        {
            Message = "message",
            DefaultValue = 5
        };

        using var form = new InputForm<int>(options, configuration);

        Assert.Equal(7, form.Start());
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
    public void SelectForm_MessageLongerThanBufferWidth_StillWorks()
    {
        var (driver, configuration) = CreateTestContext();

        driver.EnqueueKey(ConsoleKey.DownArrow);
        driver.EnqueueKey(ConsoleKey.DownArrow);
        driver.EnqueueEnter();

        var options = new SelectOptions<string>
        {
            Message = new string('x', 200),
            Items = ["apple", "banana", "cherry"]
        };

        using var form = new SelectForm<string>(options, configuration);

        Assert.Equal("banana", form.Start());
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

    [Fact]
    public void PasswordForm_TypedText_ReturnsPasswordWithoutEchoingIt()
    {
        var (driver, configuration) = CreateTestContext();

        driver.EnqueueText("secret");
        driver.EnqueueEnter();

        var options = new PasswordOptions
        {
            Message = "message"
        };

        using var form = new PasswordForm(options, configuration);

        Assert.Equal("secret", form.Start());
        Assert.DoesNotContain("secret", driver.Output);
        Assert.Contains("******", driver.Output);
    }

    [Fact]
    public void PasswordForm_ValidationFailure_ClearsBufferAndRetries()
    {
        var (driver, configuration) = CreateTestContext();

        driver.EnqueueText("short");
        driver.EnqueueEnter();
        driver.EnqueueText("longenough");
        driver.EnqueueEnter();

        var options = new PasswordOptions
        {
            Message = "message",
            Validators = { Validators.MinLength(8) }
        };

        using var form = new PasswordForm(options, configuration);

        Assert.Equal("longenough", form.Start());
    }

    [Fact]
    public void ListForm_CollectsItemsUntilEmptyEnter()
    {
        var (driver, configuration) = CreateTestContext();

        driver.EnqueueText("one");
        driver.EnqueueEnter();
        driver.EnqueueText("two");
        driver.EnqueueEnter();
        driver.EnqueueEnter();

        var options = new ListOptions<string>
        {
            Message = "message"
        };

        using var form = new ListForm<string>(options, configuration);

        Assert.Equal(new[] { "one", "two" }, form.Start());
    }

    [Fact]
    public void ListForm_CtrlDelete_RemovesLastItem()
    {
        var (driver, configuration) = CreateTestContext();

        driver.EnqueueText("one");
        driver.EnqueueEnter();
        driver.EnqueueText("two");
        driver.EnqueueEnter();
        driver.EnqueueKey(ConsoleKey.Delete, ConsoleModifiers.Control);
        driver.EnqueueEnter();

        var options = new ListOptions<string>
        {
            Message = "message"
        };

        using var form = new ListForm<string>(options, configuration);

        Assert.Equal(new[] { "one" }, form.Start());
    }

    [Fact]
    public void ListForm_BelowMinimum_ShowsErrorAndContinues()
    {
        var (driver, configuration) = CreateTestContext();

        driver.EnqueueEnter();
        driver.EnqueueText("one");
        driver.EnqueueEnter();
        driver.EnqueueEnter();

        var options = new ListOptions<string>
        {
            Message = "message",
            Minimum = 1
        };

        using var form = new ListForm<string>(options, configuration);

        Assert.Equal(new[] { "one" }, form.Start());
    }
}
