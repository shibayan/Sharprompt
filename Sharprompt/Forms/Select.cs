using System;
using System.Collections.Generic;
using System.Linq;

using Sharprompt.Internal;

namespace Sharprompt.Forms
{
    internal class Select<T> : FormBase<T>
    {
        public Select(string message, IEnumerable<T> options, object defaultValue, int pageSize, Func<T, string> valueSelector)
        : base(false)
        {
            _message = message;
            _baseOptions = options.Select(x => new Option(valueSelector(x), x)).ToArray();
            _defaultValue = defaultValue;
            _pageSize = pageSize;
            _filtering = (filter, value) => value.IndexOf(filter, StringComparison.OrdinalIgnoreCase) != -1;
        }

        private readonly string _message;
        private readonly IReadOnlyList<Option> _baseOptions;
        private readonly object _defaultValue;
        private readonly int _pageSize;
        private readonly Func<string, string, bool> _filtering;

        public override T Start()
        {
            var options = _baseOptions;
            var filteredOptions = _baseOptions;

            int selectedIndex = FindDefaultIndex(options, _defaultValue);

            var filter = "";
            var prevFilter = "";

            int prevPage = -1;
            var pageCount = (options.Count - 1) / _pageSize + 1;
            // Only resolve the page number when the option is neither 0 nor negative.
            int currentPage = (selectedIndex == 0 || selectedIndex == -1) ? 0 : GetPageFromIndex(options, selectedIndex);

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
                    options = filteredOptions.Skip(currentPage * _pageSize)
                                             .Take(_pageSize)
                                             .ToArray();

                    // Initially, we need to check for the default index. After moving the page or default index this becomes irrelevant.
                    selectedIndex = prevPage == -1 && selectedIndex != -1 ? FindDefaultIndex(options, _baseOptions[selectedIndex].Item) : 0;

                    prevPage = currentPage;
                }

                Scope.Render(Template, new TemplateModel { Message = _message, Filter = filter, SelectedIndex = selectedIndex, Options = options });

                var keyInfo = Scope.ReadKey();

                if (keyInfo.Key == ConsoleKey.Enter)
                {
                    if (selectedIndex != -1)
                    {
                        break;
                    }

                    Scope.SetError(new ValidationError("Value is required"));
                }
                else if (keyInfo.Key == ConsoleKey.UpArrow)
                {
                    selectedIndex = selectedIndex <= 0 ? options.Count - 1 : selectedIndex - 1;
                }
                else if (keyInfo.Key == ConsoleKey.DownArrow)
                {
                    selectedIndex = selectedIndex >= options.Count - 1 ? 0 : selectedIndex + 1;
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
            }

            Scope.Render(FinishTemplate, new FinishTemplateModel { Message = _message, SelectedIndex = selectedIndex, Options = options });

            return options[selectedIndex].Item;
        }

        private int FindDefaultIndex(IReadOnlyList<Option> list, object item)
        {
            if (item == null)
            {
                return 0;
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

        private int GetPageFromIndex(IReadOnlyList<Option> list, int index)
        {
            int total = list.Count - 1;
            int currentPage = 0;

            for (int i = _pageSize; i <= total; i += _pageSize)
            {
                currentPage++;
            }
            return currentPage;
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
                    renderer.Write($"> {value}", Prompt.ColorSchema.Select);
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
            renderer.Write(model.Options[model.SelectedIndex].Value, Prompt.ColorSchema.Answer);
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
