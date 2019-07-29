using System;
using System.Collections.Generic;

namespace Sharprompt
{
    internal class Select<T>
    {
        public Select(string message, IReadOnlyList<T> items)
            : this(message, items, x => x.ToString())
        {
        }

        public Select(string message, IReadOnlyList<T> items, Func<T, string> labelSelector)
        {
            _message = message;
            _items = items;
            _labelSelector = labelSelector;
        }

        private readonly string _message;
        private readonly IReadOnlyList<T> _items;
        private readonly Func<T, string> _labelSelector;

        private int _selectedIndex;

        public T Start()
        {
            using (var scope = new ConsoleScope(false))
            {
                _selectedIndex = 0;

                while (true)
                {
                    scope.Render(Template);

                    var keyInfo = scope.ReadKey();

                    if (keyInfo.Key == ConsoleKey.Enter)
                    {
                        break;
                    }

                    if (keyInfo.Key == ConsoleKey.DownArrow)
                    {
                        if (_selectedIndex == _items.Count - 1)
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
                        if (_selectedIndex == 0)
                        {
                            _selectedIndex = _items.Count - 1;
                        }
                        else
                        {
                            _selectedIndex--;
                        }
                    }
                }

                return _items[_selectedIndex];
            }
        }

        private void Template(ConsoleRenderer renderer)
        {
            renderer.WriteMessage(_message);

            for (int i = 0; i < _items.Count; i++)
            {
                var label = _labelSelector(_items[i]);

                renderer.WriteLine();

                if (_selectedIndex == i)
                {
                    renderer.Write($"> {label}", ConsoleColor.Cyan);
                }
                else
                {
                    renderer.Write($"  {label}");
                }
            }
        }
    }
}
