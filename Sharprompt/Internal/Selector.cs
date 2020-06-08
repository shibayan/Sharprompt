using System;
using System.Collections.Generic;
using System.Linq;

namespace Sharprompt.Internal
{
    public class Selector<T>
    {
        public Selector(IEnumerable<T> items, int? pageSize, object defaultValue, Func<T, string> valueSelector)
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

        public bool IsSelected => _selectedIndex != -1;

        public T SelectedItem => _selectedIndex == -1 ? default : _filteredSource[(_pageSize * _selectedPage) + _selectedIndex];

        public string FilterTerm { get; private set; } = "";

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

        private void InitializeDefaults(object defaultValue)
        {
            InitializeCollection();

            if (defaultValue == null)
            {
                return;
            }

            for (int i = 0; i < _filteredSource.Length; i++)
            {
                if (EqualityComparer<T>.Default.Equals(_filteredSource[i], (T)defaultValue))
                {
                    _selectedIndex = i % _pageSize;
                    _selectedPage = i / _pageSize;

                    break;
                }
            }
        }
    }
}
