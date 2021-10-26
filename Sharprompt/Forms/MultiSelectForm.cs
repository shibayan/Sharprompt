using System;
using System.Collections.Generic;
using System.Linq;

using Sharprompt.Internal;

namespace Sharprompt.Forms
{
    internal class MultiSelectForm<T> : FormBase<IEnumerable<T>>
    {
        public MultiSelectForm(MultiSelectOptions<T> options)
        {
            if (options.Items is null)
            {
                throw new ArgumentNullException(nameof(options.Items));
            }

            if (options.Minimum < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(options.Minimum), $"The minimum ({options.Minimum}) is not valid");
            }

            if (options.Maximum < options.Minimum)
            {
                throw new ArgumentException($"The maximum ({options.Maximum}) is not valid when minimum is set to ({options.Minimum})", nameof(options.Maximum));
            }

            _paginator = new Paginator<T>(options.Items, options.PageSize, Optional<T>.Empty, options.TextSelector);

            if (options.DefaultValues is not null)
            {
                _selectedItems.AddRange(options.DefaultValues);
            }

            _options = options;
        }

        private readonly MultiSelectOptions<T> _options;
        private readonly Paginator<T> _paginator;

        private readonly List<T> _selectedItems = new();
        private readonly TextInputBuffer _filterBuffer = new();

        protected override bool TryGetResult(out IEnumerable<T> result)
        {
            do
            {
                var keyInfo = ConsoleDriver.ReadKey();

                switch (keyInfo.Key)
                {
                    case ConsoleKey.Enter when _selectedItems.Count >= _options.Minimum:
                        result = _options.Items
                                         .Where(x => _selectedItems.Contains(x))
                                         .ToArray();
                        return true;
                    case ConsoleKey.Enter:
                        SetError($"A minimum selection of {_options.Minimum} items is required");
                        break;
                    case ConsoleKey.Spacebar when _paginator.TryGetSelectedItem(out var currentItem):
                        if (_selectedItems.Contains(currentItem))
                        {
                            _selectedItems.Remove(currentItem);
                        }
                        else
                        {
                            if (_selectedItems.Count >= _options.Maximum)
                            {
                                SetError($"A maximum selection of {_options.Maximum} items is required");
                            }
                            else
                            {
                                _selectedItems.Add(currentItem);
                            }
                        }

                        break;
                    case ConsoleKey.UpArrow:
                        _paginator.PreviousItem();
                        break;
                    case ConsoleKey.DownArrow:
                        _paginator.NextItem();
                        break;
                    case ConsoleKey.LeftArrow:
                        _paginator.PreviousPage();
                        break;
                    case ConsoleKey.RightArrow:
                        _paginator.NextPage();
                        break;
                    case ConsoleKey.Backspace when !_filterBuffer.IsStart:
                        _filterBuffer.Backspace();

                        _paginator.UpdateFilter(_filterBuffer.ToString());
                        break;
                    case ConsoleKey.Backspace:
                        ConsoleDriver.Beep();
                        break;
                    default:
                        if (!char.IsControl(keyInfo.KeyChar))
                        {
                            _filterBuffer.Insert(keyInfo.KeyChar);

                            _paginator.UpdateFilter(_filterBuffer.ToString());
                        }
                        break;
                }

            } while (ConsoleDriver.KeyAvailable);

            result = default;

            return false;
        }

        protected override void InputTemplate(OffscreenBuffer offscreenBuffer)
        {
            offscreenBuffer.WritePrompt(_options.Message);
            offscreenBuffer.Write(_paginator.FilterTerm);

            offscreenBuffer.PushCursor();

            if (string.IsNullOrEmpty(_paginator.FilterTerm))
            {
                offscreenBuffer.WriteHint("Hit <space> key to select");
            }

            var subset = _paginator.ToSubset();

            foreach (var item in subset)
            {
                var value = _options.TextSelector(item);

                offscreenBuffer.WriteLine();

                if (_paginator.TryGetSelectedItem(out var selectedItem) && EqualityComparer<T>.Default.Equals(item, selectedItem))
                {
                    if (_selectedItems.Contains(item))
                    {
                        offscreenBuffer.WriteSelect($"{Prompt.Symbols.Selector} {Prompt.Symbols.Selected} {value}");
                    }
                    else
                    {
                        offscreenBuffer.WriteSelect($"{Prompt.Symbols.Selector} {Prompt.Symbols.NotSelect} {value}");
                    }
                }
                else
                {
                    if (_selectedItems.Contains(item))
                    {
                        offscreenBuffer.WriteSelect($"  {Prompt.Symbols.Selected} {value}");
                    }
                    else
                    {
                        offscreenBuffer.Write($"  {Prompt.Symbols.NotSelect} {value}");
                    }
                }
            }

            if (_paginator.PageCount > 1 && _options.Pagination != null)
            {
                offscreenBuffer.WriteLine();
                offscreenBuffer.WriteHint(_options.Pagination(_paginator.TotalCount, _paginator.SelectedPage + 1, _paginator.PageCount));
            }
        }

        protected override void FinishTemplate(OffscreenBuffer offscreenBuffer, IEnumerable<T> result)
        {
            offscreenBuffer.WriteDone(_options.Message);
            offscreenBuffer.WriteAnswer(string.Join(", ", result.Select(_options.TextSelector)));
        }
    }
}
