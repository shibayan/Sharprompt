using System;

using Sharprompt.Internal;

namespace Sharprompt.Forms
{
    internal class ConfirmForm : FormBase<bool>
    {
        public ConfirmForm(ConfirmOptions options)
        {
            _options = options;
        }

        private readonly ConfirmOptions _options;

        private readonly TextInputBuffer _textInputBuffer = new();

        protected override bool TryGetResult(out bool result)
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

            if (_options.DefaultValue.HasValue)
            {
                offscreenBuffer.WriteHint(_options.DefaultValue.Value ? "(Y/n) " : "(y/N) ");
            }
            else
            {
                offscreenBuffer.WriteHint("(y/n) ");
            }

            offscreenBuffer.WriteInput(_textInputBuffer);
        }

        protected override void FinishTemplate(OffscreenBuffer offscreenBuffer, bool result)
        {
            offscreenBuffer.WriteDone(_options.Message);
            offscreenBuffer.WriteAnswer(result ? "Yes" : "No");
        }

        private bool HandleEnter(ref bool result)
        {
            var input = _textInputBuffer.ToString();

            if (string.IsNullOrEmpty(input))
            {
                if (_options.DefaultValue.HasValue)
                {
                    result = _options.DefaultValue.Value;

                    return true;
                }

                SetError("Value is required");
            }
            else
            {
                switch (input.ToLowerInvariant())
                {
                    case "y" or "yes":
                        result = true;

                        return true;
                    case "n" or "no":
                        result = false;

                        return true;
                    default:
                        SetError("Value is invalid");
                        break;
                }
            }

            return false;
        }
    }
}
