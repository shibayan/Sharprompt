using System;

using Sharprompt.Internal;
using Sharprompt.Strings;

namespace Sharprompt.Forms;

internal class InputForm<T> : FormBase<T>
{
    public InputForm(InputOptions<T> options)
    {
        options.EnsureOptions();

        _options = options;

        _defaultValue = Optional<T>.Create(options.DefaultValue);
    }

    private readonly InputOptions<T> _options;
    private readonly Optional<T> _defaultValue;

    private readonly TextInputBuffer _textInputBuffer = new();

    protected override bool TryGetResult(out T result)
    {
        do
        {
            var keyInfo = ConsoleDriver.ReadKey();
            var controlPressed = keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control);

            switch (keyInfo.Key)
            {
                case ConsoleKey.Enter:
                    return HandleEnter(out result);
                case ConsoleKey.LeftArrow when controlPressed && !_textInputBuffer.IsStart:
                    _textInputBuffer.MoveToPreviousWord();
                    break;
                case ConsoleKey.RightArrow when controlPressed && !_textInputBuffer.IsEnd:
                    _textInputBuffer.MoveToNextWord();
                    break;
                case ConsoleKey.LeftArrow when !_textInputBuffer.IsStart:
                    _textInputBuffer.MoveBackward();
                    break;
                case ConsoleKey.RightArrow when !_textInputBuffer.IsEnd:
                    _textInputBuffer.MoveForward();
                    break;
                case ConsoleKey.Home when !_textInputBuffer.IsStart:
                    _textInputBuffer.MoveToStart();
                    break;
                case ConsoleKey.End when !_textInputBuffer.IsEnd:
                    _textInputBuffer.MoveToEnd();
                    break;
                case ConsoleKey.Backspace when controlPressed && !_textInputBuffer.IsStart:
                    _textInputBuffer.BackspaceWord();
                    break;
                case ConsoleKey.Delete when controlPressed && !_textInputBuffer.IsEnd:
                    _textInputBuffer.DeleteWord();
                    break;
                case ConsoleKey.Backspace when !_textInputBuffer.IsStart:
                    _textInputBuffer.Backspace();
                    break;
                case ConsoleKey.Delete when !_textInputBuffer.IsEnd:
                    _textInputBuffer.Delete();
                    break;
                case ConsoleKey.LeftArrow:
                case ConsoleKey.RightArrow:
                case ConsoleKey.Home:
                case ConsoleKey.End:
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

        if (_defaultValue.HasValue)
        {
            offscreenBuffer.WriteHint($"({_defaultValue.Value}) ");
        }

        if (_textInputBuffer.Length == 0 && !string.IsNullOrEmpty(_options.Placeholder))
        {
            offscreenBuffer.PushCursor();
            offscreenBuffer.WriteHint(_options.Placeholder);
        }

        offscreenBuffer.WriteInput(_textInputBuffer);
    }

    protected override void FinishTemplate(OffscreenBuffer offscreenBuffer, T result)
    {
        offscreenBuffer.WriteDone(_options.Message);

        if (result is not null)
        {
            offscreenBuffer.WriteAnswer(result.ToString());
        }
    }

    private bool HandleEnter(out T result)
    {
        var input = _textInputBuffer.ToString();

        try
        {
            if (string.IsNullOrEmpty(input))
            {
                if (TypeHelper<T>.IsValueType && !_defaultValue.HasValue)
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

            if (TryValidate(result, _options.Validators))
            {
                return true;
            }

        }
        catch (Exception ex)
        {
            SetError(ex);
        }

        result = default;

        return false;
    }
}
