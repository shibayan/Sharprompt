﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sharprompt.Internal;
using Sharprompt.Validations;

namespace Sharprompt.Forms
{
    internal class MultiSelect<T> : FormBase<IEnumerable<T>>
    {
        public MultiSelect(string message, IEnumerable<T> items, int? pageSize, int minimum, int maximum, Func<T, string> valueSelector)
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
            _selector = new Selector<T>(items, pageSize, null, valueSelector);
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

        protected override bool TryGetResult(out IEnumerable<T> result)
        {
            var keyInfo = Renderer.ReadKey();

            if (keyInfo.Key == ConsoleKey.Enter)
            {
                if (_selectedItems.Count >= _minimum)
                {
                    result = _selectedItems;

                    return true;
                }

                Renderer.SetValidationResult(new ValidationResult($"A minimum selection of {_minimum} items is required"));
            }
            else if (keyInfo.Key == ConsoleKey.Spacebar)
            {
                var currentItem = _selector.SelectedItem;

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

        protected override void InputTemplate(FormRenderer formRenderer)
        {
            formRenderer.WriteMessage(_message);
            formRenderer.Write(_selector.FilterTerm);

            if (string.IsNullOrEmpty(_selector.FilterTerm))
            {
                formRenderer.Write(" Hit space to select", Prompt.ColorSchema.Answer);
            }

            var subset = _selector.ToSubset();

            foreach (T item in subset)
            {
                var value = _valueSelector(item);

                formRenderer.WriteLine();

                if (_selector.IsSelected && EqualityComparer<T>.Default.Equals(item, _selector.SelectedItem))
                {
                    if (_selectedItems.Contains(item))
                    {
                        formRenderer.Write($"{Symbol.Selector} {Symbol.Selected} {value}", Prompt.ColorSchema.Select);
                    }
                    else
                    {
                        formRenderer.Write($"{Symbol.Selector} {Symbol.NotSelect} {value}", Prompt.ColorSchema.Select);
                    }
                }
                else
                {
                    if (_selectedItems.Contains(item))
                    {
                        formRenderer.Write($"  {Symbol.Selected} {value}", Prompt.ColorSchema.Select);
                    }
                    else
                    {
                        formRenderer.Write($"  {Symbol.NotSelect} {value}");
                    }
                }
            }
        }

        protected override void FinishTemplate(FormRenderer formRenderer, IEnumerable<T> result)
        {
            formRenderer.WriteFinishMessage(_message);
            formRenderer.Write(result.Select(_valueSelector).Join(", "), Prompt.ColorSchema.Answer);
        }
    }
}
