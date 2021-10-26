using System;

using Sharprompt.Internal;

namespace Sharprompt.Forms
{
    internal class InputForm<T> : FormBase<T>
    {
        public InputForm(InputOptions options)
        {
            _defaultValue = Optional<T>.Create(options.DefaultValue);

            _options = options;
        }

        private readonly InputOptions _options;
        private readonly Optional<T> _defaultValue;

        private readonly TextInputBuffer _textInputBuffer = new();

        protected override bool TryGetResult(out T result)
        {
            result = default;

            do
            {
                var keyInfo = ConsoleDriver.ReadKey();

                switch (keyInfo.Key)
                {
                    case ConsoleKey.Enter:
                        return HandleEnter(ref result);
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

            return false;
        }

        protected override void InputTemplate(OffscreenBuffer offscreenBuffer)
        {
            offscreenBuffer.WritePrompt(_options.Message);

            if (_defaultValue.HasValue)
            {
                offscreenBuffer.WriteHint($"({_defaultValue.Value}) ");
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

        private bool HandleEnter(ref T result)
        {
            var input = _textInputBuffer.ToString();

            try
            {
                if (string.IsNullOrEmpty(input))
                {
                    if (TypeHelper<T>.IsValueType && !_defaultValue.HasValue)
                    {
                        SetError("Value is required");

                        result = default;

                        return false;
                    }

                    result = _defaultValue;
                }
                else
                {
                    result = TypeHelper<T>.ConvertTo(input);
                }

                if (!TryValidate(result, _options.Validators))
                {
                    result = default;

                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                SetError(ex);
            }

            return false;
        }
    }
}
