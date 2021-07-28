using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.ComponentModel.DataAnnotations;
using Sharprompt.Internal;

namespace Sharprompt.Forms
{
    internal class ListForm<T> : FormBase<IEnumerable<T>>
    {
        public ListForm(ListOptions<T> options)
        {
            if (options.Minimum < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(options.Minimum), $"The minimum ({options.Minimum}) is not valid");
            }

            if (options.Maximum < options.Minimum)
            {
                throw new ArgumentException($"The maximum ({options.Maximum}) is not valid when minimum is set to ({options.Minimum})", nameof(options.Maximum));
            }

            _options = options;

            _inputItems.AddRange(options.DefaultValues ?? Enumerable.Empty<T>());

            _paginator = new Paginator<T>(_inputItems, options.PageSize, Optional<T>.Empty, options.TextSelector);
            _paginator.FirstItem();

        }

        private readonly ListOptions<T> _options;

        private readonly Type _targetType = typeof(T);
        private readonly Type _underlyingType = Nullable.GetUnderlyingType(typeof(T));

        private int _startIndex;
        private readonly StringBuilder _inputBuffer = new StringBuilder();
        private readonly List<T> _inputItems = new List<T>();
        private Paginator<T> _paginator;


        protected override bool TryGetResult(CancellationToken cancellationToken,out IEnumerable<T> result)
        {
            do
            {
                var keyInfo = ConsoleDriver.WaitKeypress(cancellationToken);

                switch (keyInfo.Key)
                {
                    case ConsoleKey.Enter when keyInfo.Modifiers == 0:
                    {
                        var input = _inputBuffer.ToString();

                        try
                        {
                            result = _inputItems;

                            if (string.IsNullOrEmpty(input))
                            {
                                if (_inputItems.Count >= _options.Minimum)
                                {
                                    return true;
                                }

                                SetValidationResult(new ValidationResult(string.Format(Prompt.Messages.ListMinSelection, _options.Minimum)));

                                return false;
                            }

                            if (_inputItems.Count >= _options.Maximum)
                            {
                                SetValidationResult(new ValidationResult(string.Format(Prompt.Messages.ListMaxSelection, _options.Maximum)));

                                return false;
                            }


                            var inputValue = (T)Convert.ChangeType(input, _underlyingType ?? _targetType);

                            if (!TryValidate(inputValue, _options.Validators))
                            {
                                result = _inputItems;

                                return false;
                            }
                            if (!_options.AllowDuplicate)
                            {
                                if (_inputItems.Contains(inputValue))
                                {
                                    SetValidationResult(new ValidationResult(Prompt.Messages.ListItemAlreadyexists));
                                    return false;
                                }
                            }

                            _startIndex = 0;
                            _inputBuffer.Clear();

                            _inputItems.Add(inputValue);

                            _paginator = new Paginator<T>(_inputItems, _options.PageSize, Optional<T>.Empty, _options.TextSelector);

                            result = _inputItems;

                            return false;
                        }
                        catch (Exception ex)
                        {
                            SetException(ex);
                        }

                        break;
                    }
                    case ConsoleKey.LeftArrow when keyInfo.Modifiers == 0 &&  _startIndex > 0:
                        _startIndex -= 1;
                        break;
                    case ConsoleKey.RightArrow when keyInfo.Modifiers == 0 &&  _startIndex < _inputBuffer.Length:
                        _startIndex += 1;
                        break;
                    case ConsoleKey.Backspace when keyInfo.Modifiers == 0 &&  _startIndex > 0:
                        _startIndex -= 1;
                        _inputBuffer.Remove(_startIndex, 1);
                        break;
                    case ConsoleKey.Delete when keyInfo.Modifiers == 0 &&  _startIndex < _inputBuffer.Length:
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
                            _paginator = new Paginator<T>(_inputItems, _options.PageSize, Optional<T>.Empty, _options.TextSelector);
                            result = _inputItems;
                            return false;
                        }
                        catch (Exception ex)
                        {
                            SetException(ex);
                        }
                        break;
                    }
                    case ConsoleKey.PageDown when keyInfo.Modifiers == 0:
                        _paginator.PreviousPage();
                        break;
                    case ConsoleKey.PageUp when keyInfo.Modifiers == 0:
                        _paginator.NextPage();
                        break;
                    default:
                    {
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            if (!char.IsControl(keyInfo.KeyChar) && keyInfo.Modifiers == 0)
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
            screenBuffer.WritePrompt(_options.Message);

            var (left, top) = screenBuffer.GetCursorPosition();

            var input = _inputBuffer.ToString();

            screenBuffer.Write(input);

            if (_options.ShowKeyNavigation)
            {
                screenBuffer.WriteLine();
                if (_paginator.PageCount > 1)
                {
                    screenBuffer.Write(Prompt.Messages.KeyNavPaging, Prompt.ColorSchema.KeyNavigation);
                }
                screenBuffer.Write(Prompt.Messages.ListKeyNavigation, Prompt.ColorSchema.KeyNavigation);
            }

            var subset = _paginator.ToSubset();

            foreach (var item in subset)
            {
                var value = _options.TextSelector(item);

                screenBuffer.WriteLine();

                if (EqualityComparer<string>.Default.Equals(_inputBuffer.ToString(), value))
                {
                    screenBuffer.Write($"{Prompt.Symbols.Selected} {value}", Prompt.ColorSchema.Select);
                }
                else
                {
                    screenBuffer.Write($"{Prompt.Symbols.NotSelect} {value}");
                }
            }

            var width = left + input.Take(_startIndex).GetWidth();

            if (_options.ShowPagination)
            {
                if (_paginator.PageCount > 1)
                {
                    screenBuffer.WriteLine();
                    screenBuffer.Write(_paginator.PaginationMessage(_paginator.SelectedCount(_inputBuffer.ToString())), Prompt.ColorSchema.PaginationInfo);
                }
            }
            screenBuffer.SetCursorPosition(width % screenBuffer.BufferWidth, top + (width / screenBuffer.BufferWidth));
        }

        protected override void FinishTemplate(OffscreenBuffer screenBuffer, IEnumerable<T> result)
        {
            screenBuffer.WriteFinish(_options.Message);
            screenBuffer.Write(string.Join(", ", result), Prompt.ColorSchema.Answer);
        }
    }
}
