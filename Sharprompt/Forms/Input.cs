using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

using Sharprompt.Internal;

namespace Sharprompt.Forms
{
    internal class Input<T> : FormBase<T>
    {
        public Input(string message, Optional<T> defaultValue, IList<Func<object, ValidationResult>> validators)
        {
            _message = message;
            _defaultValue = defaultValue;
            _validators = validators;
        }

        private readonly string _message;
        private readonly Optional<T> _defaultValue;
        private readonly IList<Func<object, ValidationResult>> _validators;

        private readonly Type _targetType = typeof(T);
        private readonly Type _underlyingType = Nullable.GetUnderlyingType(typeof(T));

        private int _startIndex;
        private readonly StringBuilder _inputBuffer = new StringBuilder();

        protected override bool TryGetResult(out T result)
        {
            var keyInfo = ConsoleDriver.ReadKey();

            switch (keyInfo.Key)
            {
                case ConsoleKey.Enter:
                {
                    var input = _inputBuffer.ToString();

                    try
                    {
                        if (string.IsNullOrEmpty(input))
                        {
                            if (_targetType.IsValueType && _underlyingType == null && !_defaultValue.HasValue)
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

                    break;
                }
                case ConsoleKey.LeftArrow when _startIndex > 0:
                    _startIndex -= 1;
                    break;
                case ConsoleKey.LeftArrow:
                    ConsoleDriver.Beep();
                    break;
                case ConsoleKey.RightArrow when _startIndex < _inputBuffer.Length:
                    _startIndex += 1;
                    break;
                case ConsoleKey.RightArrow:
                    ConsoleDriver.Beep();
                    break;
                case ConsoleKey.Backspace when _startIndex > 0:
                    _startIndex -= 1;

                    _inputBuffer.Remove(_startIndex, 1);
                    break;
                case ConsoleKey.Backspace:
                    ConsoleDriver.Beep();
                    break;
                case ConsoleKey.Delete when _startIndex < _inputBuffer.Length:
                    _inputBuffer.Remove(_startIndex, 1);
                    break;
                case ConsoleKey.Delete:
                    ConsoleDriver.Beep();
                    break;
                default:
                {
                    if (!char.IsControl(keyInfo.KeyChar))
                    {
                        _inputBuffer.Insert(_startIndex, keyInfo.KeyChar);

                        _startIndex += 1;
                    }

                    break;
                }
            }

            result = default;

            return false;
        }

        protected override void InputTemplate(OffscreenBuffer screenBuffer)
        {
            screenBuffer.WritePrompt(_message);

            if (_defaultValue.HasValue)
            {
                screenBuffer.Write($"({_defaultValue.Value}) ");
            }

            var (left, top) = screenBuffer.GetCursorPosition();

            var input = _inputBuffer.ToString();

            screenBuffer.Write(input);

            var width = EastAsianWidth.GetWidth(input.Take(_startIndex)) + left;

            screenBuffer.SetCursorPosition(width % screenBuffer.BufferWidth, top + (width / screenBuffer.BufferWidth));
        }

        protected override void FinishTemplate(OffscreenBuffer screenBuffer, T result)
        {
            screenBuffer.WriteFinish(_message);

            if (result != null)
            {
                screenBuffer.Write(result.ToString(), Prompt.ColorSchema.Answer);
            }
        }
    }
}
