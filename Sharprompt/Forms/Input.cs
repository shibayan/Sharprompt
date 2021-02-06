using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

using Sharprompt.Internal;

namespace Sharprompt.Forms
{
    internal class Input<T> : FormBase<T> where T : notnull
    {
        public Input(string message, T? defaultValue, IReadOnlyList<Func<object?, ValidationResult>> validators)
        {
            _message = message;
            _defaultValue = defaultValue;
            _validators = validators;
        }

        private readonly string _message;
        private readonly T? _defaultValue;
        private readonly IReadOnlyList<Func<object?, ValidationResult>> _validators;

        private readonly Type _targetType = typeof(T);
        private readonly Type? _underlyingType = Nullable.GetUnderlyingType(typeof(T));

        private int _startIndex;
        private readonly StringBuilder _inputBuffer = new();

        protected override bool TryGetResult([NotNullWhen(true)] out T? result)
        {
            var keyInfo = ConsoleDriver.ReadKey();

            if (keyInfo.Key == ConsoleKey.Enter)
            {
                var input = _inputBuffer.ToString();

                try
                {
                    if (string.IsNullOrEmpty(input))
                    {
                        if (_targetType.IsValueType && _underlyingType == null && _defaultValue == null)
                        {
                            Renderer.SetValidationResult(new ValidationResult("Value is required"));

                            result = default;

                            return false;
                        }

                        result = _defaultValue;
                    }
                    else
                    {
                        result = (T)Convert.ChangeType(input, _underlyingType ?? _targetType);
                    }

                    if (!TryValidate(result, _validators))
                    {
                        result = default;

                        return false;
                    }

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
                    ConsoleDriver.Beep();
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
                    ConsoleDriver.Beep();
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
                    ConsoleDriver.Beep();
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
                    ConsoleDriver.Beep();
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

        protected override void InputTemplate(ScreenBuffer screenBuffer)
        {
            screenBuffer.WritePrompt(_message);

            if (_defaultValue != null)
            {
                screenBuffer.Write($"({_defaultValue}) ");
            }

            var (left, top) = screenBuffer.GetCursorPosition();

            var input = _inputBuffer.ToString();

            screenBuffer.Write(input);

            var width = EastAsianWidth.GetWidth(input.Take(_startIndex)) + left;

            screenBuffer.SetCursorPosition(width % screenBuffer.BufferWidth, top + (width / screenBuffer.BufferWidth));
        }

        protected override void FinishTemplate(ScreenBuffer screenBuffer, T result)
        {
            screenBuffer.WriteFinish(_message);
            screenBuffer.Write(result.ToString(), Prompt.ColorSchema.Answer);
        }
    }
}
