using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Sharprompt.Internal;

namespace Sharprompt.Forms;

/// <summary>
/// Reads and validates a password while rendering only its configured mask.
/// </summary>
internal class PasswordForm : FormBase<string>
{
    /// <summary>
    /// Validates password settings and creates the prompt with masked input and backspace support.
    /// </summary>
    /// <param name="options">The settings to validate and use for this prompt.</param>
    /// <param name="configuration">The symbols, colors, console factory, and cancellation settings to use.</param>
    public PasswordForm(PasswordOptions options, PromptConfiguration configuration) : base(configuration)
    {
        options.EnsureOptions();

        _options = options;

        KeyHandlerMaps[new ConsoleKeyBinding(ConsoleKey.Backspace)] = HandleBackspace;
    }

    private readonly PasswordOptions _options;

    /// <summary>
    /// Writes the prompt and placeholder or mask without displaying password characters.
    /// </summary>
    /// <param name="offscreenBuffer">The pending output buffer to write or render.</param>
    protected override void InputTemplate(OffscreenBuffer offscreenBuffer)
    {
        offscreenBuffer.WritePrompt(_options.Message);

        if (InputBuffer.Length == 0 && !string.IsNullOrEmpty(_options.Placeholder))
        {
            offscreenBuffer.PushCursor();
            offscreenBuffer.WriteHint(_options.Placeholder);
        }

        if (!string.IsNullOrEmpty(_options.PasswordChar))
        {
            offscreenBuffer.Write(string.Concat(Enumerable.Repeat(_options.PasswordChar, InputBuffer.Length)));
            offscreenBuffer.PushCursor();
        }
    }

    /// <summary>
    /// Writes the completed prompt with a mask rather than the accepted password.
    /// </summary>
    /// <param name="offscreenBuffer">The pending output buffer to write or render.</param>
    /// <param name="result">The accepted password; only its mask is rendered.</param>
    protected override void FinishTemplate(OffscreenBuffer offscreenBuffer, string result)
    {
        offscreenBuffer.WriteDone(_options.Message);

        if (!string.IsNullOrEmpty(_options.PasswordChar))
        {
            offscreenBuffer.WriteAnswer(string.Concat(Enumerable.Repeat(_options.PasswordChar, InputBuffer.Length)));
        }
    }

    /// <summary>
    /// Validates the password and clears rejected input before retrying.
    /// </summary>
    /// <param name="result">Receives the accepted result when the method returns true; otherwise it must not be consumed.</param>
    /// <returns>True when the prompt can finish with the result; otherwise false to continue prompting.</returns>
    protected override bool HandleEnter([NotNullWhen(true)] out string? result)
    {
        result = InputBuffer.ToString();

        if (!TryValidate(result, _options.Validators))
        {
            InputBuffer.Clear();

            return false;
        }

        return true;
    }

    private bool HandleBackspace()
    {
        if (InputBuffer.IsStart)
        {
            return false;
        }

        InputBuffer.Backspace();

        return true;
    }
}
