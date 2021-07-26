using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

using Sharprompt.Internal;

namespace Sharprompt.Forms
{
    internal class MultiSelectForm<T> : FormBase<IEnumerable<T>>
    {
        public MultiSelectForm(MultiSelectOptions<T> options)
            : base(false)
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

        private readonly List<T> _selectedItems = new List<T>();
        private readonly StringBuilder _filterBuffer = new StringBuilder();

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
                        SetValidationResult(new ValidationResult($"A minimum selection of {_options.Minimum} items is required"));
                        break;
                    case ConsoleKey.Spacebar when _paginator.TryGetSelectedItem(out var currentItem):
                    {
                        if (_selectedItems.Contains(currentItem))
                        {
                            _selectedItems.Remove(currentItem);
                        }
                        else
                        {
                            if (_selectedItems.Count >= _options.Maximum)
                            {
                                SetValidationResult(new ValidationResult($"A maximum selection of {_options.Maximum} items is required"));
                            }
                            else
                            {
                                _selectedItems.Add(currentItem);
                            }
                        }

                        break;
                    }
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
                    {
                        if (!char.IsControl(keyInfo.KeyChar))
                        {
                            _filterBuffer.Append(keyInfo.KeyChar);

                            _paginator.UpdateFilter(_filterBuffer.ToString());
                        }

                        break;
                    }
                }

            } while (ConsoleDriver.KeyAvailable);

            result = null;

            return false;
        }

        protected override void InputTemplate(OffscreenBuffer screenBuffer)
        {
            screenBuffer.WritePrompt(_options.Message);
            screenBuffer.Write(_paginator.FilterTerm);

            if (string.IsNullOrEmpty(_paginator.FilterTerm))
            {
                screenBuffer.Write(" Hit space to select", Prompt.ColorSchema.Answer);
            }

            var subset = _paginator.ToSubset();

            foreach (var item in subset)
            {
                var value = _options.TextSelector(item);

                screenBuffer.WriteLine();

                if (_paginator.TryGetSelectedItem(out var selectedItem) && EqualityComparer<T>.Default.Equals(item, selectedItem))
                {
                    if (_selectedItems.Contains(item))
                    {
                        screenBuffer.Write($"{Prompt.Symbols.Selector} {Prompt.Symbols.Selected} {value}", Prompt.ColorSchema.Select);
                    }
                    else
                    {
                        screenBuffer.Write($"{Prompt.Symbols.Selector} {Prompt.Symbols.NotSelect} {value}", Prompt.ColorSchema.Select);
                    }
                }
                else
                {
                    if (_selectedItems.Contains(item))
                    {
                        screenBuffer.Write($"  {Prompt.Symbols.Selected} {value}", Prompt.ColorSchema.Select);
                    }
                    else
                    {
                        screenBuffer.Write($"  {Prompt.Symbols.NotSelect} {value}");
                    }
                }
            }

            if (_paginator.PageCount > 1)
            {
                screenBuffer.WriteLine();
                screenBuffer.Write($"({_paginator.TotalCount} items, {_paginator.SelectedPage + 1}/{_paginator.PageCount} pages)");
            }
        }

        protected override void FinishTemplate(OffscreenBuffer screenBuffer, IEnumerable<T> result)
        {
            screenBuffer.WriteFinish(_options.Message);
            screenBuffer.Write(string.Join(", ", result.Select(_options.TextSelector)), Prompt.ColorSchema.Answer);
        }
    }
}
