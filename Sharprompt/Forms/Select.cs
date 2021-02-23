using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

using Sharprompt.Internal;

namespace Sharprompt.Forms
{
    internal class Select<T> : FormBase<T>
    {
        public Select(string message, IEnumerable<T> items, int? pageSize, Optional<T> defaultValue, Func<T, string> valueSelector)
            : base(false)
        {
            _message = message;
            _paginator = new Paginator<T>(items, pageSize, defaultValue, valueSelector);
            _valueSelector = valueSelector;
        }

        private readonly string _message;
        private readonly Paginator<T> _paginator;
        private readonly Func<T, string> _valueSelector;

        private readonly StringBuilder _filterBuffer = new StringBuilder();

        protected override bool TryGetResult(out T result)
        {
            var keyInfo = ConsoleDriver.ReadKey();

            switch (keyInfo.Key)
            {
                case ConsoleKey.Enter when _paginator.TryGetSelectedItem(out result):
                    return true;
                case ConsoleKey.Enter:
                    Renderer.SetValidationResult(new ValidationResult("Value is required"));
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
                {
                    if (!char.IsControl(keyInfo.KeyChar))
                    {
                        _filterBuffer.Append(keyInfo.KeyChar);

                        _paginator.UpdateFilter(_filterBuffer.ToString());
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
            screenBuffer.Write(_paginator.FilterTerm);

            var subset = _paginator.ToSubset();

            foreach (var item in subset)
            {
                var value = _valueSelector(item);

                screenBuffer.WriteLine();

                if (_paginator.TryGetSelectedItem(out var selectedItem) && EqualityComparer<T>.Default.Equals(item, selectedItem))
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
