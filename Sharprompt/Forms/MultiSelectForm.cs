using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Sharprompt.Internal;
using Sharprompt.Strings;

namespace Sharprompt.Forms;

internal class MultiSelectForm<T> : FormBase<IEnumerable<T>> where T : notnull
{
    public MultiSelectForm(MultiSelectOptions<T> options)
    {
        options.EnsureOptions();

        _options = options;
        _paginator = new Paginator<T>(options.Items, Math.Min(options.PageSize, Height - 2), Optional<T>.Empty, options.TextSelector);

        foreach (var defaultValue in options.DefaultValues)
        {
            _selectedItems.Add(defaultValue);
        }

        KeyHandlerMaps = new()
        {
            [ConsoleKey.Spacebar] = HandleSpacebar,
            [ConsoleKey.UpArrow] = HandleUpArrow,
            [ConsoleKey.DownArrow] = HandleDownArrow,
            [ConsoleKey.LeftArrow] = HandleLeftArrow,
            [ConsoleKey.RightArrow] = HandleRightArrow,
            [ConsoleKey.Backspace] = HandleBackspace,
            [ConsoleKey.A] = HandleAWithControl,
            [ConsoleKey.I] = HandleIWithControl,
        };
    }

    private readonly MultiSelectOptions<T> _options;
    private readonly Paginator<T> _paginator;

    private readonly HashSet<T> _selectedItems = new();

    protected override void InputTemplate(OffscreenBuffer offscreenBuffer)
    {
        offscreenBuffer.WritePrompt(_options.Message);
        offscreenBuffer.Write(_paginator.FilterTerm);

        offscreenBuffer.PushCursor();

        if (string.IsNullOrEmpty(_paginator.FilterTerm))
        {
            offscreenBuffer.WriteHint(Resource.MultiSelectForm_Message_Hint);
        }

        var subset = _paginator.ToSubset();

        foreach (var item in subset)
        {
            var value = _options.TextSelector(item);

            offscreenBuffer.WriteLine();

            if (_paginator.TryGetSelectedItem(out var selectedItem) && EqualityComparer<T>.Default.Equals(item, selectedItem))
            {
                offscreenBuffer.WriteSelect($"{Prompt.Symbols.Selector} {(_selectedItems.Contains(item) ? Prompt.Symbols.Selected : Prompt.Symbols.NotSelect)} {value}");
            }
            else
            {
                if (_selectedItems.Contains(item))
                {
                    offscreenBuffer.WriteSelect($"  {Prompt.Symbols.Selected} {value}");
                }
                else
                {
                    offscreenBuffer.Write($"  {Prompt.Symbols.NotSelect} {value}");
                }
            }
        }

        if (_paginator.PageCount > 1)
        {
            offscreenBuffer.WriteLine();
            offscreenBuffer.WriteHint(_options.Pagination(_paginator.TotalCount, _paginator.SelectedPage + 1, _paginator.PageCount));
        }
    }

    protected override void FinishTemplate(OffscreenBuffer offscreenBuffer, IEnumerable<T> result)
    {
        offscreenBuffer.WriteDone(_options.Message);
        offscreenBuffer.WriteAnswer(string.Join(", ", result.Select(_options.TextSelector)));
    }

    protected override bool HandleEnter([NotNullWhen(true)] out IEnumerable<T>? result)
    {
        if (_selectedItems.Count >= _options.Minimum)
        {
            result = _options.Items
                             .Where(x => _selectedItems.Contains(x))
                             .ToArray();

            return true;
        }

        SetError(string.Format(Resource.Validation_Minimum_SelectionRequired, _options.Minimum));

        result = default;

        return false;
    }

    protected override bool HandleTextInput(ConsoleKeyInfo keyInfo)
    {
        base.HandleTextInput(keyInfo);

        _paginator.UpdateFilter(InputBuffer.ToString());

        return true;
    }

    private bool HandleSpacebar(ConsoleKeyInfo keyInfo)
    {
        if (!_paginator.TryGetSelectedItem(out var currentItem))
        {
            return false;
        }

        if (_selectedItems.Contains(currentItem))
        {
            _selectedItems.Remove(currentItem);
        }
        else
        {
            if (_selectedItems.Count >= _options.Maximum)
            {
                SetError(string.Format(Resource.Validation_Maximum_SelectionRequired, _options.Maximum));
            }
            else
            {
                _selectedItems.Add(currentItem);
            }
        }

        return true;
    }

    private bool HandleUpArrow(ConsoleKeyInfo keyInfo)
    {
        _paginator.PreviousItem();

        return true;
    }

    private bool HandleDownArrow(ConsoleKeyInfo keyInfo)
    {
        _paginator.NextItem();

        return true;
    }

    private bool HandleLeftArrow(ConsoleKeyInfo keyInfo)
    {
        _paginator.PreviousPage();

        return true;
    }

    private bool HandleRightArrow(ConsoleKeyInfo keyInfo)
    {
        _paginator.NextPage();

        return true;
    }

    private bool HandleBackspace(ConsoleKeyInfo keyInfo)
    {
        if (InputBuffer.IsStart)
        {
            return false;
        }

        InputBuffer.Backspace();
        _paginator.UpdateFilter(InputBuffer.ToString());

        return true;
    }

    private bool HandleAWithControl(ConsoleKeyInfo keyInfo)
    {
        if (keyInfo.Modifiers != ConsoleModifiers.Control)
        {
            return false;
        }

        if (_selectedItems.Count == _paginator.TotalCount)
        {
            _selectedItems.Clear();
        }
        else
        {
            foreach (var item in _paginator.FilteredItems)
            {
                _selectedItems.Add(item);
            }
        }

        return true;
    }

    private bool HandleIWithControl(ConsoleKeyInfo keyInfo)
    {
        if (keyInfo.Modifiers != ConsoleModifiers.Control)
        {
            return false;
        }

        var invertedItems = _paginator.FilteredItems.Except(_selectedItems).ToArray();

        _selectedItems.Clear();

        foreach (var item in invertedItems)
        {
            _selectedItems.Add(item);
        }

        return true;
    }
}
