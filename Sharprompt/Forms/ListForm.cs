﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Sharprompt.Internal;

namespace Sharprompt.Forms
{
    internal class ListForm<T> : FormBase<IEnumerable<T>>
    {
        public ListForm(ListOptions<T> options)
        {
            _options = options;

            _inputItems.AddRange(options.DefaultValues ?? Enumerable.Empty<T>());
        }

        private readonly ListOptions<T> _options;

        private readonly Type _targetType = typeof(T);
        private readonly Type _underlyingType = Nullable.GetUnderlyingType(typeof(T));

        private int _startIndex;
        private readonly StringBuilder _inputBuffer = new StringBuilder();
        private readonly List<T> _inputItems = new List<T>();

        protected override bool TryGetResult(CancellationToken cancellationToken,out IEnumerable<T> result)
        {
            do
            {
                var keyInfo = ConsoleDriver.WaitKeypress(cancellationToken);

                switch (keyInfo.Key)
                {
                    case ConsoleKey.Enter:
                    {
                        var input = _inputBuffer.ToString();

                        try
                        {
                            if (string.IsNullOrEmpty(input))
                            {
                                result = _inputItems;

                                return true;
                            }

                            var inputValue = (T)Convert.ChangeType(input, _underlyingType ?? _targetType);

                            if (!TryValidate(inputValue, _options.Validators))
                            {
                                result = _inputItems;

                                return false;
                            }

                            _startIndex = 0;
                            _inputBuffer.Clear();

                            _inputItems.Add(inputValue);

                            result = _inputItems;

                            return false;
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
                    case ConsoleKey.Delete when keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control):
                    {
                        var input = _inputBuffer.ToString();
                        try
                        {
                            if (string.IsNullOrEmpty(input))
                            {
                                result = _inputItems;

                                return true;
                            }

                            var inputValue = (T)Convert.ChangeType(input, _underlyingType ?? _targetType);

                            if (!TryValidate(inputValue, _options.Validators))
                            {
                                result = _inputItems;

                                return false;
                            }

                            _startIndex = 0;
                            _inputBuffer.Clear();
                            if (_options.RemoveAllMatch)
                            {
                                _inputItems.RemoveAll(x => x.Equals(inputValue));
                            }
                            else
                            {
                                if (_inputItems.Contains(inputValue))
                                {
                                    _inputItems.Remove(inputValue);
                                }
                            }
                            result = _inputItems;
                            return false;
                        }
                        catch (Exception ex)
                        {
                            Renderer.SetException(ex);
                        }
                        break;
                    }
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

            result = null;

            return false;
        }

        protected override void InputTemplate(OffscreenBuffer screenBuffer)
        {
            screenBuffer.WritePrompt($"{_options.Message} Ctrl-Del Remove");
            screenBuffer.Write($"({string.Join(", ", _inputItems)}) ", Prompt.ColorSchema.Answer);

            var (left, top) = screenBuffer.GetCursorPosition();

            var input = _inputBuffer.ToString();

            screenBuffer.Write(input);

            var width = EastAsianWidth.GetWidth(input.Take(_startIndex)) + left;


            screenBuffer.SetCursorPosition(width % screenBuffer.BufferWidth, top + (width / screenBuffer.BufferWidth));
        }

        protected override void FinishTemplate(OffscreenBuffer screenBuffer, IEnumerable<T> result)
        {
            screenBuffer.WriteFinish(_options.Message);
            screenBuffer.Write(string.Join(", ", result), Prompt.ColorSchema.Answer);
        }
    }
}
