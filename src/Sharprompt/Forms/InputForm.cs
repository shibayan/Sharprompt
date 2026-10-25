using System;
using System.Diagnostics.CodeAnalysis;

using Sharprompt.Internal;
using Sharprompt.Strings;

namespace Sharprompt.Forms;

/// <summary>
/// Reads typed input, applies invariant-culture conversion and validation, and manages an optional editable default.
/// </summary>
/// <typeparam name="T">The result type returned by the prompt or template.</typeparam>
internal class InputForm<T> : TextFormBase<T>
{
    /// <summary>
    /// Validates input settings, converts the optional default, and registers insertion of that default with Tab.
    /// </summary>
    /// <param name="options">The settings to validate and use for this prompt.</param>
    /// <param name="configuration">The symbols, colors, console factory, and cancellation settings to use.</param>
    public InputForm(InputOptions<T> options, PromptConfiguration configuration) : base(configuration)
    {
        options.EnsureOptions();

        _options = options;

        _defaultValue = Optional<T>.Create(options.DefaultValue, nameof(options.DefaultValue));

        KeyHandlerMaps[new ConsoleKeyBinding(ConsoleKey.Tab)] = HandleTab;
    }

    private readonly InputOptions<T> _options;
    private Optional<T> _defaultValue;

    /// <summary>
    /// Writes the prompt, invariant-culture default hint, placeholder, and editable input.
    /// </summary>
    /// <param name="offscreenBuffer">The pending output buffer to write or render.</param>
    protected override void InputTemplate(OffscreenBuffer offscreenBuffer)
    {
        offscreenBuffer.WritePrompt(_options.Message);

        // Show the default value in the same invariant format that Tab inserts and Enter parses.
        // A default value that renders as empty text (e.g. "") is still used on Enter,
        // but showing it would only print empty parentheses.
        var defaultValueText = _defaultValue.HasValue && _defaultValue.Value is not null ? TypeHelper<T>.ConvertToString(_defaultValue.Value) : null;

        if (!string.IsNullOrEmpty(defaultValueText))
        {
            offscreenBuffer.WriteHint($"({defaultValueText}) ");
        }

        if (InputBuffer.Length == 0 && !string.IsNullOrEmpty(_options.Placeholder))
        {
            offscreenBuffer.PushCursor();
            offscreenBuffer.WriteHint(_options.Placeholder);
        }

        offscreenBuffer.WriteInput(InputBuffer);
    }

    /// <summary>
    /// Writes the completed prompt with the invariant-culture representation of the result.
    /// </summary>
    /// <param name="offscreenBuffer">The pending output buffer to write or render.</param>
    /// <param name="result">The accepted result used to render the completed prompt.</param>
    protected override void FinishTemplate(OffscreenBuffer offscreenBuffer, T result)
    {
        offscreenBuffer.WriteDone(_options.Message);

        if (result is not null)
        {
            offscreenBuffer.WriteAnswer(TypeHelper<T>.ConvertToString(result));
        }
    }

    /// <summary>
    /// Converts typed text or accepts the current default, then applies validators before completing the prompt.
    /// </summary>
    /// <param name="result">Receives the accepted result when the method returns true; otherwise it must not be consumed.</param>
    /// <returns>True when the prompt can finish with the result; otherwise false to continue prompting.</returns>
    protected override bool HandleEnter([NotNullWhen(true)] out T? result)
    {
        var input = InputBuffer.ToString();

        try
        {
            if (string.IsNullOrEmpty(input))
            {
                if (!TypeHelper<T>.IsNullable && !_defaultValue.HasValue)
                {
                    SetError(Resource.Validation_Required);

                    result = default;

                    return false;
                }

                result = _defaultValue;
            }
            else
            {
                result = TypeHelper<T>.ConvertTo(input);
            }

            return TryValidate(result, _options.Validators);
        }
        catch (Exception ex)
        {
            SetError(ex);
        }

        result = default;

        return false;
    }

    private bool HandleTab()
    {
        if (!_defaultValue.HasValue || InputBuffer.Length > 0)
        {
            return false;
        }

        // Use the invariant converter so the inserted text round-trips through
        // TypeHelper<T>.ConvertTo regardless of the current culture.
        foreach (var c in TypeHelper<T>.ConvertToString(_defaultValue.Value))
        {
            InputBuffer.Insert(c);
        }

        _defaultValue = Optional<T>.Empty;

        return true;
    }

    /// <summary>
    /// Dismisses an untouched default, otherwise deletes the preceding text element.
    /// </summary>
    /// <returns>True when the key changed or handled the input; otherwise false.</returns>
    protected override bool HandleBackspace()
    {
        // Backspace with nothing typed dismisses the default value: Enter then
        // behaves as if no default value was provided, so nullable types can
        // submit an empty value while non-nullable types still require input.
        if (InputBuffer.Length == 0 && _defaultValue.HasValue)
        {
            _defaultValue = Optional<T>.Empty;

            return true;
        }

        return base.HandleBackspace();
    }
}
