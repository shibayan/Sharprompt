using System;

using Sharprompt.Internal;
using Sharprompt.Strings;

namespace Sharprompt.Forms;

internal class ConfirmForm : FormBase<bool>
{
    public ConfirmForm(ConfirmOptions options)
    {
        options.EnsureOptions();

        _options = options;
    }

    private readonly ConfirmOptions _options;

    private readonly TextInputBuffer _textInputBuffer = new();

    protected override bool TryGetResult(out bool result)
    {
        do
        {
            var keyInfo = ConsoleDriver.ReadKey();

            switch (keyInfo.Key)
            {
                case ConsoleKey.Enter:
                    return HandleEnter(out result);
                case ConsoleKey.LeftArrow when !_textInputBuffer.IsStart:
                    _textInputBuffer.MoveBackward();
                    break;
                case ConsoleKey.RightArrow when !_textInputBuffer.IsEnd:
                    _textInputBuffer.MoveForward();
                    break;
                case ConsoleKey.Backspace when !_textInputBuffer.IsStart:
                    _textInputBuffer.Backspace();
                    break;
                case ConsoleKey.Delete when !_textInputBuffer.IsEnd:
                    _textInputBuffer.Delete();
                    break;
                case ConsoleKey.LeftArrow:
                case ConsoleKey.RightArrow:
                case ConsoleKey.Backspace:
                case ConsoleKey.Delete:
                    ConsoleDriver.Beep();
                    break;
                default:
                    if (!char.IsControl(keyInfo.KeyChar))
                    {
                        _textInputBuffer.Insert(keyInfo.KeyChar);
                    }
                    break;
            }

        } while (ConsoleDriver.KeyAvailable);

        result = default;

        return false;
    }

    protected override void InputTemplate(OffscreenBuffer offscreenBuffer)
    {
        offscreenBuffer.WritePrompt(_options.Message);

        var answerYes = char.ToLowerInvariant(Resource.ConfirmForm_Answer_Yes[0]);
        var answerNo = char.ToLowerInvariant(Resource.ConfirmForm_Answer_No[0]);

        if (_options.DefaultValue.HasValue)
        {
            if (_options.DefaultValue.Value)
            {
                answerYes = char.ToUpperInvariant(answerYes);
            }
            else
            {
                answerNo = char.ToUpperInvariant(answerNo);
            }
        }

        offscreenBuffer.WriteHint($"({answerYes}/{answerNo}) ");
        offscreenBuffer.WriteInput(_textInputBuffer);
    }

    protected override void FinishTemplate(OffscreenBuffer offscreenBuffer, bool result)
    {
        offscreenBuffer.WriteDone(_options.Message);
        offscreenBuffer.WriteAnswer(result ? Resource.ConfirmForm_Answer_Yes : Resource.ConfirmForm_Answer_No);
    }

    private bool HandleEnter(out bool result)
    {
        var input = _textInputBuffer.ToString();

        if (string.IsNullOrEmpty(input))
        {
            if (_options.DefaultValue.HasValue)
            {
                result = _options.DefaultValue.Value;

                return true;
            }

            SetError(Resource.Validation_Required);
        }
        else
        {
            if (input.Equals(Resource.ConfirmForm_Answer_Yes, StringComparison.OrdinalIgnoreCase) ||
                input.Equals(Resource.ConfirmForm_Answer_Yes.Remove(1), StringComparison.OrdinalIgnoreCase))
            {
                result = true;

                return true;
            }

            if (input.Equals(Resource.ConfirmForm_Answer_No, StringComparison.OrdinalIgnoreCase) ||
                input.Equals(Resource.ConfirmForm_Answer_No.Remove(1), StringComparison.OrdinalIgnoreCase))
            {
                result = false;

                return true;
            }

            SetError(Resource.Validation_Invalid);
        }

        result = default;

        return false;
    }
}
