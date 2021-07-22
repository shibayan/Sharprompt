﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading;

using Sharprompt.Internal;

namespace Sharprompt.Forms
{
    internal class InputForm<T> : FormBase<T>
    {
        public InputForm(InputOptions options)
        {
            _defaultValue = Optional<T>.Create(options.DefaultValue);
            _options = options;
            _initform = true;
        }

        private readonly InputOptions _options;
        private readonly Optional<T> _defaultValue;

        private readonly Type _targetType = typeof(T);
        private readonly Type _underlyingType = Nullable.GetUnderlyingType(typeof(T));

        private int _startIndex;
        private readonly StringBuilder _inputBuffer = new StringBuilder();
        private bool _initform; 
        protected override bool TryGetResult(CancellationToken cancellationToken, out T result)
        {
            do
            {
                ConsoleKeyInfo keyInfo;
                while (!ConsoleDriver.KeyAvailable && !cancellationToken.IsCancellationRequested)
                {
                    Thread.Sleep(Prompt.DefaultMessageValues.IdleReadKey);
                }
                if (ConsoleDriver.KeyAvailable && !cancellationToken.IsCancellationRequested)
                {
                    keyInfo = ConsoleDriver.ReadKey();
                }

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
                                    Renderer.SetValidationResult(new ValidationResult(Prompt.DefaultMessageValues.DefaultRequiredMessage));

                                    result = default;

                                    return false;
                                }

                                result = _defaultValue;
                            }
                            else
                            {
                                result = (T)Convert.ChangeType(input, _underlyingType ?? _targetType);
                            }

                            if (!TryValidate(result, _options.Validators))
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
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            if (!char.IsControl(keyInfo.KeyChar))
                            {
                                _inputBuffer.Insert(_startIndex, keyInfo.KeyChar);

                                _startIndex += 1;
                            }
                        }
                        break;
                    }
                }

            } while (ConsoleDriver.KeyAvailable && !cancellationToken.IsCancellationRequested);

            result = default;

            return false;
        }

        protected override void InputTemplate(OffscreenBuffer screenBuffer)
        {
            var prompt = _options.Message;
            if (_defaultValue.HasValue)
            {
                if (_initform)
                {
                    _inputBuffer.Append(_defaultValue.Value);
                }
                prompt = $"{_options.Message} ({_defaultValue.Value}) ";
            }

            screenBuffer.WritePrompt(prompt);

            var (left, top) = screenBuffer.GetCursorPosition();

            var input = _inputBuffer.ToString();

            screenBuffer.Write(input);

            if (_initform)
            {
                _startIndex = input.Length;
            }

            var width = EastAsianWidth.GetWidth(input.Take(_startIndex)) + left;

            screenBuffer.SetCursorPosition(width % screenBuffer.BufferWidth,  top + (width / screenBuffer.BufferWidth));

            _initform = false;
        }

        protected override void FinishTemplate(OffscreenBuffer screenBuffer, T result)
        {
            screenBuffer.WriteFinish(_options.Message);
            if (result != null)
            {
                screenBuffer.Write(result.ToString(), Prompt.ColorSchema.Answer);
            }
        }
    }
}
