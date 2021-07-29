using System;
using System.Text;

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

        private readonly StringBuilder _inputBuffer = new();

        protected override bool TryGetResult(out string result)
        {
            do
            {
                var keyInfo = ConsoleDriver.ReadKey();

                switch (keyInfo.Key)
                {
                    case ConsoleKey.Enter:
                        result = _inputBuffer.ToString();

                        if (TryValidate(result, _options.Validators))
                        {
                            return true;
                        }
                        break;
                    case ConsoleKey.Backspace when _inputBuffer.Length == 0:
                        ConsoleDriver.Beep();
                        break;
                    case ConsoleKey.Backspace:
                        _inputBuffer.Length -= 1;
                        break;
                    default:
                        if (!char.IsControl(keyInfo.KeyChar))
                        {
                            _inputBuffer.Append(keyInfo.KeyChar);
                        }
                        break;
                }

            } while (ConsoleDriver.KeyAvailable);

            result = null;

            return false;
        }

        protected override void InputTemplate(OffscreenBuffer offscreenBuffer)
        {
            offscreenBuffer.WritePrompt(_options.Message);
            offscreenBuffer.Write(new string('*', _inputBuffer.Length));

            offscreenBuffer.SetCursorPosition();
        }

        protected override void FinishTemplate(OffscreenBuffer offscreenBuffer, string result)
        {
            offscreenBuffer.WriteFinish(_options.Message);
            offscreenBuffer.Write(new string('*', _inputBuffer.Length), Prompt.ColorSchema.Answer);
        }
    }
}
