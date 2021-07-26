using System;
using System.Text;
using System.Threading;

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

        private readonly StringBuilder _inputBuffer = new StringBuilder();

        protected override bool TryGetResult(CancellationToken cancellationToken,out string result)
        {
            do
            {
                var keyInfo = ConsoleDriver.WaitKeypress(cancellationToken);
                switch (keyInfo.Key)
                {
                    case ConsoleKey.Enter:
                    {
                        result = _inputBuffer.ToString();

                        if (TryValidate(result, _options.Validators))
                        {
                            return true;
                        }

                        break;
                    }
                    case ConsoleKey.Backspace when _inputBuffer.Length == 0:
                        ConsoleDriver.Beep();
                        break;
                    case ConsoleKey.Backspace:
                        _inputBuffer.Length -= 1;
                        break;
                    default:
                    {
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            if (!char.IsControl(keyInfo.KeyChar))
                            {
                                _inputBuffer.Append(keyInfo.KeyChar);
                            }
                        }
                        break;
                    }
                }

            } while (ConsoleDriver.KeyAvailable && !cancellationToken.IsCancellationRequested);

            result = null;

            return false;
        }

        protected override void InputTemplate(OffscreenBuffer screenBuffer)
        {
            screenBuffer.WritePrompt(_options.Message);
            screenBuffer.Write(new string(Prompt.DefaultMessageValues.DefautPasswordChar, _inputBuffer.Length));

            screenBuffer.SetCursorPosition();
        }

        protected override void FinishTemplate(OffscreenBuffer screenBuffer, string result)
        {
            screenBuffer.WriteFinish(_options.Message);
            screenBuffer.Write(new string(Prompt.DefaultMessageValues.DefautPasswordChar, _inputBuffer.Length), Prompt.ColorSchema.Answer);
        }
    }
}
