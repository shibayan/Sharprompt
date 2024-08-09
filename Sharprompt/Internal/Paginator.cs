using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Sharprompt.Internal;

internal class Paginator<T> : IEnumerable<T> where T : notnull
{
    public Paginator(IEnumerable<T> items, int pageSize, Optional<T> defaultValue, Func<T, string> textSelector)
    {
        _items = items.ToArray();
        _pageSize = pageSize <= 0 ? _items.Length : Math.Min(pageSize, _items.Length);
        _textSelector = textSelector;

        InitializeDefaults(defaultValue);
    }

    private readonly T[] _items;
    private readonly Func<T, string> _textSelector;

    private int _pageSize;
    private T[] _filteredItems = Array.Empty<T>();
    private int _selectedIndex = -1;

    public ReadOnlySpan<T> CurrentItems => new(_filteredItems, _pageSize * CurrentPage, Count);

    public int PageCount { get; private set; }

    public int CurrentPage { get; private set; }

    public int Count => Math.Min(_filteredItems.Length - (_pageSize * CurrentPage), _pageSize);

    public int TotalCount => _filteredItems.Length;

    public string FilterKeyword { get; private set; } = "";

    public bool LoopingSelection { get; set; }

    public bool TryGetSelectedItem([NotNullWhen(true)] out T? selectedItem)
    {
        if (_filteredItems.Length == 1)
        {
            selectedItem = _filteredItems[0];

            return true;
        }

        if (_selectedIndex == -1 || _filteredItems.Length == 0)
        {
            selectedItem = default;

            return false;
        }

        selectedItem = _filteredItems[(_pageSize * CurrentPage) + _selectedIndex];

        return true;
    }

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

    public void PreviousItem()
    {
        if (_selectedIndex <= 0)
        {
            if (!LoopingSelection)
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

    public void NextPage()
    {
        if (PageCount == 1)
        {
            return;
        }

        _selectedIndex = -1;
        CurrentPage = CurrentPage >= PageCount - 1 ? 0 : CurrentPage + 1;
    }

    public void PreviousPage()
    {
        if (PageCount == 1)
        {
            return;
        }

        _selectedIndex = -1;
        CurrentPage = CurrentPage <= 0 ? PageCount - 1 : CurrentPage - 1;
    }

    public void UpdateFilter(string keyword)
    {
        FilterKeyword = keyword;

        _selectedIndex = -1;

        UpdateFilteredItems();
    }

    public void UpdatePageSize(int newPageSize)
    {
        if (_pageSize == newPageSize)
        {
            return;
        }

        TryGetSelectedItem(out var selectedItem);

        _pageSize = newPageSize <= 0 ? _items.Length : Math.Min(newPageSize, _items.Length);

        InitializeDefaults(Optional<T>.Create(selectedItem));
    }

    public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)_filteredItems).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private void UpdateFilteredItems()
    {
        _filteredItems = _items.Where(x => _textSelector(x).IndexOf(FilterKeyword, StringComparison.OrdinalIgnoreCase) != -1)
                               .ToArray();

        PageCount = (_filteredItems.Length - 1) / _pageSize + 1;

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
