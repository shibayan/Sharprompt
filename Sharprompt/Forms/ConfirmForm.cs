using System;
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
        private readonly StringBuilder _inputBuffer = new();

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

                        break;
                    }
                    case ConsoleKey.LeftArrow when _startIndex > 0:
                        _startIndex -= 1;
                        break;
                    case ConsoleKey.RightArrow when _startIndex < _inputBuffer.Length:
                        _startIndex += 1;
                        break;
                    case ConsoleKey.Backspace when _startIndex > 0:
                        _inputBuffer.Remove(--_startIndex, 1);
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
                        if (!char.IsControl(keyInfo.KeyChar))
                        {
                            _inputBuffer.Insert(_startIndex++, keyInfo.KeyChar);
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

            if (_options.DefaultValue == null)
            {
                offscreenBuffer.Write("(y/n) ");
            }
            else
            {
                offscreenBuffer.Write(_options.DefaultValue.Value ? "(Y/n) " : "(y/N) ");
            }

            offscreenBuffer.Write(_inputBuffer.ToString(0, _startIndex));

            offscreenBuffer.PushCursor();

            offscreenBuffer.Write(_inputBuffer.ToString(_startIndex, _inputBuffer.Length - _startIndex));
        }

        protected override void FinishTemplate(OffscreenBuffer offscreenBuffer, bool result)
        {
            offscreenBuffer.WriteFinish(_options.Message);
            offscreenBuffer.Write(result ? "Yes" : "No", Prompt.ColorSchema.Answer);
        }
    }
}
