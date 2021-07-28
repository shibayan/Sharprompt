﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading;

using Sharprompt.Internal;

namespace Sharprompt.Forms
{
    internal class MultiSelectForm<T> : FormBase<IEnumerable<T>>
    {
        public MultiSelectForm(MultiSelectOptions<T> options) : base(true)
        {
            // throw early when invalid options are passed
            if (options.Minimum < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(options.Minimum), $"The minimum ({options.Minimum}) is not valid");
            }

            if (options.Maximum < options.Minimum)
            {
                throw new ArgumentException($"The maximum ({options.Maximum}) is not valid when minimum is set to ({options.Minimum})", nameof(options.Maximum));
            }


            _paginator = new Paginator<T>(options.Items, options.PageSize, Optional<T>.Empty, options.TextSelector);
            _paginator.FirstItem();

            if (options.DefaultValues != null)
            {
                _selectedItems.AddRange(options.DefaultValues);
            }
            _options = options;
        }

        private readonly MultiSelectOptions<T> _options;
        private readonly Paginator<T> _paginator;

        private readonly List<T> _selectedItems = new List<T>();
        private readonly StringBuilder _filterBuffer = new StringBuilder();

        protected override bool TryGetResult(CancellationToken cancellationToken,out IEnumerable<T> result)
        {
            do
            {
                var keyInfo = ConsoleDriver.WaitKeypress(cancellationToken);

                switch (keyInfo.Key)
                {
                    case ConsoleKey.Enter when keyInfo.Modifiers == ConsoleModifiers.Control:
                        _paginator.UnSelected();
                        break;
                    case ConsoleKey.Enter when keyInfo.Modifiers == 0 &&  _selectedItems.Count >= _options.Minimum:
                        result = _selectedItems;
                        return true;
                    case ConsoleKey.Enter when keyInfo.Modifiers == 0:
                        SetValidationResult(new ValidationResult(string.Format(Prompt.Messages.MultiSelectMinSelection,_options.Minimum)));
                        break;
                    case ConsoleKey.Spacebar when keyInfo.Modifiers == 0 && _paginator.TryGetSelectedItem(out var currentItem):
                    {
                        if (_selectedItems.Contains(currentItem))
                        {
                            _selectedItems.Remove(currentItem);
                        }
                        else
                        {
                            if (_selectedItems.Count >= _options.Maximum)
                            {
                                SetValidationResult(new ValidationResult(string.Format(Prompt.Messages.MultiSelectMaxSelection, _options.Maximum)));
                            }
                            else
                            {
                                _selectedItems.Add(currentItem);
                            }
                        }

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

            result = null;

            return false;
        }

        protected override void InputTemplate(OffscreenBuffer screenBuffer)
        {
            screenBuffer.WritePrompt(_options.Message);

            var showSelected = (_selectedItems.Count > 0 && _filterBuffer.Length == 0) || !_paginator.IsUnSelected;

            if (_paginator.IsUnSelected)
            {
                screenBuffer.Write(_filterBuffer.ToString());
            }

            var (left, top) = screenBuffer.GetCursorPosition();

            if (showSelected && !_paginator.IsUnSelected)
            {
                screenBuffer.Write(string.Join(", ", _selectedItems.Select(_options.TextSelector)), Prompt.ColorSchema.Answer);
            }

            if (_options.ShowKeyNavigation)
            {
                screenBuffer.WriteLine();
                if (_paginator.PageCount > 1)
                {
                    screenBuffer.Write(Prompt.Messages.KeyNavPaging, Prompt.ColorSchema.KeyNavigation);
                }
                screenBuffer.Write(Prompt.Messages.MultiSelectKeyNavigation, Prompt.ColorSchema.KeyNavigation);
                if (_filterBuffer.Length > 0)
                {
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

            if (_options.ShowPagination)
            {
                if (_paginator.PageCount > 1)
                {
                    screenBuffer.WriteLine();
                    screenBuffer.Write(_paginator.PaginationMessage(), Prompt.ColorSchema.PaginationInfo);
                }
            }

            screenBuffer.SetCursorPosition(left, top);
        }

        protected override void FinishTemplate(OffscreenBuffer screenBuffer, IEnumerable<T> result)
        {
            screenBuffer.WriteFinish(_options.Message);
            screenBuffer.Write(string.Join(", ", result.Select(_options.TextSelector)), Prompt.ColorSchema.Answer);
        }
    }
}
