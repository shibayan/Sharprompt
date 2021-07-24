using System;
using System.Collections.Generic;

namespace Sharprompt
{
    public class MultiSelectOptions<T>
    {
        public string Message { get; set; }

        public IEnumerable<T> Items { get; set; }

        public IEnumerable<T> DefaultValues { get; set; }

        public int? PageSize { get; set; }

        public int Minimum { get; set; } = 1;

        public int Maximum { get; set; } = int.MaxValue;

        public Func<T, string> TextSelector { get; set; } = x => x.ToString();
    }
}
