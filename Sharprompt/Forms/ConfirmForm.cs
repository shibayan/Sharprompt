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
            do
            {
                var keyInfo = ConsoleDriver.ReadKey();

                switch (keyInfo.Key)
                {
                    case ConsoleKey.Enter:
                    {
                        var input = _textInputBuffer.ToString();

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
                    case ConsoleKey.LeftArrow when !_textInputBuffer.IsStart:
                        _textInputBuffer.Backward();
                        break;
                    case ConsoleKey.RightArrow when !_textInputBuffer.IsEnd:
                        _textInputBuffer.Forward();
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

            if (_options.DefaultValue == null)
            {
                offscreenBuffer.WriteHint("(y/n) ");
            }
            else
            {
                offscreenBuffer.WriteHint(_options.DefaultValue.Value ? "(Y/n) " : "(y/N) ");
            }

            offscreenBuffer.Write(_textInputBuffer.ToBackwardString());

            offscreenBuffer.PushCursor();

            offscreenBuffer.Write(_textInputBuffer.ToForwardString());
        }

        protected override void FinishTemplate(OffscreenBuffer offscreenBuffer, bool result)
        {
            offscreenBuffer.WriteDone(_options.Message);
            offscreenBuffer.WriteAnswer(result ? "Yes" : "No");
        }
    }
}
