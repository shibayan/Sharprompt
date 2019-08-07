using System;
using System.Collections.Generic;
using System.Linq;

namespace Sharprompt
{
    internal class Select<T>
    {
        public Select(string message, IEnumerable<T> options, object defaultValue, Func<T, string> valueSelector)
        {
            _message = message;
            _baseOptions = options.Select(x => new Option(valueSelector(x), x)).ToArray();
            _defaultValue = defaultValue;
            _filtering = (filter, value) => value.IndexOf(filter, StringComparison.OrdinalIgnoreCase) != -1;
        }

        private readonly string _message;
        private readonly IReadOnlyList<Option> _baseOptions;
        private readonly object _defaultValue;
        private readonly Func<string, string, bool> _filtering;

        public T Start()
        {
            using (var scope = new ConsoleScope(false))
            {
                var filter = "";
                var prevFilter = "";

                var options = _baseOptions;

                var selectedIndex = FindDefaultIndex(options, _defaultValue);

                while (true)
                {
                    scope.Render(Template, new TemplateModel { Message = _message, Filter = filter, SelectedIndex = selectedIndex, Options = options });

                    var keyInfo = scope.ReadKey();

                    if (keyInfo.Key == ConsoleKey.Enter)
                    {
                        if (selectedIndex != -1)
                        {
                            break;
                        }

                        scope.SetError(new ValidationError("Value is required"));
                    }
                    else if (keyInfo.Key == ConsoleKey.DownArrow)
                    {
                        if (selectedIndex >= options.Count - 1)
                        {
                            selectedIndex = 0;
                        }
                        else
                        {
                            selectedIndex++;
                        }
                    }
                    else if (keyInfo.Key == ConsoleKey.UpArrow)
                    {
                        if (selectedIndex <= 0)
                        {
                            selectedIndex = options.Count - 1;
                        }
                        else
                        {
                            selectedIndex--;
                        }
                    }
                    else if (keyInfo.Key == ConsoleKey.Backspace)
                    {
                        if (filter.Length == 0)
                        {
                            scope.Beep();
                        }
                        else
                        {
                            filter = filter.Remove(filter.Length - 1, 1);
                        }
                    }
                    else if (!char.IsControl(keyInfo.KeyChar))
                    {
                        filter += keyInfo.KeyChar;
                    }

                    if (filter != prevFilter)
                    {
                        options = _baseOptions.Where(x => _filtering(filter, x.Value)).ToArray();

                        prevFilter = filter;
                        selectedIndex = -1;
                    }
                }

                scope.Render(FinishTemplate, new FinishTemplateModel { Message = _message, SelectedIndex = selectedIndex, Options = options });

                return options[selectedIndex].Item;
            }
        }

        private int FindDefaultIndex(IReadOnlyList<Option> list, object item)
        {
            if (item == null)
            {
                return -1;
            }

            for (int i = 0; i < list.Count; i++)
            {
                if (EqualityComparer<T>.Default.Equals((T)item, list[i].Item))
                {
                    return i;
                }
            }

            return -1;
        }

        private void Template(ConsoleRenderer renderer, TemplateModel model)
        {
            renderer.WriteMessage(model.Message);
            renderer.Write(model.Filter);

            for (int i = 0; i < model.Options.Count; i++)
            {
                var value = model.Options[i].Value;

                renderer.WriteLine();

                if (model.SelectedIndex == i)
                {
                    renderer.Write($"> {value}", ConsoleColor.Green);
                }
                else
                {
                    renderer.Write($"  {value}");
                }
            }
        }

        private void FinishTemplate(ConsoleRenderer renderer, FinishTemplateModel model)
        {
            renderer.WriteMessage(model.Message);
            renderer.Write(model.Options[model.SelectedIndex].Value, ConsoleColor.Cyan);
        }

        private class TemplateModel
        {
            public string Message { get; set; }
            public string Filter { get; set; }
            public IReadOnlyList<Option> Options { get; set; }
            public int SelectedIndex { get; set; }
        }

        private class FinishTemplateModel
        {
            public string Message { get; set; }
            public IReadOnlyList<Option> Options { get; set; }
            public int SelectedIndex { get; set; }
        }

        private struct Option
        {
            public Option(string value, T item)
            {
                Value = value;
                Item = item;
            }

            public string Value { get; }
            public T Item { get; }
        }
    }
}
