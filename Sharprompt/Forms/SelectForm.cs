using System;
using System.Collections.Generic;
using System.Text;

using Sharprompt.Internal;

namespace Sharprompt.Forms
{
    internal class SelectForm<T> : FormBase<T>
    {
        public SelectForm(SelectOptions<T> options)
        {
            _paginator = new Paginator<T>(options.Items, options.PageSize, Optional<T>.Create(options.DefaultValue), options.TextSelector);

            _options = options;
        }

        private readonly SelectOptions<T> _options;
        private readonly Paginator<T> _paginator;

        private readonly StringBuilder _filterBuffer = new();

        protected override bool TryGetResult(out T result)
        {
            do
            {
                var keyInfo = ConsoleDriver.ReadKey();

                switch (keyInfo.Key)
                {
                    case ConsoleKey.Enter when _paginator.TryGetSelectedItem(out result):
                        return true;
                    case ConsoleKey.Enter:
                        SetError("Value is required");
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

            result = default;

            return false;
        }

        protected override void InputTemplate(OffscreenBuffer offscreenBuffer)
        {
            offscreenBuffer.WritePrompt(_options.Message);
            offscreenBuffer.Write(_paginator.FilterTerm);

            offscreenBuffer.PushCursor();

            var subset = _paginator.ToSubset();

            foreach (var item in subset)
            {
                var value = _options.TextSelector(item);

                offscreenBuffer.WriteLine();

                if (_paginator.TryGetSelectedItem(out var selectedItem) && EqualityComparer<T>.Default.Equals(item, selectedItem))
                {
                    offscreenBuffer.Write($"{Prompt.Symbols.Selector} {value}", Prompt.ColorSchema.Select);
                }
                else
                {
                    offscreenBuffer.Write($"  {value}");
                }
            }

            if (_paginator.PageCount > 1)
            {
                offscreenBuffer.WriteLine();
                offscreenBuffer.Write($"({_paginator.TotalCount} items, {_paginator.SelectedPage + 1}/{_paginator.PageCount} pages)");
            }
        }

        protected override void FinishTemplate(OffscreenBuffer offscreenBuffer, T result)
        {
            offscreenBuffer.WriteFinish(_options.Message);
            offscreenBuffer.Write(_options.TextSelector(result), Prompt.ColorSchema.Answer);
        }
    }
}
