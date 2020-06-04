using System;
using System.Collections.Generic;
using System.Linq;

using Sharprompt.Internal;

namespace Sharprompt.Forms
{
    internal class MultiSelect<T> : FormBase<IEnumerable<T>>
    {
        public MultiSelect(string message, IEnumerable<T> options, int limit, int min, int pageSize, Func<T, string> valueSelector)
            : base(false)
        {
            // throw early when invalid options are passed
            if (min < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(min), $"The min ({min}) is not valid");
            }

            if (limit != -1 && limit < min)
            {
                throw new ArgumentException($"The limit ({limit}) is not valid when min is set to ({min})", nameof(limit));
            }

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

        private IReadOnlyList<Option> _disabledByLimit = new List<Option>();
        private bool _showConfirm;

        private const string ArrowRight = "\u0010";

        private bool IsOptionEnabled(T o)
        {
            var boxed = typeof(T).GetProperty("Enabled")?.GetValue(o, null);

            if (boxed == null) return true;

            return (bool)boxed;
        }

        public override IEnumerable<T> Start()
        {
            // Defaults
            var selectedOptions = new List<Option>();
            var options = _baseOptions;
            var filteredOptions = _baseOptions;
            int currentIndex = 0;
            string filter = "", prevFilter = "";
            int prevPage = -1, pageCount = (options.Count - 1) / _pageSize + 1;
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

                    options = (filteredOptions).Skip(currentPage * _pageSize)
                                               .Take(_pageSize)
                                               .ToArray();

                    currentIndex = 0;
                    prevPage = currentPage;
                }

                Scope.Render(Template, new TemplateModel { Message = _message, Filter = filter, SelectedOptions = selectedOptions, Options = options, CurrentIndex = currentIndex });

                var keyInfo = Scope.ReadKey();

                if (keyInfo.Key == ConsoleKey.Enter)
                {
                    // Prevents selecting when filter does not match any items
                    if (options.Count == 0) continue;

                    var currentOption = options[currentIndex];

                    if (!selectedOptions.Contains(currentOption))
                    {
                        selectedOptions.Add(options[currentIndex]);
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
                    currentPage = currentPage <= 0 ? pageCount - 1 : currentPage - 1;
                }
                else if (keyInfo.Key == ConsoleKey.RightArrow)
                {
                    currentPage = currentPage >= pageCount - 1 ? 0 : currentPage + 1;
                }
                else if (keyInfo.Key == ConsoleKey.Backspace)
                {
                    if (filter.Length == 0)
                    {
                        Scope.Beep();
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
                else if (keyInfo.Key == ConsoleKey.Tab)
                {
                    if (selectedOptions.Count >= _min)
                    {
                        break;
                    }

                    Scope.SetError(new ValidationError($"A minimum selection of {_min} items is required"));
                }
                else if (keyInfo.Key == ConsoleKey.UpArrow)
                {
                    if (options.All(o => _disabledByLimit.Contains(o))) continue;

                    currentIndex = (currentIndex <= 0) ? options.Count - 1 : currentIndex - 1;
                    while (!options[currentIndex].Enabled || _disabledByLimit.Contains(options[currentIndex]))
                    {
                        currentIndex = (currentIndex <= 0) ? options.Count - 1 : currentIndex - 1;
                    };
                }
                else if (keyInfo.Key == ConsoleKey.DownArrow)
                {
                    if (options.All(o => _disabledByLimit.Contains(o))) continue;

                    currentIndex = (currentIndex >= options.Count - 1) ? 0 : currentIndex + 1;
                    while (!options[currentIndex].Enabled || _disabledByLimit.Contains(options[currentIndex]))
                    {
                        currentIndex = currentIndex >= options.Count - 1 ? 0 : currentIndex + 1;
                    };
                }
            }

            Scope.Render(FinishTemplate, new FinishTemplateModel { Message = _message, SelectedOptions = selectedOptions, Options = options, CurrentIndex = currentIndex });

            return _baseOptions.Where(o => selectedOptions.Contains(o)).Select(x => x.Item);
        }

        private void Template(ConsoleRenderer renderer, TemplateModel model)
        {
            renderer.WriteMessage(model.Message);
            renderer.Write(model.Filter);

            if (_showConfirm && string.IsNullOrEmpty(model.Filter))
            {
                renderer.Write(" Press Tab to confirm", Prompt.ColorSchema.Answer);
            }

            for (int i = 0; i < model.Options.Count; i++)
            {
                var currentOption = model.Options[i];
                var value = currentOption.Value;

                renderer.WriteLine();

                if (model.SelectedOptions.Contains(currentOption) && model.CurrentIndex != i && currentOption.Enabled)
                {
                    renderer.Write($"  {ArrowRight} ", Prompt.ColorSchema.Select);
                    renderer.Write($"{value}");
                }
                else if (model.CurrentIndex == i && currentOption.Enabled && !_disabledByLimit.Contains(currentOption))
                {
                    renderer.Write($"  {ArrowRight} {value}", Prompt.ColorSchema.Select);
                }
                // Check whether this option was disabled by default or whether it was disabled by the limiter
                else if (!currentOption.Enabled || _disabledByLimit.Contains(currentOption))
                {
                    renderer.Write($"    {value} (disabled)", Prompt.ColorSchema.DisabledOption);
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
            var joined = string.Join(", ", model.SelectedOptions.Select(res => res.Value));
            renderer.Write(joined, Prompt.ColorSchema.Answer);
        }

        private class TemplateModel
        {
            public string Message { get; set; }
            public string Filter { get; set; }
            public IReadOnlyList<Option> Options { get; set; }
            public IReadOnlyList<Option> SelectedOptions { get; set; }
            public int CurrentIndex { get; set; }
        }

        private class FinishTemplateModel
        {
            public string Message { get; set; }
            public int CurrentIndex { get; set; }
            public IReadOnlyList<Option> Options { get; set; }
            public IReadOnlyList<Option> SelectedOptions { get; set; }
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
