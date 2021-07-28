﻿using System;
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
                    case ConsoleKey.Enter when keyInfo.Modifiers == 0:
                    {
                        result = _inputBuffer.ToString();

                        if (TryValidate(result, _options.Validators))
                        {
                            return true;
                        }

                        break;
                    }
                    case ConsoleKey.Backspace when keyInfo.Modifiers == 0:
                        if (_inputBuffer.Length > 0)
                        {
                            _inputBuffer.Length -= 1;
                        }
                        break;
                    default:
                    {
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            if (!char.IsControl(keyInfo.KeyChar) && keyInfo.Modifiers == 0)
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
            screenBuffer.Write(new string(Prompt.Messages.PasswordChar, _inputBuffer.Length));

            screenBuffer.SetCursorPosition();
        }

        protected override void FinishTemplate(OffscreenBuffer screenBuffer, string result)
        {
            screenBuffer.WriteFinish(_options.Message);
            screenBuffer.Write(new string(Prompt.Messages.PasswordChar, _inputBuffer.Length), Prompt.ColorSchema.Answer);
        }
    }
}
