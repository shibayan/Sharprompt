using System;
using System.Collections.Generic;
using System.Linq;

namespace Sharprompt
{
    internal class MultiSelect<T>
    {
        public MultiSelect(string message, IEnumerable<T> options, int limit, int min, int pageSize, Func<T, string> valueSelector)
        {
            // throw early when invalid options are passed
            if (limit != -1 && limit < min) throw new ArgumentException($"The limit ({limit}) is not valid when min is set to ({min})", "limit");

            _message = message;
            _limit = limit;
            _min = min;
            _pageSize = pageSize;
            _filtering = (filter, value) => value.IndexOf(filter, StringComparison.OrdinalIgnoreCase) != -1;
            _baseOptions = options.Select(x => new Option(valueSelector(x), x, IsOptionEnabled(x))).ToArray();
        }

        private readonly string _message;
        private readonly int _limit;
        private readonly int _min;
        private readonly int _pageSize;
        private readonly Func<string, string, bool> _filtering;
        private readonly IReadOnlyList<Option> _baseOptions;
        private const string _arrowRight = "\u0010";

        private IReadOnlyList<Option> _disabledByLimit = new List<Option>();

        private bool _showConfirm = false;

        private bool IsOptionEnabled(T o)
        {
            object boxed = typeof(T).GetProperty("Enabled")?.GetValue(o, null);

            if (boxed == null) return true;

            return (bool)boxed;
        }

