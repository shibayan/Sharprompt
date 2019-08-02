using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sharprompt
{
    internal class Select<T>
    {
        public Select(string message, IReadOnlyList<T> options, object defaultValue = null, Func<T, string> labelSelector = null)
        {
            _message = message;
            _options = options;
            _defaultValue = defaultValue;
            _labelSelector = labelSelector ?? (x => x.ToString());
            _filtering = (filter, value) => value.Contains(filter);

            _filteredOptions = FilteredOptions();
        }

        private readonly string _message;
        private readonly IReadOnlyList<T> _options;
        private readonly object _defaultValue;
        private readonly Func<T, string> _labelSelector;
        private readonly Func<string, string, bool> _filtering;

        private int _selectedIndex;
        private StringBuilder _filter;
        private IReadOnlyList<T> _filteredOptions;

        public T Start()
        {
            using (var scope = new ConsoleScope(false))
            {
                _selectedIndex = FindIndex();

                _filter = new StringBuilder(64);

                while (true)
                {
                    scope.Render(Template);

                    var keyInfo = scope.ReadKey();

                    if (keyInfo.Key == ConsoleKey.Enter)
                    {
                        if (_selectedIndex != -1)
                        {
                            break;
                        }

                        scope.SetError(new ValidationError("Value is required"));
                    }
                    else if (keyInfo.Key == ConsoleKey.DownArrow)
                    {
                        if (_selectedIndex >= _filteredOptions.Count - 1)
                        {
                            _selectedIndex = 0;
                        }
                        else
                        {
                            _selectedIndex++;
                        }
                    }
                    else if (keyInfo.Key == ConsoleKey.UpArrow)
                    {
                        if (_selectedIndex <= 0)
                        {
                            _selectedIndex = _filteredOptions.Count - 1;
                        }
                        else
                        {
                            _selectedIndex--;
                        }
                    }
                    else if (keyInfo.Key == ConsoleKey.Backspace)
                    {
                        if (_filter.Length == 0)
                        {
                            scope.Beep();
                        }
                        else
                        {
                            _filter.Length -= 1;

                            _filteredOptions = FilteredOptions();
                            _selectedIndex = -1;
                        }
                    }
                    else if (!char.IsControl(keyInfo.KeyChar))
                    {
                        _filter.Append(keyInfo.KeyChar);

                        _filteredOptions = FilteredOptions();
                        _selectedIndex = -1;
                    }
                }

                scope.Render(FinishTemplate);

                return _filteredOptions[_selectedIndex];
            }
        }

        private void Template(ConsoleRenderer renderer)
        {
            renderer.WriteMessage(_message);
            renderer.Write(_filter.ToString());

            for (int i = 0; i < _filteredOptions.Count; i++)
            {
                var label = _labelSelector(_filteredOptions[i]);

                if (!label.Contains(_filter.ToString()))
                {
                    continue;
                }

                renderer.WriteLine();

                if (_selectedIndex == i)
                {
                    renderer.Write($"> {label}", ConsoleColor.Green);
                }
                else
                {
                    renderer.Write($"  {label}");
                }
            }
        }

        private void FinishTemplate(ConsoleRenderer renderer)
        {
            renderer.WriteMessage(_message);
            renderer.Write(_labelSelector(_filteredOptions[_selectedIndex]), ConsoleColor.Cyan);
        }

        private int FindIndex()
        {
            if (_defaultValue == null)
            {
                return -1;
            }

            for (int i = 0; i < _filteredOptions.Count; i++)
            {
                if (_defaultValue.Equals(_filteredOptions[i]))
                {
                    return i;
                }
            }

            return -1;
        }

        private IReadOnlyList<T> FilteredOptions()
        {
            if (_filter == null || _filter.Length == 0)
            {
                return _options;
            }

            var filter = _filter.ToString();

            return _options.Where(x => _filtering(filter, _labelSelector(x)))
                           .ToArray();
        }
    }
}
