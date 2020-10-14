using System;
using System.Collections.Generic;
using System.Text;

using Sharprompt.Internal;
using Sharprompt.Validations;

namespace Sharprompt.Forms
{
    internal class Select<T> : FormBase<T>
    {
        public Select(string message, IEnumerable<T> items, int? pageSize, object defaultValue, Func<T, string> valueSelector)
            : base(false)
        {
            _message = message;
            _selector = new Selector<T>(items, pageSize, defaultValue, valueSelector);
            _valueSelector = valueSelector;
        }

        private readonly string _message;
        private readonly Selector<T> _selector;
        private readonly Func<T, string> _valueSelector;

        private readonly StringBuilder _filterBuffer = new StringBuilder();

        protected override bool TryGetResult(out T result)
        {
            var keyInfo = ConsoleDriver.ReadKey();

            if (keyInfo.Key == ConsoleKey.Enter)
            {
                if (_selector.IsSelected)
                {
                    result = _selector.SelectedItem;

                    return true;
                }

                Renderer.SetValidationResult(new ValidationResult("Value is required"));
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
                    ConsoleDriver.Beep();
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

            result = default;

            return false;
        }

        protected override void InputTemplate(ScreenBuffer screenBuffer)
        {
            screenBuffer.WriteMessage(_message);
            screenBuffer.Write(_selector.FilterTerm);

            var subset = _selector.ToSubset();

            foreach (T item in subset)
            {
                var value = _valueSelector(item);

                screenBuffer.WriteLine();

                if (_selector.IsSelected && EqualityComparer<T>.Default.Equals(item, _selector.SelectedItem))
                {
                    screenBuffer.Write($"{Symbol.Selector} {value}", Prompt.ColorSchema.Select);
                }
                else
                {
                    screenBuffer.Write($"  {value}");
                }
            }
        }

        protected override void FinishTemplate(ScreenBuffer screenBuffer, T result)
        {
            screenBuffer.WriteFinishMessage(_message);
            screenBuffer.Write(_valueSelector(result), Prompt.ColorSchema.Answer);
        }
    }
}
