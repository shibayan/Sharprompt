using System;
using System.Collections.Generic;
using System.Linq;

namespace Sharprompt.Internal
{
    internal class Selector<T>
    {
        public Selector(IEnumerable<T> items, int? pageSize, Optional<T> defaultValue, Func<T, string> valueSelector)
        {
            _items = items.ToArray();
            _pageSize = pageSize ?? _items.Length;
            _valueSelector = valueSelector;

            InitializeDefaults(defaultValue);
        }

        private readonly T[] _items;
        private readonly int _pageSize;
        private readonly Func<T, string> _valueSelector;

        private T[] _filteredSource;
        private int _pageCount;

        private int _selectedIndex = -1;
        private int _selectedPage;

        public int Count => Math.Min(_filteredSource.Length - (_pageSize * _selectedPage), _pageSize);

        public string FilterTerm { get; private set; } = "";

        public bool TryGetSelectedItem(out T selectedItem)
        {
            if (_selectedIndex == -1)
            {
                selectedItem = default;

                return false;
            }

            selectedItem = _filteredSource[(_pageSize * _selectedPage) + _selectedIndex];

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
            _selectedPage = _selectedPage >= _pageCount - 1 ? 0 : _selectedPage + 1;
            _selectedIndex = -1;
        }

        public void PreviousPage()
        {
            _selectedPage = _selectedPage <= 0 ? _pageCount - 1 : _selectedPage - 1;
            _selectedIndex = -1;
        }

        public void UpdateFilter(string term)
        {
            FilterTerm = term;

            _selectedIndex = -1;
            _selectedPage = 0;

            InitializeCollection();
        }

        public ArraySegment<T> ToSubset()
        {
            return new ArraySegment<T>(_filteredSource, _pageSize * _selectedPage, Count);
        }

        private void InitializeCollection()
        {
            _filteredSource = _items.Where(x => _valueSelector(x).IndexOf(FilterTerm, StringComparison.OrdinalIgnoreCase) != -1)
                                    .ToArray();

            _pageCount = (_filteredSource.Length - 1) / _pageSize + 1;
        }

        private void InitializeDefaults(Optional<T> defaultValue)
        {
            InitializeCollection();

            if (!defaultValue.HasValue)
            {
                return;
            }

            for (var i = 0; i < _filteredSource.Length; i++)
            {
                if (EqualityComparer<T>.Default.Equals(_filteredSource[i], defaultValue))
                {
                    _selectedIndex = i % _pageSize;
                    _selectedPage = i / _pageSize;

                    break;
                }
            }
        }
    }
}