        public IEnumerable<T> Start()
        {
            using (var scope = new ConsoleScope(false))
            {
                // Defaults
                ICollection<Option> selectedOptions = new List<Option>();
                IReadOnlyList<Option> _options = _baseOptions, filteredOptions = _baseOptions;
                int currentIndex = 0;
                string filter = "", prevFilter = "";
                int prevPage = -1, pageCount = (_options.Count - 1) / _pageSize + 1;
                int currentPage = 0;

                while (true)
                {
                    if (filter != prevFilter)
                    {
                        filteredOptions = _baseOptions.Where(x => _filtering(filter, x.Value))
                                                      .ToArray();

                        prevFilter = filter;
                        currentPage = 0;
                        prevPage = -1;
                        pageCount = (filteredOptions.Count - 1) / _pageSize + 1;
                    }
                    if (currentPage != prevPage)
                    {

                        _options = (filteredOptions).Skip(currentPage * _pageSize)
                                                 .Take(_pageSize)
                                                 .ToArray();

                        currentIndex = 0;

                        prevPage = currentPage;
                    }

                    scope.Render(Template, new TemplateModel { Message = _message, Filter = filter, SelectedOptions = selectedOptions, Options = _options, CurrentIndex = currentIndex });

                    var keyInfo = scope.ReadKey();

                    if (keyInfo.Key == ConsoleKey.Enter)
                    {
                        Option currentOption = _options[currentIndex];
                        if (!selectedOptions.Contains(currentOption))
                        {
                            selectedOptions.Add(_options[currentIndex]);
                        }
                        else
                        {
                            selectedOptions.Remove(currentOption);
                        }
                        // If we have reached the limit, determine which items should not be selected anymore
                        if (_limit == selectedOptions.Count)
                        {
                            _disabledByLimit = _baseOptions.Where(o => !selectedOptions.Contains(o)).ToList();
                            _showConfirm = true;
                        }
                        else
                        {
                            _showConfirm = selectedOptions.Count >= _min;

                            if (_disabledByLimit.Count > 0)
                            {
                                _showConfirm = false;
                                _disabledByLimit = new List<Option>();
                            }
                        }
                    }
                    else if (keyInfo.Key == ConsoleKey.LeftArrow)
                    {
                        if (currentPage <= 0)
                        {
                            currentPage = pageCount - 1;
                        }
                        else
                        {
                            currentPage--;
                        }
                    }
                    else if (keyInfo.Key == ConsoleKey.RightArrow)
                    {
                        if (currentPage >= pageCount - 1)
                        {
                            currentPage = 0;
                        }
                        else
                        {
                            currentPage++;
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
                    else if (!char.IsControl(keyInfo.KeyChar) && !_showConfirm)
                    {
                        filter += keyInfo.KeyChar;
                    }
                    else if (keyInfo.Key == ConsoleKey.Tab)
                    {
                        if (selectedOptions.Count > 0 && selectedOptions.Count >= _min)
                        {
                            break;
                        }
                        scope.SetError(new ValidationError($"A minimum selection of {(_min > -1 ? _min : 1)} items is required"));

                    }
                    else if (keyInfo.Key == ConsoleKey.UpArrow)
                    {
                        if (_options.All(o => _disabledByLimit.Contains(o))) continue;

                        currentIndex = (currentIndex <= 0) ? _baseOptions.Count - 1 : currentIndex -= 1;
                        while (!_baseOptions[currentIndex].Enabled || _disabledByLimit.Contains(_baseOptions[currentIndex]))
                        {
                            currentIndex = (currentIndex <= 0) ? _options.Count - 1 : currentIndex -= 1;
                        };
                    }
                    else if (keyInfo.Key == ConsoleKey.DownArrow)
                    {
                        if (_options.All(o => _disabledByLimit.Contains(o))) continue;

                        currentIndex = (currentIndex >= _options.Count - 1) ? 0 : currentIndex += 1;
                        while (!_options[currentIndex].Enabled || _disabledByLimit.Contains(_options[currentIndex]))
                        {
                            currentIndex = currentIndex >= _options.Count - 1 ? 0 : currentIndex += 1;
                        };
                    }

                }
                scope.Render(FinishTemplate, new FinishTemplateModel { Message = _message, SelectedOptions = selectedOptions, Options = _options, CurrentIndex = currentIndex });

                return _baseOptions.Where(o => selectedOptions.Contains(o)).Select(x => x.Item);
            }
        }


        private void Template(ConsoleRenderer renderer, TemplateModel model)
        {
            renderer.WriteMessage(model.Message);
            renderer.Write(model.Filter);
            if (_showConfirm)
            {
                renderer.Write(" Press Tab to confirm", ConsoleColor.Cyan);
            }

            for (int i = 0; i < model.Options.Count; i++)
            {
                Option currentOption = model.Options[i];
                var value = currentOption.Value;

                renderer.WriteLine();

                if (model.SelectedOptions.Contains(currentOption) && model.CurrentIndex != i && currentOption.Enabled)
                {
                    renderer.Write($"  {_arrowRight} ", ConsoleColor.Green);
                    renderer.Write($"{value}");
                }
                else if (model.CurrentIndex == i && currentOption.Enabled && !_disabledByLimit.Contains(currentOption))
                {
                    renderer.Write($"  {_arrowRight} {value}", ConsoleColor.Green);
                }
                // Check whether this option was disabled by default or whether it was disabled by the limiter
                else if (!currentOption.Enabled || _disabledByLimit.Contains(currentOption))
                {
                    renderer.Write($"    {value} (disabled)", ConsoleColor.DarkCyan);
                }
                else
                {
                    renderer.Write($"    {value}", Console.ForegroundColor);
                }
            }

        }

        private void FinishTemplate(ConsoleRenderer renderer, FinishTemplateModel model)
        {
            renderer.WriteMessage(model.Message);
            string joined = string.Join(", ", model.SelectedOptions.Select(res => res.Value));
            renderer.Write(joined, ConsoleColor.Cyan);
        }

        private class TemplateModel
        {
            public string Message { get; set; }
            public string Filter { get; set; }
            public IReadOnlyList<Option> Options { get; set; }
            public IEnumerable<Option> SelectedOptions { get; set; }
            public int CurrentIndex { get; set; }
        }

        private class FinishTemplateModel
        {
            public string Message { get; set; }
            public int CurrentIndex { get; set; }
            public IReadOnlyList<Option> Options { get; set; }
            public IEnumerable<Option> SelectedOptions { get; set; }
        }



        private class Option
        {
            public Option(string value, T item, bool enabled = true, bool selected = false)
            {
                Value = value;
                Item = item;
                Enabled = enabled;
                Selected = selected;
            }

            public string Value { get; }
            public T Item { get; }

            public bool Enabled { get; set; }
            public bool Selected { get; }
        }
    }
}
