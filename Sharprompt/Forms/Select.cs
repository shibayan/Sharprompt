using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sharprompt.Internal;

namespace Sharprompt.Forms
{
    internal class Select<T> : FormBase<T>
    {
        public Select(string message, IEnumerable<T> options, T defaultValue, int pageSize, Func<T, string> valueSelector)
            : base(false)
        {
            _message = message;
            _selector = new Selector<T>(options.ToArray(), pageSize, defaultValue, valueSelector);
            _valueSelector = valueSelector;
        }

        private readonly string _message;
        private readonly Selector<T> _selector;
        private readonly Func<T, string> _valueSelector;

        private readonly StringBuilder _filterBuffer = new StringBuilder();

        protected override bool TryGetResult(out T result)
        {
            var keyInfo = Scope.ReadKey();

            if (keyInfo.Key == ConsoleKey.Enter)
            {
                if (_selector.CurrentItem != null)
                {
                    result = _selector.CurrentItem;

                    return true;
                }

                Scope.SetError(new ValidationError("Value is required"));
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
                    Scope.Beep();
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

        protected override void InputTemplate(IConsoleRenderer consoleRenderer)
        {
            consoleRenderer.WriteMessage(_message);
            consoleRenderer.Write(_selector.FilterTerm);

            var subset = _selector.ToSubset();

            foreach (T item in subset)
            {
                var value = _valueSelector(item);

                consoleRenderer.WriteLine();

                if (EqualityComparer<T>.Default.Equals(item, _selector.CurrentItem))
                {
                    consoleRenderer.Write($"> {value}", Prompt.ColorSchema.Select);
                }
                else
                {
                    consoleRenderer.Write($"  {value}");
                }
            }
        }

        protected override void FinishTemplate(IConsoleRenderer consoleRenderer, T result)
        {
            consoleRenderer.WriteMessage(_message);
            consoleRenderer.Write(_valueSelector(result), Prompt.ColorSchema.Answer);
        }
    }
}
