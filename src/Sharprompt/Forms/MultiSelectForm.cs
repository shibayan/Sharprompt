using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Sharprompt.Internal;
using Sharprompt.Strings;

namespace Sharprompt.Forms;

internal class MultiSelectForm<T> : SelectFormBase<T, IEnumerable<T>> where T : notnull
{
    public MultiSelectForm(MultiSelectOptions<T> options, PromptConfiguration configuration) : this(options, Prepare(options), configuration)
    {
    }

    private MultiSelectForm(MultiSelectOptions<T> options, T[] items, PromptConfiguration configuration) : base(configuration)
    {
        _options = options;

        _items = items;

        InitializePaginator(_items, options.PageSize, Optional<T>.Empty, options.TextSelector, options.LoopingSelection);

        // Default values that are not in Items can never be returned, so ignore them
        // instead of letting them count towards Minimum.
        var itemSet = new HashSet<T>(_items);

        foreach (var defaultValue in options.DefaultValues)
        {
            if (itemSet.Contains(defaultValue))
            {
                _selectedItems.Add(defaultValue);
            }
        }

        KeyHandlerMaps[new ConsoleKeyBinding(ConsoleKey.Spacebar)] = HandleSpacebar;
        KeyHandlerMaps[new ConsoleKeyBinding(ConsoleKey.A, ConsoleModifiers.Control)] = HandleCtrlA;
        KeyHandlerMaps[new ConsoleKeyBinding(ConsoleKey.I, ConsoleModifiers.Control)] = HandleCtrlI;
    }

    private readonly MultiSelectOptions<T> _options;
    private readonly T[] _items;

    // Runs before the base constructor creates the console driver, so invalid options throw without leaving it undisposed.
    private static T[] Prepare(MultiSelectOptions<T> options)
    {
        options.EnsureOptions();

        // Items are enumerated once, and rendering and the result both use this snapshot. Equal items are listed
        // once, at the position of the first; otherwise checking one would check all of them and return each.
        var items = options.Items.Distinct().ToArray();

        // Enter would always be rejected, so the prompt could only be canceled.
        if (options.Minimum > items.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(options.Minimum), string.Format(Resource.Validation_Minimum_ExceedsItems, options.Minimum, items.Length));
        }

        return items;
    }

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
        if (_selectedItems.Count < _options.Minimum)
        {
            SetError(string.Format(Resource.Validation_Minimum_SelectionRequired, _options.Minimum));

            result = default;

            return false;
        }

        // DefaultValues can preselect more items than Maximum allows.
        if (_selectedItems.Count > _options.Maximum)
        {
            SetError(string.Format(Resource.Validation_Maximum_SelectionRequired, _options.Maximum));

            result = default;

            return false;
        }

        result = _items.Where(_selectedItems.Contains).ToArray();

        return true;
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
        // Bulk operations only apply to the currently filtered items, so selections
        // hidden by the filter are preserved.
        var selectedItems = new HashSet<T>(_selectedItems);

        if (Paginator.All(_selectedItems.Contains))
        {
            selectedItems.ExceptWith(Paginator);
        }
        else
        {
            selectedItems.UnionWith(Paginator);
        }

        UpdateSelectedItems(selectedItems);

        return true;
    }

    private bool HandleCtrlI()
    {
        var selectedItems = new HashSet<T>(_selectedItems);

        selectedItems.SymmetricExceptWith(Paginator);

        UpdateSelectedItems(selectedItems);

        return true;
    }

    private void UpdateSelectedItems(HashSet<T> selectedItems)
    {
        if (selectedItems.Count > _options.Maximum)
        {
            SetError(string.Format(Resource.Validation_Maximum_SelectionRequired, _options.Maximum));

            return;
        }

        _selectedItems.Clear();
        _selectedItems.UnionWith(selectedItems);
    }
}
