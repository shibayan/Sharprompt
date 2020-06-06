using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sharprompt.Drivers;
using Sharprompt.Internal;

namespace Sharprompt.Forms
{
    internal class MultiSelect<T> : FormBase<IEnumerable<T>>
    {
        public MultiSelect(string message, IEnumerable<T> options, int minimum, int maximum, int pageSize, Func<T, string> valueSelector)
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
            _selector = new Selector<T>(options.ToArray(), pageSize, null, valueSelector);
            _minimum = minimum;
            _maximum = maximum;
            _valueSelector = valueSelector;
        }

        private readonly string _message;
        private readonly Selector<T> _selector;
        private readonly int _minimum;
        private readonly int _maximum;
        private readonly Func<T, string> _valueSelector;

        private readonly IList<T> _selectedItems = new List<T>();
        private readonly StringBuilder _filterBuffer = new StringBuilder();

        private bool _showConfirm;

        protected override bool TryGetResult(out IEnumerable<T> result)
        {
            var keyInfo = Renderer.ReadKey();

            if (keyInfo.Key == ConsoleKey.Enter)
            {
                var currentItem = _selector.CurrentItem;

                if (currentItem == null)
                {
                    result = null;

                    return false;
                }

                if (_selectedItems.Contains(currentItem))
                {
                    _selectedItems.Remove(currentItem);
                }
                else
                {
                    _selectedItems.Add(currentItem);
                }

                // If we have reached the limit, determine which items should not be selected anymore
                if (_maximum == _selectedItems.Count)
                {
                    _showConfirm = true;
                }
                else
                {
                    _showConfirm = _selectedItems.Count >= _minimum;
                }
            }
            else if (keyInfo.Key == ConsoleKey.Tab)
            {
                if (_selectedItems.Count >= _minimum)
                {
                    result = _selectedItems;

                    return true;
                }

                Renderer.SetError(new ValidationError($"A minimum selection of {_minimum} items is required"));
            }
            else if (keyInfo.Key == ConsoleKey.UpArrow)
            {
                _selector.PreviousItem();
            }
            else if (keyInfo.Key == ConsoleKey.DownArrow)
            {
                _selector.NextItem();
            }
            else if (keyInfo.Key == ConsoleKey.LeftArrow)
            {
                _selector.PreviousPage();
            }
            else if (keyInfo.Key == ConsoleKey.RightArrow)
            {
                _selector.NextPage();
            }
            else if (keyInfo.Key == ConsoleKey.Backspace)
            {
                if (_filterBuffer.Length == 0)
                {
                    Renderer.Beep();
                }
                else
                {
                    _filterBuffer.Length -= 1;

                    _selector.UpdateFilter(_filterBuffer.ToString());
                }
            }
            else if (!char.IsControl(keyInfo.KeyChar))
            {
                _filterBuffer.Append(keyInfo.KeyChar);

                _selector.UpdateFilter(_filterBuffer.ToString());
            }

            result = null;

            return false;
        }

        protected override void InputTemplate(IConsoleDriver consoleDriver)
        {
            consoleDriver.WriteMessage(_message);
            consoleDriver.Write(_selector.FilterTerm);

            if (_showConfirm && string.IsNullOrEmpty(_selector.FilterTerm))
            {
                consoleDriver.Write(" Press Tab to confirm", Prompt.ColorSchema.Answer);
            }

            var subset = _selector.ToSubset();

            foreach (T item in subset)
            {
                var value = _valueSelector(item);

                consoleDriver.WriteLine();

                if (EqualityComparer<T>.Default.Equals(item, _selector.CurrentItem))
                {
                    if (_selectedItems.Contains(item))
                    {
                        consoleDriver.Write($"> [x] {value}", Prompt.ColorSchema.Select);
                    }
                    else
                    {
                        consoleDriver.Write($"> [ ] {value}", Prompt.ColorSchema.Select);
                    }
                }
                else if (_selectedItems.Contains(item))
                {
                    consoleDriver.Write($"  [x] {value}", Prompt.ColorSchema.Select);
                }
                else
                {
                    consoleDriver.Write($"  [ ] {value}");
                }
            }
        }

        protected override void FinishTemplate(IConsoleDriver consoleDriver, IEnumerable<T> result)
        {
            consoleDriver.WriteMessage(_message);
            consoleDriver.Write(result.Select(_valueSelector).Join(", "), Prompt.ColorSchema.Answer);
        }
    }
}
