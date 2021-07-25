using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

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

        private int _startIndex;
        private readonly StringBuilder _inputBuffer = new StringBuilder();

        protected override bool TryGetResult(out bool result)
        {
            do
            {
                var keyInfo = ConsoleDriver.ReadKey();

                switch (keyInfo.Key)
                {
                    case ConsoleKey.Enter:
                    {
                        var input = _inputBuffer.ToString();

                        if (string.IsNullOrEmpty(input))
                        {
                            if (_options.DefaultValue != null)
                            {
                                result = _options.DefaultValue.Value;

                                return true;
                            }

                            SetValidationResult(new ValidationResult("Value is required"));
                        }
                        else
                        {
                            var lowerInput = input.ToLower();

                            if (lowerInput == "y" || lowerInput == "yes")
                            {
                                result = true;

                                return true;
                            }

                            if (lowerInput == "n" || lowerInput == "no")
                            {
                                result = false;

                                return true;
                            }

                            SetValidationResult(new ValidationResult("Value is invalid"));
                        }

                        break;
                    }
                    case ConsoleKey.LeftArrow when _startIndex > 0:
                        _startIndex -= 1;
                        break;
                    case ConsoleKey.RightArrow when _startIndex < _inputBuffer.Length:
                        _startIndex += 1;
                        break;
                    case ConsoleKey.Backspace when _startIndex > 0:
                        _startIndex -= 1;

                        _inputBuffer.Remove(_startIndex, 1);
                        break;
                    case ConsoleKey.Delete when _startIndex < _inputBuffer.Length:
                        _inputBuffer.Remove(_startIndex, 1);
                        break;
                    case ConsoleKey.LeftArrow:
                    case ConsoleKey.RightArrow:
                    case ConsoleKey.Backspace:
                    case ConsoleKey.Delete:
                        ConsoleDriver.Beep();
                        break;
                    default:
                    {
                        if (!char.IsControl(keyInfo.KeyChar))
                        {
                            _inputBuffer.Insert(_startIndex, keyInfo.KeyChar);

                            _startIndex += 1;
                        }

                        break;
                    }
                }

            } while (ConsoleDriver.KeyAvailable);

            result = default;

            return false;
        }

        protected override void InputTemplate(OffscreenBuffer screenBuffer)
        {
            screenBuffer.WritePrompt(_options.Message);

            if (_options.DefaultValue == null)
            {
                screenBuffer.Write("(y/n) ");
            }
            else if (_options.DefaultValue.Value)
            {
                screenBuffer.Write("(Y/n) ");
            }
            else
            {
                screenBuffer.Write("(y/N) ");
            }

            var (left, top) = screenBuffer.GetCursorPosition();

            var input = _inputBuffer.ToString();

            screenBuffer.Write(input);

            var width = left + input.Take(_startIndex).GetWidth();

            screenBuffer.SetCursorPosition(width % screenBuffer.BufferWidth, top + (width / screenBuffer.BufferWidth));
        }

        protected override void FinishTemplate(OffscreenBuffer screenBuffer, bool result)
        {
            screenBuffer.WriteFinish(_options.Message);
            screenBuffer.Write(result ? "Yes" : "No", Prompt.ColorSchema.Answer);
        }
    }
}
