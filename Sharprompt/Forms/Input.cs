using System;
using System.Collections.Generic;
using System.Text;

using Sharprompt.Validations;

namespace Sharprompt.Forms
{
    internal class Input<T> : FormBase<T>
    {
        public Input(string message, object defaultValue, IList<Func<object, ValidationResult>> validators)
        {
            _message = message;
            _defaultValue = defaultValue;
            _validators = validators;
        }

        private readonly string _message;
        private readonly object _defaultValue;
        private readonly IList<Func<object, ValidationResult>> _validators;

        private readonly Type _targetType = typeof(T);
        private readonly Type _underlyingType = Nullable.GetUnderlyingType(typeof(T));

        private int _startIndex;
        private readonly StringBuilder _inputBuffer = new StringBuilder();

        protected override bool TryGetResult(out T result)
        {
            var keyInfo = Renderer.ReadKey();

            if (keyInfo.Key == ConsoleKey.Enter)
            {
                var input = _inputBuffer.ToString();

                if (!TryValidate(input, _validators))
                {
                    result = default;

                    return false;
                }

                if (string.IsNullOrEmpty(input))
                {
                    if (_targetType.IsValueType && _underlyingType == null && _defaultValue == null)
                    {
                        Renderer.SetValidationResult(new ValidationResult("Value is required"));

                        result = default;

                        return false;
                    }

                    result = (T)_defaultValue;

                    return true;
                }

                try
                {
                    result = (T)Convert.ChangeType(input, _underlyingType ?? _targetType);

                    return true;
                }
                catch (Exception ex)
                {
                    Renderer.SetException(ex);
                }
            }
            else if (keyInfo.Key == ConsoleKey.LeftArrow)
            {
                if (_startIndex > 0)
                {
                    _startIndex -= 1;
                }
                else
                {
                    Renderer.Beep();
                }
            }
            else if (keyInfo.Key == ConsoleKey.RightArrow)
            {
                if (_startIndex < _inputBuffer.Length)
                {
                    _startIndex += 1;
                }
                else
                {
                    Renderer.Beep();
                }
            }
            else if (keyInfo.Key == ConsoleKey.Backspace)
            {
                if (_startIndex > 0)
                {
                    _startIndex -= 1;

                    _inputBuffer.Remove(_startIndex, 1);
                }
                else
                {
                    Renderer.Beep();
                }
            }
            else if (keyInfo.Key == ConsoleKey.Delete)
            {
                if (_startIndex < _inputBuffer.Length)
                {
                    _inputBuffer.Remove(_startIndex, 1);
                }
                else
                {
                    Renderer.Beep();
                }
            }
            else if (!char.IsControl(keyInfo.KeyChar))
            {
                _inputBuffer.Insert(_startIndex, keyInfo.KeyChar);

                _startIndex += 1;
            }

            result = default;

            return false;
        }

        protected override void InputTemplate(FormRenderer formRenderer)
        {
            formRenderer.WriteMessage(_message);

            if (_defaultValue != null)
            {
                formRenderer.Write($"({_defaultValue}) ");
            }

            var (left, top) = Renderer.GetCursorPosition();

            formRenderer.Write(_inputBuffer.ToString());

            Renderer.SetCursorPosition(left + _startIndex, top);
        }

        protected override void FinishTemplate(FormRenderer formRenderer, T result)
        {
            formRenderer.WriteFinishMessage(_message);

            if (result != null)
            {
                formRenderer.Write(result.ToString(), Prompt.ColorSchema.Answer);
            }
        }
    }
}
