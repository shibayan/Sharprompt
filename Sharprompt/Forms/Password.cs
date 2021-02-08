using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

using Sharprompt.Internal;

namespace Sharprompt.Forms
{
    internal class Password : FormBase<string>
    {
        public Password(string message, IList<Func<object, ValidationResult>> validators)
        {
            _message = message;
            _validators = validators;
        }

        private readonly string _message;
        private readonly IList<Func<object, ValidationResult>> _validators;

        private readonly StringBuilder _inputBuffer = new StringBuilder();

        protected override bool TryGetResult(out string result)
        {
            var keyInfo = ConsoleDriver.ReadKey();

            switch (keyInfo.Key)
            {
                case ConsoleKey.Enter:
                {
                    result = _inputBuffer.ToString();

                    if (TryValidate(result, _validators))
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
                    if (!char.IsControl(keyInfo.KeyChar))
                    {
                        _inputBuffer.Append(keyInfo.KeyChar);
                    }

                    break;
                }
            }

            result = null;

            return false;
        }

        protected override void InputTemplate(ScreenBuffer screenBuffer)
        {
            screenBuffer.WritePrompt(_message);
            screenBuffer.Write(new string('*', _inputBuffer.Length));

            screenBuffer.SetCursorPosition();
        }

        protected override void FinishTemplate(ScreenBuffer screenBuffer, string result)
        {
            screenBuffer.WriteFinish(_message);
            screenBuffer.Write(new string('*', _inputBuffer.Length), Prompt.ColorSchema.Answer);
        }
    }
}
