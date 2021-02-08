using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

using Sharprompt.Internal;

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

            switch (keyInfo.Key)
            {
                case ConsoleKey.Enter when _selector.TryGetSelectedItem(out result):
                    return true;
                case ConsoleKey.Enter:
                    Renderer.SetValidationResult(new ValidationResult("Value is required"));
                    break;
                case ConsoleKey.UpArrow:
                    _selector.PreviousItem();
                    break;
                case ConsoleKey.DownArrow:
                    _selector.NextItem();
                    break;
                case ConsoleKey.LeftArrow:
                    _selector.PreviousPage();
                    break;
                case ConsoleKey.RightArrow:
                    _selector.NextPage();
                    break;
                case ConsoleKey.Backspace when _filterBuffer.Length == 0:
                    ConsoleDriver.Beep();
                    break;
                case ConsoleKey.Backspace:
                    _filterBuffer.Length -= 1;

                    _selector.UpdateFilter(_filterBuffer.ToString());
                    break;
                default:
                {
                    if (!char.IsControl(keyInfo.KeyChar))
                    {
                        _filterBuffer.Append(keyInfo.KeyChar);

                        _selector.UpdateFilter(_filterBuffer.ToString());
                    }

                    break;
                }
            }

            result = default;

            return false;
        }

        protected override void InputTemplate(ScreenBuffer screenBuffer)
        {
            screenBuffer.WritePrompt(_message);
            screenBuffer.Write(_selector.FilterTerm);

            var subset = _selector.ToSubset();

            foreach (T item in subset)
            {
                var value = _valueSelector(item);

                screenBuffer.WriteLine();

                if (_selector.TryGetSelectedItem(out var selectedItem) && EqualityComparer<T>.Default.Equals(item, selectedItem))
                {
                    screenBuffer.Write($"{Prompt.Symbols.Selector} {value}", Prompt.ColorSchema.Select);
                }
                else
                {
                    screenBuffer.Write($"  {value}");
                }
            }
        }

        protected override void FinishTemplate(ScreenBuffer screenBuffer, T result)
        {
            screenBuffer.WriteFinish(_message);
            screenBuffer.Write(_valueSelector(result), Prompt.ColorSchema.Answer);
        }
    }
}
