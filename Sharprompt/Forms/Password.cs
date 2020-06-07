using System;
using System.Collections.Generic;
using System.Text;

using Sharprompt.Validations;

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
            var keyInfo = Renderer.ReadKey();

            if (keyInfo.Key == ConsoleKey.Enter)
            {
                result = _inputBuffer.ToString();

                if (TryValidate(result, _validators))
                {
                    return true;
                }

                _inputBuffer.Clear();
            }
            else if (keyInfo.Key == ConsoleKey.Backspace)
            {
                if (_inputBuffer.Length == 0)
                {
                    Renderer.Beep();
                }
                else
                {
                    _inputBuffer.Length -= 1;
                }
            }
            else if (!char.IsControl(keyInfo.KeyChar))
            {
                _inputBuffer.Append(keyInfo.KeyChar);
            }

            result = null;

            return false;
        }

        protected override void InputTemplate(FormRenderer formRenderer)
        {
            formRenderer.WriteMessage(_message);
            formRenderer.Write(new string('*', _inputBuffer.Length));
        }
    }
}
