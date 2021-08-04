using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sharprompt.Internal;

namespace Sharprompt.Forms
{
    internal class MultiSelectForm<T> : FormBase<IEnumerable<T>>
    {
        public MultiSelectForm(MultiSelectOptions<T> options)
        {
            if (options.Minimum < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(options.Minimum), $"The minimum ({options.Minimum}) is not valid");
            }

            if (options.Maximum < options.Minimum)
            {
                throw new ArgumentException($"The maximum ({options.Maximum}) is not valid when minimum is set to ({options.Minimum})", nameof(options.Maximum));
            }

            _paginator = new Paginator<T>(options.Items, options.PageSize, Optional<T>.Empty, options.TextSelector);

            if (options.DefaultValues != null)
            {
                _selectedItems.AddRange(options.DefaultValues);
            }

            _options = options;
        }

        private readonly MultiSelectOptions<T> _options;
        private readonly Paginator<T> _paginator;

        private readonly List<T> _selectedItems = new();
        private readonly StringBuilder _filterBuffer = new();

        protected override bool TryGetResult(out IEnumerable<T> result)
        {
            do
            {
                var keyInfo = ConsoleDriver.ReadKey();

                switch (keyInfo.Key)
                {
                    case ConsoleKey.Enter when _selectedItems.Count >= _options.Minimum:
                        result = _selectedItems;
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
                    case ConsoleKey.Backspace when _filterBuffer.Length == 0:
                        ConsoleDriver.Beep();
                        break;
                    case ConsoleKey.Backspace:
                        _filterBuffer.Length -= 1;

                        _paginator.UpdateFilter(_filterBuffer.ToString());
                        break;
                    default:
                        if (!char.IsControl(keyInfo.KeyChar))
                        {
                            _filterBuffer.Append(keyInfo.KeyChar);

                            _paginator.UpdateFilter(_filterBuffer.ToString());
                        }
                        break;
                }

            } while (ConsoleDriver.KeyAvailable);

            result = null;

            return false;
        }

        protected override void InputTemplate(OffscreenBuffer offscreenBuffer)
        {
            offscreenBuffer.WritePrompt(_options.Message);
            offscreenBuffer.Write(_paginator.FilterTerm);

            offscreenBuffer.PushCursor();

            if (string.IsNullOrEmpty(_paginator.FilterTerm))
            {
                offscreenBuffer.Write("Hit space to select", Prompt.ColorSchema.Hint);
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
                        offscreenBuffer.Write($"{Prompt.Symbols.Selector} {Prompt.Symbols.Selected} {value}", Prompt.ColorSchema.Select);
                    }
                    else
                    {
                        offscreenBuffer.Write($"{Prompt.Symbols.Selector} {Prompt.Symbols.NotSelect} {value}", Prompt.ColorSchema.Select);
                    }
                }
                else
                {
                    if (_selectedItems.Contains(item))
                    {
                        offscreenBuffer.Write($"  {Prompt.Symbols.Selected} {value}", Prompt.ColorSchema.Select);
                    }
                    else
                    {
                        offscreenBuffer.Write($"  {Prompt.Symbols.NotSelect} {value}");
                    }
                }
            }

            if (_paginator.PageCount > 1)
            {
                offscreenBuffer.WriteLine();
                offscreenBuffer.Write($"({_paginator.TotalCount} items, {_paginator.SelectedPage + 1}/{_paginator.PageCount} pages)", Prompt.ColorSchema.Hint);
            }
        }

        protected override void FinishTemplate(OffscreenBuffer offscreenBuffer, IEnumerable<T> result)
        {
            offscreenBuffer.WriteFinish(_options.Message);
            offscreenBuffer.Write(string.Join(", ", result.Select(_options.TextSelector)), Prompt.ColorSchema.Answer);
        }
    }
}
