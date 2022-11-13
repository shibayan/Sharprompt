using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Sharprompt.Internal;

namespace Sharprompt.Forms;

internal class PasswordForm : FormBase<string>
{
    public PasswordForm(PasswordOptions options)
    {
        options.EnsureOptions();

        _options = options;

        KeyHandlerMaps = new()
        {
            [ConsoleKey.Backspace] = HandleBackspace
        };
    }

    private readonly PasswordOptions _options;

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

    protected override void FinishTemplate(OffscreenBuffer offscreenBuffer, string result)
    {
        offscreenBuffer.WriteDone(_options.Message);

        if (!string.IsNullOrEmpty(_options.PasswordChar))
        {
            offscreenBuffer.WriteAnswer(string.Concat(Enumerable.Repeat(_options.PasswordChar, InputBuffer.Length)));
        }
    }

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

    private bool HandleBackspace(ConsoleKeyInfo keyInfo)
    {
        if (InputBuffer.IsStart)
        {
            return false;
        }

        InputBuffer.Backspace();

        return true;
    }
}
