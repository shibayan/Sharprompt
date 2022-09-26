using System;
using System.Collections.Generic;
using System.Linq;

namespace Sharprompt.Internal;

internal class Paginator<T>
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

    private int _selectedIndex = -1;

    public T[] FilteredItems { get; private set; }

    public int PageCount { get; private set; }

    public int SelectedPage { get; private set; }

    public int Count => Math.Min(FilteredItems.Length - (_pageSize * SelectedPage), _pageSize);

    public int TotalCount => FilteredItems.Length;

    public string FilterTerm { get; private set; } = "";

    public bool TryGetSelectedItem(out T selectedItem)
    {
        if (_selectedIndex == -1 || FilteredItems.Length == 0)
        {
            selectedItem = default;

            return false;
        }

        selectedItem = FilteredItems[(_pageSize * SelectedPage) + _selectedIndex];

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

        SelectedPage = SelectedPage >= PageCount - 1 ? 0 : SelectedPage + 1;
        _selectedIndex = -1;
    }

    public void PreviousPage()
    {
        if (PageCount == 1)
        {
            return;
        }

        SelectedPage = SelectedPage <= 0 ? PageCount - 1 : SelectedPage - 1;
        _selectedIndex = -1;
    }

    public void UpdateFilter(string term)
    {
        FilterTerm = term;

        _selectedIndex = -1;
        SelectedPage = 0;

        UpdateItems();
    }

    public ArraySegment<T> ToSubset() => new(FilteredItems, _pageSize * SelectedPage, Count);

    private void UpdateItems()
    {
        FilteredItems = _items.Where(x => _textSelector(x).IndexOf(FilterTerm, StringComparison.OrdinalIgnoreCase) != -1)
                              .ToArray();

        PageCount = (FilteredItems.Length - 1) / _pageSize + 1;
    }

    private void InitializeDefaults(Optional<T> defaultValue)
    {
        UpdateItems();

        if (!defaultValue.HasValue)
        {
            return;
        }

        for (var i = 0; i < FilteredItems.Length; i++)
        {
            if (EqualityComparer<T>.Default.Equals(FilteredItems[i], defaultValue))
            {
                _selectedIndex = i % _pageSize;
                SelectedPage = i / _pageSize;

                break;
            }
        }
    }
}
