using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Sharprompt.Internal;

/// <summary>
/// Snapshots items and maintains filtered pages, navigation, and an optional highlighted item.
/// </summary>
/// <typeparam name="T">The stored, converted, or selected value type.</typeparam>
internal class Paginator<T> : IEnumerable<T> where T : notnull
{
    /// <summary>
    /// Snapshots the items, normalizes page size, and applies the optional initial selection.
    /// </summary>
    /// <param name="items">The items to snapshot and display in their supplied order.</param>
    /// <param name="pageSize">The requested items per page, normalized to at least one item.</param>
    /// <param name="defaultValue">The optional item to highlight initially.</param>
    /// <param name="textSelector">Formats items for display, filtering, and exact-match selection.</param>
    public Paginator(IEnumerable<T> items, int pageSize, Optional<T> defaultValue, Func<T, string> textSelector)
    {
        _items = items.ToArray();
        _pageSize = NormalizePageSize(pageSize);
        _textSelector = textSelector;

        InitializeDefaults(defaultValue);
    }

    private readonly T[] _items;
    private readonly Func<T, string> _textSelector;

    private int _pageSize;
    private T[] _filteredItems = [];
    private int _selectedIndex = -1;

    /// <summary>
    /// The filtered items displayed on the current page.
    /// </summary>
    public ReadOnlySpan<T> CurrentItems => new(_filteredItems, _pageSize * CurrentPage, Count);

    /// <summary>
    /// The number of filtered pages; an empty result still has one page.
    /// </summary>
    public int PageCount { get; private set; }

    /// <summary>
    /// The zero-based index of the current page.
    /// </summary>
    public int CurrentPage { get; private set; }

    /// <summary>
    /// The number of items on the current filtered page.
    /// </summary>
    public int Count => Math.Min(_filteredItems.Length - (_pageSize * CurrentPage), _pageSize);

    /// <summary>
    /// The total number of items matching the current filter across all pages.
    /// </summary>
    public int TotalCount => _filteredItems.Length;

    /// <summary>
    /// The case-insensitive substring filter currently applied to item display text.
    /// </summary>
    public string FilterKeyword { get; private set; } = "";

    /// <summary>
    /// Whether moving past a page boundary wraps within that page rather than changing pages.
    /// </summary>
    public bool LoopingSelection { get; set; }

    /// <summary>
    /// Resolves the highlighted item, the sole filtered item, or an exact display-text match when no item is highlighted.
    /// </summary>
    /// <param name="selectedItem">Receives the resolved item, or the type default when no item can be resolved.</param>
    /// <returns>True when an item is resolved; otherwise false.</returns>
    public bool TryGetSelectedItem([NotNullWhen(true)] out T? selectedItem)
    {
        if (_filteredItems.Length == 1)
        {
            selectedItem = _filteredItems[0];

            return true;
        }

        if (_selectedIndex == -1 || _filteredItems.Length == 0)
        {
            if (!string.IsNullOrEmpty(FilterKeyword))
            {
                foreach (var item in _filteredItems)
                {
                    if (_textSelector(item).Equals(FilterKeyword, StringComparison.OrdinalIgnoreCase))
                    {
                        selectedItem = item;

                        return true;
                    }
                }
            }

            selectedItem = default;

            return false;
        }

        selectedItem = _filteredItems[(_pageSize * CurrentPage) + _selectedIndex];

        return true;
    }

    /// <summary>
    /// Moves the highlight forward, wrapping within the page or advancing pages according to the looping setting.
    /// </summary>
    public void NextItem()
    {
        if (_selectedIndex >= Count - 1)
        {
            if (!LoopingSelection)
            {
                NextPage();
            }

            _selectedIndex = 0;
        }
        else
        {
            _selectedIndex += 1;
        }
    }

    /// <summary>
    /// Moves the highlight backward, wrapping within the page or changing pages according to the looping setting.
    /// </summary>
    public void PreviousItem()
    {
        if (_selectedIndex <= 0)
        {
            // With nothing selected, select the last item on the current page
            // instead of moving to the previous page, mirroring NextItem.
            if (!LoopingSelection && _selectedIndex == 0)
            {
                PreviousPage();
            }

            _selectedIndex = Count - 1;
        }
        else
        {
            _selectedIndex -= 1;
        }
    }

    /// <summary>
    /// Advances to the next filtered page, wrapping to the first and clearing the item highlight.
    /// </summary>
    public void NextPage()
    {
        if (PageCount == 1)
        {
            return;
        }

        _selectedIndex = -1;
        CurrentPage = CurrentPage >= PageCount - 1 ? 0 : CurrentPage + 1;
    }

    /// <summary>
    /// Moves to the previous filtered page, wrapping to the last and clearing the item highlight.
    /// </summary>
    public void PreviousPage()
    {
        if (PageCount == 1)
        {
            return;
        }

        _selectedIndex = -1;
        CurrentPage = CurrentPage <= 0 ? PageCount - 1 : CurrentPage - 1;
    }

    /// <summary>
    /// Applies a new display-text filter, clears the item highlight, and recalculates valid pages.
    /// </summary>
    /// <param name="keyword">The case-insensitive substring to match against item display text.</param>
    public void UpdateFilter(string keyword)
    {
        FilterKeyword = keyword;

        _selectedIndex = -1;

        UpdateFilteredItems();
    }

    /// <summary>
    /// Changes page size while preserving the selected item when it can still be resolved.
    /// </summary>
    /// <param name="newPageSize">The new requested item count per page, normalized to at least one.</param>
    public void UpdatePageSize(int newPageSize)
    {
        newPageSize = NormalizePageSize(newPageSize);

        if (_pageSize == newPageSize)
        {
            return;
        }

        var hasSelected = TryGetSelectedItem(out var selectedItem);

        _pageSize = newPageSize;

        InitializeDefaults(hasSelected ? new Optional<T>(selectedItem!) : Optional<T>.Empty);
    }

    /// <summary>
    /// Enumerates all filtered items, including items outside the current page.
    /// </summary>
    /// <returns>An enumerator over all filtered items in their original order.</returns>
    public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)_filteredItems).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    // Keep the page size at least 1 so an empty item list yields a single empty page
    // instead of dividing by zero.
    private int NormalizePageSize(int pageSize) => Math.Max(pageSize <= 0 ? _items.Length : Math.Min(pageSize, _items.Length), 1);

    private void UpdateFilteredItems()
    {
        if (string.IsNullOrEmpty(FilterKeyword))
        {
            _filteredItems = _items;
        }
        else
        {
            var filteredItems = new List<T>();

            foreach (var item in _items)
            {
                if (_textSelector(item).Contains(FilterKeyword, StringComparison.OrdinalIgnoreCase))
                {
                    filteredItems.Add(item);
                }
            }

            _filteredItems = filteredItems.ToArray();
        }

        PageCount = Math.Max((_filteredItems.Length - 1) / _pageSize + 1, 1);

        if (CurrentPage >= PageCount)
        {
            CurrentPage = 0;
        }
    }

    private void InitializeDefaults(Optional<T> defaultValue)
    {
        UpdateFilteredItems();

        if (!defaultValue.HasValue)
        {
            return;
        }

        for (var i = 0; i < _filteredItems.Length; i++)
        {
            if (EqualityComparer<T>.Default.Equals(_filteredItems[i], defaultValue))
            {
                _selectedIndex = i % _pageSize;
                CurrentPage = i / _pageSize;

                break;
            }
        }
    }
}
