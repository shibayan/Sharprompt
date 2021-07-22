﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading;

using Sharprompt.Internal;

namespace Sharprompt.Forms
{
    internal class SelectForm<T> : FormBase<T>
    {
        public SelectForm(SelectOptions<T> options)
            : base(false)
        {
            _paginator = new Paginator<T>(options.Items, options.PageSize, Optional<T>.Create(options.DefaultValue), options.TextSelector);

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
                    case ConsoleKey.Enter when _paginator.TryGetSelectedItem(out result):
                        return true;
                    case ConsoleKey.Enter:
                        Renderer.SetValidationResult(new ValidationResult(Prompt.DefaultMessageValues.DefaultRequiredMessage));
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
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            if (!char.IsControl(keyInfo.KeyChar))
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
            screenBuffer.Write(_paginator.FilterTerm);
            if (_options.DefaultValue != null)
            {
                _paginator.TryGetSelectedItem(out var result);
                screenBuffer.Write(_options.TextSelector(result), Prompt.ColorSchema.Answer);
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
        }

        protected override void FinishTemplate(OffscreenBuffer screenBuffer, T result)
        {
            screenBuffer.WriteFinish(_options.Message);
            screenBuffer.Write(_options.TextSelector(result), Prompt.ColorSchema.Answer);
        }
    }
}
