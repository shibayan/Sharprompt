using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Sharprompt.Internal;
using Sharprompt.Strings;

namespace Sharprompt.Forms;

internal class MultiSelectForm<T> : SelectFormBase<T, IEnumerable<T>> where T : notnull
{
    public MultiSelectForm(MultiSelectOptions<T> options, PromptConfiguration configuration) : base(configuration)
    {
        options.EnsureOptions();

        _options = options;

        InitializePaginator(options.Items, options.PageSize, Optional<T>.Empty, options.TextSelector, options.LoopingSelection);

        foreach (var defaultValue in options.DefaultValues)
        {
            _selectedItems.Add(defaultValue);
        }

        KeyHandlerMaps[new ConsoleKeyBinding(ConsoleKey.Spacebar)] = HandleSpacebar;
        KeyHandlerMaps[new ConsoleKeyBinding(ConsoleKey.A, ConsoleModifiers.Control)] = HandleCtrlA;
        KeyHandlerMaps[new ConsoleKeyBinding(ConsoleKey.I, ConsoleModifiers.Control)] = HandleCtrlI;
    }

    private readonly MultiSelectOptions<T> _options;

    private readonly HashSet<T> _selectedItems = [];

    protected override void InputTemplate(OffscreenBuffer offscreenBuffer)
    {
        Paginator.UpdatePageSize(Math.Min(_options.PageSize, Height - 2));

        offscreenBuffer.WritePrompt(_options.Message);
        offscreenBuffer.Write(Paginator.FilterKeyword);

        offscreenBuffer.PushCursor();

        if (string.IsNullOrEmpty(Paginator.FilterKeyword))
        {
            offscreenBuffer.WriteHint(Resource.MultiSelectForm_Message_Hint);
        }

        var hasSelected = Paginator.TryGetSelectedItem(out var selectedItem);
        var comparer = EqualityComparer<T>.Default;

        foreach (var item in Paginator.CurrentItems)
        {
            var value = _options.TextSelector(item);
            var isChecked = _selectedItems.Contains(item);

            offscreenBuffer.WriteLine();

            if (hasSelected && comparer.Equals(item, selectedItem))
            {
                offscreenBuffer.WriteSelect($"{Configuration.Symbols.Selector} {(isChecked ? Configuration.Symbols.Selected : Configuration.Symbols.NotSelect)} {value}");
            }
            else
            {
                if (isChecked)
                {
                    offscreenBuffer.WriteSelect($"  {Configuration.Symbols.Selected} {value}");
                }
                else
                {
                    offscreenBuffer.Write($"  {Configuration.Symbols.NotSelect} {value}");
                }
            }
        }

        RenderPagination(offscreenBuffer, _options.Pagination);
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

    private bool HandleSpacebar()
    {
        if (!Paginator.TryGetSelectedItem(out var currentItem))
        {
            return false;
        }

        if (!_selectedItems.Remove(currentItem))
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

    private bool HandleCtrlA()
    {
        if (_selectedItems.Count == Paginator.TotalCount)
        {
            _selectedItems.Clear();
        }
        else
        {
            foreach (var item in Paginator)
            {
                _selectedItems.Add(item);
            }
        }

        return true;
    }

    private bool HandleCtrlI()
    {
        var invertedItems = Paginator.Except(_selectedItems).ToArray();

        _selectedItems.Clear();

        foreach (var item in invertedItems)
        {
            _selectedItems.Add(item);
        }

        return true;
    }
}
