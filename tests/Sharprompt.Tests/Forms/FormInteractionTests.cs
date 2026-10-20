using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Threading;

using Sharprompt.Forms;
using Sharprompt.Strings;

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
    public void MultiSelectForm_CtrlA_SelectsAllItems()
    {
        var (driver, configuration) = CreateTestContext();

        driver.EnqueueKey(ConsoleKey.A, ConsoleModifiers.Control, '\x01');
        driver.EnqueueEnter();

        var options = new MultiSelectOptions<string>
        {
            Message = "message",
            Items = ["apple", "banana", "cherry"]
        };

        using var form = new MultiSelectForm<string>(options, configuration);

        Assert.Equal(new[] { "apple", "banana", "cherry" }, form.Start());
    }

    [Fact]
    public void MultiSelectForm_CtrlA_AllSelected_DeselectsAllItems()
    {
        var (driver, configuration) = CreateTestContext();

        driver.EnqueueKey(ConsoleKey.A, ConsoleModifiers.Control, '\x01');
        driver.EnqueueEnter();

        var options = new MultiSelectOptions<string>
        {
            Message = "message",
            Items = ["apple", "banana", "cherry"],
            DefaultValues = ["apple", "banana", "cherry"],
            Minimum = 0
        };

        using var form = new MultiSelectForm<string>(options, configuration);

        Assert.Empty(form.Start());
    }

    [Fact]
    public void MultiSelectForm_CtrlA_ExceedingMaximum_KeepsSelection()
    {
        var (driver, configuration) = CreateTestContext();

        driver.EnqueueKey(ConsoleKey.A, ConsoleModifiers.Control, '\x01');
        driver.EnqueueEnter();

        var options = new MultiSelectOptions<string>
        {
            Message = "message",
            Items = ["apple", "banana", "cherry"],
            DefaultValues = ["apple"],
            Maximum = 2
        };

        using var form = new MultiSelectForm<string>(options, configuration);

        Assert.Equal(new[] { "apple" }, form.Start());
    }

    [Fact]
    public void MultiSelectForm_CtrlA_WithFilter_SelectsOnlyFilteredItems()
    {
        var (driver, configuration) = CreateTestContext();

        driver.EnqueueText("b");
        driver.EnqueueKey(ConsoleKey.A, ConsoleModifiers.Control, '\x01');
        driver.EnqueueEnter();

        var options = new MultiSelectOptions<string>
        {
            Message = "message",
            Items = ["apple", "banana", "cherry", "blueberry"]
        };

        using var form = new MultiSelectForm<string>(options, configuration);

        Assert.Equal(new[] { "banana", "blueberry" }, form.Start());
    }

    [Fact]
    public void MultiSelectForm_CtrlA_WithFilter_DeselectsOnlyFilteredItems()
    {
        var (driver, configuration) = CreateTestContext();

        driver.EnqueueText("b");
        driver.EnqueueKey(ConsoleKey.A, ConsoleModifiers.Control, '\x01');
        driver.EnqueueEnter();

        var options = new MultiSelectOptions<string>
        {
            Message = "message",
            Items = ["apple", "banana", "cherry", "blueberry"],
            DefaultValues = ["apple", "banana", "blueberry"]
        };

        using var form = new MultiSelectForm<string>(options, configuration);

        Assert.Equal(new[] { "apple" }, form.Start());
    }

    [Fact]
    public void MultiSelectForm_CtrlI_InvertsSelection()
    {
        var (driver, configuration) = CreateTestContext();

        driver.EnqueueKey(ConsoleKey.I, ConsoleModifiers.Control, '\t');
        driver.EnqueueEnter();

        var options = new MultiSelectOptions<string>
        {
            Message = "message",
            Items = ["apple", "banana", "cherry"],
            DefaultValues = ["apple"]
        };

        using var form = new MultiSelectForm<string>(options, configuration);

        Assert.Equal(new[] { "banana", "cherry" }, form.Start());
    }

    [Fact]
    public void MultiSelectForm_CtrlI_WithFilter_PreservesHiddenSelections()
    {
        var (driver, configuration) = CreateTestContext();

        driver.EnqueueText("b");
        driver.EnqueueKey(ConsoleKey.I, ConsoleModifiers.Control, '\t');
        driver.EnqueueEnter();

        var options = new MultiSelectOptions<string>
        {
            Message = "message",
            Items = ["apple", "banana", "cherry", "blueberry"],
            DefaultValues = ["apple", "banana"]
        };

        using var form = new MultiSelectForm<string>(options, configuration);

        Assert.Equal(new[] { "apple", "blueberry" }, form.Start());
    }

    [Fact]
    public void MultiSelectForm_CtrlI_ExceedingMaximum_KeepsSelection()
    {
        var (driver, configuration) = CreateTestContext();

        driver.EnqueueKey(ConsoleKey.I, ConsoleModifiers.Control, '\t');
        driver.EnqueueEnter();

        var options = new MultiSelectOptions<string>
        {
            Message = "message",
            Items = ["apple", "banana", "cherry"],
            DefaultValues = ["apple"],
            Maximum = 1
        };

        using var form = new MultiSelectForm<string>(options, configuration);

        Assert.Equal(new[] { "apple" }, form.Start());
    }

    [Fact]
    public void SelectForm_EmptyItems_RendersWithoutThrowing()
    {
        var (driver, configuration) = CreateTestContext();

        driver.EnqueueKey(ConsoleKey.DownArrow);
        driver.EnqueueKey(ConsoleKey.UpArrow);
        driver.EnqueueEnter();
        driver.EnqueueKey(ConsoleKey.Escape);

        var options = new SelectOptions<string>
        {
            Message = "message",
            Items = []
        };

        using var form = new SelectForm<string>(options, configuration);

        // Nothing can be selected, so Enter keeps prompting until the user cancels.
        Assert.Throws<PromptCanceledException>(() => form.Start());
    }

    [Fact]
    public void SelectForm_DefaultValueOfCompatibleType_SelectsItem()
    {
        var (driver, configuration) = CreateTestContext();

        driver.EnqueueEnter();

        var options = new SelectOptions<long>
        {
            Message = "message",
            Items = [1L, 2L, 3L],
            DefaultValue = 2
        };

        using var form = new SelectForm<long>(options, configuration);

        Assert.Equal(2L, form.Start());
    }

    [Fact]
    public void MultiSelectForm_EmptyItemsWithZeroMinimum_ReturnsEmpty()
    {
        var (driver, configuration) = CreateTestContext();

        driver.EnqueueKey(ConsoleKey.DownArrow);
        driver.EnqueueKey(ConsoleKey.Spacebar, keyChar: ' ');
        driver.EnqueueEnter();

        var options = new MultiSelectOptions<string>
        {
            Message = "message",
            Items = [],
            Minimum = 0
        };

        using var form = new MultiSelectForm<string>(options, configuration);

        Assert.Empty(form.Start());
    }

    [Fact]
    public void MultiSelectForm_DefaultValuesNotInItems_AreIgnored()
    {
        var (driver, configuration) = CreateTestContext();

        // The first Enter must fail Minimum because "zebra" is not a selectable item.
        driver.EnqueueEnter();
        driver.EnqueueKey(ConsoleKey.DownArrow);
        driver.EnqueueKey(ConsoleKey.Spacebar, keyChar: ' ');
        driver.EnqueueEnter();

        var options = new MultiSelectOptions<string>
        {
            Message = "message",
            Items = ["apple", "banana"],
            DefaultValues = ["zebra"],
            Minimum = 1
        };

        using var form = new MultiSelectForm<string>(options, configuration);

        Assert.Equal(new[] { "apple" }, form.Start());
    }

    [Fact]
    public void MultiSelectForm_DefaultValuesExceedingMaximum_RequireDeselection()
    {
        var (driver, configuration) = CreateTestContext();

        driver.EnqueueEnter();
        driver.EnqueueKey(ConsoleKey.DownArrow);
        driver.EnqueueKey(ConsoleKey.Spacebar, keyChar: ' ');
        driver.EnqueueKey(ConsoleKey.DownArrow);
        driver.EnqueueKey(ConsoleKey.Spacebar, keyChar: ' ');
        driver.EnqueueEnter();

        var options = new MultiSelectOptions<string>
        {
            Message = "message",
            Items = ["apple", "banana", "cherry"],
            DefaultValues = ["apple", "banana", "cherry"],
            Maximum = 1
        };

        using var form = new MultiSelectForm<string>(options, configuration);

        Assert.Equal(new[] { "cherry" }, form.Start());
    }

    [Fact]
    public void MultiSelectForm_ItemsEnumeratedOnce()
    {
        var (driver, configuration) = CreateTestContext();

        driver.EnqueueKey(ConsoleKey.DownArrow);
        driver.EnqueueKey(ConsoleKey.Spacebar, keyChar: ' ');
        driver.EnqueueEnter();

        var enumerationCount = 0;

        IEnumerable<string> Items()
        {
            enumerationCount++;

            yield return "apple";
            yield return "banana";
        }

        var options = new MultiSelectOptions<string>
        {
            Message = "message",
            Items = Items()
        };

        using var form = new MultiSelectForm<string>(options, configuration);

        Assert.Equal(new[] { "apple" }, form.Start());
        Assert.Equal(1, enumerationCount);
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
    public void InputForm_Tab_RoundTripsDefaultValueUnderNonInvariantCulture()
    {
        var previousCulture = CultureInfo.CurrentCulture;

        try
        {
            // de-DE formats 1.5 as "1,5" via ToString, which ConvertFromInvariantString
            // cannot parse back; the inserted text must use the invariant format.
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");

            var (driver, configuration) = CreateTestContext();

            driver.EnqueueKey(ConsoleKey.Tab, keyChar: '\t');
            driver.EnqueueEnter();

            var options = new InputOptions<double>
            {
                Message = "message",
                DefaultValue = 1.5
            };

            using var form = new InputForm<double>(options, configuration);

            Assert.Equal(1.5, form.Start());
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
        }
    }

    [Fact]
    public void InputForm_DefaultValueAndAnswer_UseInvariantFormatUnderNonInvariantCulture()
    {
        var previousCulture = CultureInfo.CurrentCulture;

        try
        {
            // The hint and the answer must show text that can be typed back as input,
            // which is parsed with the invariant culture.
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");

            var (driver, configuration) = CreateTestContext();

            driver.EnqueueEnter();

            var options = new InputOptions<double>
            {
                Message = "message",
                DefaultValue = 1.5
            };

            using var form = new InputForm<double>(options, configuration);

            Assert.Equal(1.5, form.Start());
            Assert.Contains("(1.5)", driver.Output);
            Assert.Contains("message: 1.5", driver.Output);
            Assert.DoesNotContain("1,5", driver.Output);
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
        }
    }

    [Fact]
    public void InputForm_CustomTypeConverter_ConvertsInputAndDefaultValue()
    {
        var (driver, configuration) = CreateTestContext();

        driver.EnqueueText("30C");
        driver.EnqueueEnter();

        var options = new InputOptions<Temperature>
        {
            Message = "message",
            DefaultValue = new Temperature(21)
        };

        using var form = new InputForm<Temperature>(options, configuration);

        Assert.Equal(new Temperature(30), form.Start());
        Assert.Contains("(21C)", driver.Output);
        Assert.Contains("message: 30C", driver.Output);
    }

    [Fact]
    public void ListForm_Items_UseInvariantFormatUnderNonInvariantCulture()
    {
        var previousCulture = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");

            var (driver, configuration) = CreateTestContext();

            driver.EnqueueText("1.5");
            driver.EnqueueEnter();
            driver.EnqueueEnter();

            var options = new ListOptions<double>
            {
                Message = "message"
            };

            using var form = new ListForm<double>(options, configuration);

            Assert.Equal(new[] { 1.5 }, form.Start());
            Assert.Contains("  1.5", driver.Output);
            Assert.Contains("message: 1.5", driver.Output);
            Assert.DoesNotContain("1,5", driver.Output);
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
        }
    }

    [TypeConverter(typeof(TemperatureConverter))]
    private readonly record struct Temperature(double Celsius);

    private sealed class TemperatureConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
            => value is string text && text.EndsWith('C') ? new Temperature(double.Parse(text[..^1], culture)) : base.ConvertFrom(context, culture, value);

        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
            => value is Temperature temperature && destinationType == typeof(string) ? temperature.Celsius.ToString(culture) + "C" : base.ConvertTo(context, culture, value, destinationType);
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
    public void InputForm_DefaultValueOfCompatibleType_IsConverted()
    {
        var (driver, configuration) = CreateTestContext();

        driver.EnqueueEnter();

        var options = new InputOptions<long>
        {
            Message = "message",
            DefaultValue = 1
        };

        using var form = new InputForm<long>(options, configuration);

        Assert.Equal(1L, form.Start());
    }

    [Fact]
    public void InputForm_IncompatibleDefaultValue_ThrowsArgumentException()
    {
        var (_, configuration) = CreateTestContext();

        var options = new InputOptions<int>
        {
            Message = "message",
            DefaultValue = new object()
        };

        var exception = Assert.Throws<ArgumentException>(() => new InputForm<int>(options, configuration));

        Assert.Equal(nameof(InputOptions<int>.DefaultValue), exception.ParamName);
    }

    [Fact]
    public void InputForm_EmptyStringDefaultValue_RendersNoHintAndReturnsEmpty()
    {
        var (driver, configuration) = CreateTestContext();

        driver.EnqueueEnter();

        var options = new InputOptions<string>
        {
            Message = "message",
            DefaultValue = ""
        };

        using var form = new InputForm<string>(options, configuration);

        // The empty default is still applied on Enter, but no "()" hint is rendered.
        Assert.Equal("", form.Start());
        Assert.DoesNotContain("()", driver.Output);
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
    public void CanceledToken_ThrowsBeforeRendering()
    {
        var (driver, configuration) = CreateTestContext();

        driver.EnqueueText("abc");
        driver.EnqueueEnter();

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        using var form = new InputForm<string>(new InputOptions<string> { Message = "message" }, configuration);

        var exception = Assert.Throws<OperationCanceledException>(() => form.Start(cts.Token));

        Assert.Equal(cts.Token, exception.CancellationToken);
        Assert.Empty(driver.Output);
        Assert.True(driver.KeyAvailable);
    }

    [Fact]
    public void TokenCanceledWhileWaitingForInput_ThrowsOperationCanceledException()
    {
        var (driver, configuration) = CreateTestContext();

        driver.EnqueueText("abc");

        using var cts = new CancellationTokenSource();

        // Cancel only once every queued key has been consumed and the form is waiting for more input.
        driver.KeysExhausted = cts.Cancel;

        using var form = new InputForm<string>(new InputOptions<string> { Message = "message" }, configuration);

        var exception = Assert.Throws<OperationCanceledException>(() => form.Start(cts.Token));

        Assert.Equal(cts.Token, exception.CancellationToken);
        Assert.Equal(Resource.Message_PromptCanceled, exception.Message);
        Assert.False(driver.KeyAvailable);
        Assert.Contains("abc", driver.Output);
    }

    [Fact]
    public void CancelableTokenNotCanceled_ReturnsResult()
    {
        var (driver, configuration) = CreateTestContext();

        driver.EnqueueText("abc");
        driver.EnqueueEnter();

        using var cts = new CancellationTokenSource();

        using var form = new InputForm<string>(new InputOptions<string> { Message = "message" }, configuration);

        Assert.Equal("abc", form.Start(cts.Token));
    }

    [Fact]
    public void DriverOverridingReadKeyWithToken_CancelableToken_ReturnsResultWithoutKeyAvailable()
    {
        var driver = new CancelableConsoleDriver();
        var configuration = new PromptConfiguration { ThrowExceptionOnCancel = true, ConsoleDriverFactory = () => driver };

        driver.Inner.EnqueueText("abc");
        driver.Inner.EnqueueEnter();

        using var cts = new CancellationTokenSource();

        using var form = new InputForm<string>(new InputOptions<string> { Message = "message" }, configuration);

        Assert.Equal("abc", form.Start(cts.Token));
        Assert.Equal(4, driver.ReadKeyWithTokenCount);
    }

    [Fact]
    public void DriverOverridingReadKeyWithToken_CanceledWhileWaiting_CancelsFormAndThrows()
    {
        var driver = new CancelableConsoleDriver();
        var configuration = new PromptConfiguration { ThrowExceptionOnCancel = true, ConsoleDriverFactory = () => driver };

        driver.Inner.EnqueueText("abc");

        using var cts = new CancellationTokenSource();

        driver.Inner.KeysExhausted = cts.Cancel;

        using var form = new InputForm<string>(new InputOptions<string> { Message = "message" }, configuration);

        var exception = Assert.Throws<OperationCanceledException>(() => form.Start(cts.Token));

        Assert.Equal(cts.Token, exception.CancellationToken);
        Assert.Equal(Resource.Message_PromptCanceled, exception.Message);
        Assert.IsType<OperationCanceledException>(exception.InnerException);
        Assert.True(driver.ResetCalled);
    }

    [Fact]
    public void DriverThrowsForDifferentToken_PropagatesExceptionUnchanged()
    {
        var driver = new CancelableConsoleDriver();
        var configuration = new PromptConfiguration { ThrowExceptionOnCancel = true, ConsoleDriverFactory = () => driver };

        using var cts = new CancellationTokenSource();
        using var otherCts = new CancellationTokenSource();

        var thrown = new OperationCanceledException(otherCts.Token);

        driver.CanceledExceptionFactory = _ => thrown;
        driver.Inner.KeysExhausted = cts.Cancel;

        using var form = new InputForm<string>(new InputOptions<string> { Message = "message" }, configuration);

        var exception = Assert.Throws<OperationCanceledException>(() => form.Start(cts.Token));

        Assert.Same(thrown, exception);
        Assert.False(driver.ResetCalled);
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

    [Fact]
    public void SelectForm_CursorBelowWindowInScrollbackBuffer_RedrawsInPlace()
    {
        // Windows conhost: a tall scrollback buffer, with the prompt starting far below the
        // first window's worth of rows. Cursor rows are buffer-relative there.
        var driver = new FakeConsoleDriver { BufferHeight = 9001, WindowHeight = 30, CursorTop = 100 };

        var configuration = new PromptConfiguration
        {
            ThrowExceptionOnCancel = true,
            ConsoleDriverFactory = () => driver
        };

        driver.EnqueueKey(ConsoleKey.DownArrow);
        driver.EnqueueEnter();

        var options = new SelectOptions<string>
        {
            Message = "message",
            Items = ["apple", "banana", "cherry"]
        };

        using var form = new SelectForm<string>(options, configuration);

        Assert.Equal("apple", form.Start());

        // The prompt occupies rows 100-103, so redraws must clear only those rows.
        Assert.All(driver.ClearedLines, top => Assert.InRange(top, 100, 103));
    }

    [Fact]
    public void SelectForm_CursorBeyondShrunkBuffer_ClampsClearedLines()
    {
        // Unix terminals and Windows Terminal: the buffer is the window, so after the window
        // shrinks the previous cursor row can lie past its last row.
        var driver = new FakeConsoleDriver { BufferHeight = 10, WindowHeight = 10, CursorTop = 20 };

        var configuration = new PromptConfiguration
        {
            ThrowExceptionOnCancel = true,
            ConsoleDriverFactory = () => driver
        };

        driver.EnqueueEnter();

        var options = new SelectOptions<string>
        {
            Message = "message",
            Items = ["apple"]
        };

        using var form = new SelectForm<string>(options, configuration);

        Assert.Equal("apple", form.Start());

        Assert.All(driver.ClearedLines, top => Assert.InRange(top, 0, 9));
    }
}
