using System;
using System.Collections.Generic;
using System.Linq;

namespace Sharprompt.Internal
{
    public class Selector<T>
    {
        public Selector(IEnumerable<T> source, int? pageSize, object defaultValue, Func<T, string> valueSelector)
        {
            _source = source.ToArray();
            _pageSize = pageSize ?? _source.Length;
            _valueSelector = valueSelector;

            InitializeDefaults(defaultValue);
        }

        private readonly T[] _source;
        private readonly int _pageSize;
        private readonly Func<T, string> _valueSelector;

        private T[] _filteredSource;
        private int _pageCount;

        private int _currentIndex = -1;
        private int _currentPage;

        public int Count => Math.Min(_filteredSource.Length - (_pageSize * _currentPage), _pageSize);

        public T CurrentItem => _currentIndex == -1 ? default : _filteredSource[(_pageSize * _currentPage) + _currentIndex];

        public string FilterTerm { get; private set; } = "";

        public void NextItem()
        {
            _currentIndex = _currentIndex >= Count - 1 ? 0 : _currentIndex + 1;
        }

        public void PreviousItem()
        {
            _currentIndex = _currentIndex <= 0 ? Count - 1 : _currentIndex - 1;
        }

        public void NextPage()
        {
            _currentPage = _currentPage >= _pageCount - 1 ? 0 : _currentPage + 1;
            _currentIndex = -1;
        }

        public void PreviousPage()
        {
            _currentPage = _currentPage <= 0 ? _pageCount - 1 : _currentPage - 1;
            _currentIndex = -1;
        }

        public void UpdateFilter(string term)
        {
            FilterTerm = term;

            _currentIndex = -1;
            _currentPage = 0;

            InitializeCollection();
        }

        public ArraySegment<T> ToSubset()
        {
            return new ArraySegment<T>(_filteredSource, _pageSize * _currentPage, Count);
        }

        private void InitializeCollection()
        {
            _filteredSource = _source.Where(x => _valueSelector(x).IndexOf(FilterTerm, StringComparison.OrdinalIgnoreCase) != -1)
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
                    _currentIndex = i % _pageSize;
                    _currentPage = i / _pageSize;

                    break;
                }
            }
        }
    }
}
