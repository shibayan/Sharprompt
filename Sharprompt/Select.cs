using System;
using System.Collections.Generic;
using System.Linq;

namespace Sharprompt
{
    internal class Select<T>
    {
        public Select(string message, IReadOnlyList<T> options, object defaultValue = null, Func<T, string> valueSelector = null)
        {
            _message = message;
            _baseOptions = options;
            _defaultValue = defaultValue;
            _valueSelector = valueSelector ?? (x => x.ToString());
            _filtering = (filter, value) => value.IndexOf(filter, StringComparison.OrdinalIgnoreCase) != -1;

            UpdateOptions();
        }

        private readonly string _message;
        private readonly IReadOnlyList<T> _baseOptions;
        private readonly object _defaultValue;
        private readonly Func<T, string> _valueSelector;
        private readonly Func<string, string, bool> _filtering;

        private int _selectedIndex;
        private string _filter = "";
        private IReadOnlyList<T> _options;

        public T Start()
        {
            using (var scope = new ConsoleScope(false))
            {
                _selectedIndex = FindIndex();

                var prevFilter = "";

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
                        if (_selectedIndex >= _options.Count - 1)
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
                            _selectedIndex = _options.Count - 1;
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
                            _filter = _filter.Remove(_filter.Length - 1, 1);
                        }
                    }
                    else if (!char.IsControl(keyInfo.KeyChar))
                    {
                        _filter += keyInfo.KeyChar;
                    }

                    if (_filter != prevFilter)
                    {
                        UpdateOptions();

                        prevFilter = _filter;
                    }
                }

                scope.Render(FinishTemplate);

                return _options[_selectedIndex];
            }
        }

        private void Template(ConsoleRenderer renderer)
        {
            renderer.WriteMessage(_message);
            renderer.Write(_filter);

            for (int i = 0; i < _options.Count; i++)
            {
                var label = _valueSelector(_options[i]);

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
            renderer.Write(_valueSelector(_options[_selectedIndex]), ConsoleColor.Cyan);
        }

        private int FindIndex()
        {
            if (_defaultValue == null)
            {
                return -1;
            }

            for (int i = 0; i < _options.Count; i++)
            {
                if (_defaultValue.Equals(_options[i]))
                {
                    return i;
                }
            }

            return -1;
        }

        private void UpdateOptions()
        {
            if (string.IsNullOrEmpty(_filter))
            {
                _options = _baseOptions;
            }
            else
            {
                _options = _baseOptions.Where(x => _filtering(_filter, _valueSelector(x)))
                                       .ToArray();
            }

            _selectedIndex = -1;
        }
    }
}
