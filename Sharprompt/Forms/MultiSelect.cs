using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

using Sharprompt.Internal;

namespace Sharprompt.Forms
{
    internal class MultiSelect<T> : FormBase<IEnumerable<T>>
    {
        public MultiSelect(string message, IEnumerable<T> items, int? pageSize, int minimum, int maximum, Func<T, string> textSelector)
            : base(false)
        {
            // throw early when invalid options are passed
            if (minimum < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minimum), $"The minimum ({minimum}) is not valid");
            }

            if (maximum != -1 && maximum < minimum)
            {
                throw new ArgumentException($"The maximum ({maximum}) is not valid when minimum is set to ({minimum})", nameof(maximum));
            }

            _message = message;
            _paginator = new Paginator<T>(items, pageSize, Optional<T>.Empty, textSelector);
            _minimum = minimum;
            _maximum = maximum;
            _textSelector = textSelector;
        }

        private readonly string _message;
        private readonly Paginator<T> _paginator;
        private readonly int _minimum;
        private readonly int _maximum;
        private readonly Func<T, string> _textSelector;

        private readonly IList<T> _selectedItems = new List<T>();
        private readonly StringBuilder _filterBuffer = new StringBuilder();

        protected override bool TryGetResult(out IEnumerable<T> result)
        {
            var keyInfo = ConsoleDriver.ReadKey();

            switch (keyInfo.Key)
            {
                case ConsoleKey.Enter when _selectedItems.Count >= _minimum:
                    result = _selectedItems;
                    return true;
                case ConsoleKey.Enter:
                    Renderer.SetValidationResult(new ValidationResult($"A minimum selection of {_minimum} items is required"));
                    break;
                case ConsoleKey.Spacebar when _paginator.TryGetSelectedItem(out var currentItem):
                {
                    if (_selectedItems.Contains(currentItem))
                    {
                        _selectedItems.Remove(currentItem);
                    }
                    else
                    {
                        _selectedItems.Add(currentItem);
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

            result = null;

            return false;
        }

        protected override void InputTemplate(ScreenBuffer screenBuffer)
        {
            screenBuffer.WritePrompt(_message);
            screenBuffer.Write(_paginator.FilterTerm);

            if (string.IsNullOrEmpty(_paginator.FilterTerm))
            {
                screenBuffer.Write(" Hit space to select", Prompt.ColorSchema.Answer);
            }

            var subset = _paginator.ToSubset();

            foreach (var item in subset)
            {
                var value = _textSelector(item);

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
        }

        protected override void FinishTemplate(ScreenBuffer screenBuffer, IEnumerable<T> result)
        {
            screenBuffer.WriteFinish(_message);
            screenBuffer.Write(string.Join(", ", result.Select(_textSelector)), Prompt.ColorSchema.Answer);
        }
    }
}
