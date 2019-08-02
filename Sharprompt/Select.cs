using System;
using System.Collections.Generic;

namespace Sharprompt
{
    public class Select<T>
    {
        public Select(string message, IReadOnlyList<T> options, object defaultValue = null, Func<T, string> labelSelector = null)
        {
            _message = message;
            _options = options;
            _defaultValue = defaultValue;
            _labelSelector = labelSelector ?? (x => x.ToString());
        }

        private readonly string _message;
        private readonly IReadOnlyList<T> _options;
        private readonly object _defaultValue;
        private readonly Func<T, string> _labelSelector;

        private int _selectedIndex;

        public T Start()
        {
            using (var scope = new ConsoleScope(false))
            {
                _selectedIndex = FindIndex();

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

                        scope.SetError(new Error("Value is required"));
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
                }

                scope.Render(FinishTemplate);

                return _options[_selectedIndex];
            }
        }

        private void Template(ConsoleRenderer renderer)
        {
            renderer.WriteMessage(_message);

            for (int i = 0; i < _options.Count; i++)
            {
                var label = _labelSelector(_options[i]);

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
            renderer.Write(_labelSelector(_options[_selectedIndex]), ConsoleColor.Cyan);
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
    }
}
