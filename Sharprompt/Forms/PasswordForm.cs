using System;

using Sharprompt.Internal;

namespace Sharprompt.Forms
{
    internal class PasswordForm : FormBase<string>
    {
        public PasswordForm(PasswordOptions options)
        {
            _options = options;
        }

        private readonly PasswordOptions _options;

        private readonly TextInputBuffer _textInputBuffer = new();

        protected override bool TryGetResult(out string result)
        {
            do
            {
                var keyInfo = ConsoleDriver.ReadKey();

                switch (keyInfo.Key)
                {
                    case ConsoleKey.Enter:
                        result = _textInputBuffer.ToString();

                        if (TryValidate(result, _options.Validators))
                        {
                            return true;
                        }
                        break;
                    case ConsoleKey.Backspace when !_textInputBuffer.IsStart:
                        _textInputBuffer.Backspace();
                        break;
                    case ConsoleKey.Backspace:
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

            if (_options.PasswordChar.HasValue)
            {
                offscreenBuffer.Write(new string(_options.PasswordChar.Value, _textInputBuffer.Length));
            }

            offscreenBuffer.PushCursor();
        }

        protected override void FinishTemplate(OffscreenBuffer offscreenBuffer, string result)
        {
            offscreenBuffer.WriteDone(_options.Message);

            if (_options.PasswordChar.HasValue)
            {
                offscreenBuffer.WriteAnswer(new string(_options.PasswordChar.Value, _textInputBuffer.Length));
            }
        }
    }
}
