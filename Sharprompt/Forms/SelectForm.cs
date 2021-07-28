using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading;

using Sharprompt.Internal;

namespace Sharprompt.Forms
{
    internal class SelectForm<T> : FormBase<T>
    {
        public SelectForm(SelectOptions<T> options): base(true)
        {
            _paginator = new Paginator<T>(options.Items, options.PageSize, Optional<T>.Create(options.DefaultValue), options.TextSelector);
            _paginator.FirstItem();
            _options = options;
        }

        private readonly SelectOptions<T> _options;
        private readonly Paginator<T> _paginator;
        private readonly StringBuilder _filterBuffer = new StringBuilder();

        protected override bool TryGetResult(CancellationToken cancellationToken,out T result)
        {
            do
            {
                var keyInfo = ConsoleDriver.WaitKeypress(cancellationToken);

                switch (keyInfo.Key)
                {
                    case ConsoleKey.Enter when keyInfo.Modifiers == 0 && _paginator.TryGetSelectedItem(out result) :
                        return true;
                    case ConsoleKey.Enter when keyInfo.Modifiers ==  ConsoleModifiers.Control:
                        _paginator.UnSelected();
                        break;
                    case ConsoleKey.Enter when keyInfo.Modifiers == 0:
                    {
                        if (!typeof(T).Name.StartsWith("EnumValue`"))
                        {
                            result = default;
                            return true;
                        }
                        SetValidationResult(new ValidationResult(Prompt.Messages.Required));
                        break;
                    }
                    case ConsoleKey.UpArrow when keyInfo.Modifiers == 0:
                        if (_paginator.IsFistPageItem)
                        {
                            _paginator.PreviousPage();
                            _paginator.LastItem();
                        }
                        else
                        {
                            _paginator.PreviousItem();
                        }
                        break;
                    case ConsoleKey.DownArrow when keyInfo.Modifiers == 0:
                        if (_paginator.IsLastPageItem)
                        {
                            _paginator.NextPage();
                            _paginator.FirstItem();
                        }
                        else
                        {
                            _paginator.NextItem();
                        }
                        break;
                    case ConsoleKey.PageUp when keyInfo.Modifiers == 0:
                        _paginator.PreviousPage();
                        break;
                    case ConsoleKey.PageDown when keyInfo.Modifiers == 0:
                        _paginator.NextPage();
                        break;
                    case ConsoleKey.Backspace when keyInfo.Modifiers == 0:
                        if (_filterBuffer.Length > 0)
                        {
                            _filterBuffer.Length -= 1;
                            _paginator.UpdateFilter(_filterBuffer.ToString());
                        }
                        break;
                    default:
                    {
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            if (!char.IsControl(keyInfo.KeyChar) && keyInfo.Modifiers == 0)
                            {
                                _filterBuffer.Append(keyInfo.KeyChar);
                                _paginator.UpdateFilter(_filterBuffer.ToString());
                            }
                        }
                        break;
                    }
                }

            } while (ConsoleDriver.KeyAvailable && !cancellationToken.IsCancellationRequested);

            result = default;

            return false;
        }

        protected override void InputTemplate(OffscreenBuffer screenBuffer)
        {
            screenBuffer.WritePrompt(_options.Message);
            if (_paginator.IsUnSelected)
            {
                screenBuffer.Write(_filterBuffer.ToString());
            }
            var (left, top) = screenBuffer.GetCursorPosition();

            if (_paginator.TryGetSelectedItem(out var result))
            {
                screenBuffer.Write(_options.TextSelector(result), Prompt.ColorSchema.Answer);
            }

            if (_options.ShowKeyNavigation)
            {
                screenBuffer.WriteLine();
                if (_paginator.PageCount > 1)
                {
                    screenBuffer.Write(Prompt.Messages.KeyNavPaging, Prompt.ColorSchema.KeyNavigation);
                }
                screenBuffer.Write(Prompt.Messages.SelectKeyNavigation, Prompt.ColorSchema.KeyNavigation);
                if (_filterBuffer.Length > 0)
                {
                    screenBuffer.WriteLine();
                    screenBuffer.Write(Prompt.Messages.ItemsFiltered, Prompt.ColorSchema.Warnning);
                }
            }

            var subset = _paginator.ToSubset();

            foreach (var item in subset)
            {
                var value = _options.TextSelector(item);

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

            if (_options.ShowPagination)
            {
                if (_paginator.PageCount > 1)
                {
                    screenBuffer.WriteLine();
                    screenBuffer.Write(_paginator.PaginationMessage(),Prompt.ColorSchema.PaginationInfo);
                }
            }
            screenBuffer.SetCursorPosition(left, top);
        }

        protected override void FinishTemplate(OffscreenBuffer screenBuffer, T result)
        {
            screenBuffer.WriteFinish(_options.Message);
            screenBuffer.Write(_options.TextSelector(result), Prompt.ColorSchema.Answer);
        }
    }
}
