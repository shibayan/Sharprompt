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
        _pageSize = Math.Min(pageSize, _items.Length);
        _textSelector = textSelector;

        InitializeDefaults(defaultValue);
    }

    private readonly T[] _items;
    private readonly int _pageSize;
    private readonly Func<T, string> _textSelector;

    private T[] _filteredItems = Array.Empty<T>();
    private int _selectedIndex = -1;

    public ReadOnlySpan<T> CurrentItems => new(_filteredItems, _pageSize * CurrentPage, Count);

    public int PageCount { get; private set; }

    public int CurrentPage { get; private set; }

    public int Count => Math.Min(_filteredItems.Length - (_pageSize * CurrentPage), _pageSize);

    public int TotalCount => _filteredItems.Length;

    public string FilterKeyword { get; private set; } = "";

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
        _selectedIndex = _selectedIndex >= Count - 1 ? 0 : _selectedIndex + 1;
    }

    public void PreviousItem()
    {
        _selectedIndex = _selectedIndex <= 0 ? Count - 1 : _selectedIndex - 1;
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
        CurrentPage = 0;

        UpdateFilteredItems();
    }

    public IEnumerator<T> GetEnumerator() => (IEnumerator<T>)_filteredItems.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private void UpdateFilteredItems()
    {
        _filteredItems = _items.Where(x => _textSelector(x).IndexOf(FilterKeyword, StringComparison.OrdinalIgnoreCase) != -1)
                               .ToArray();

        PageCount = (_filteredItems.Length - 1) / _pageSize + 1;
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
