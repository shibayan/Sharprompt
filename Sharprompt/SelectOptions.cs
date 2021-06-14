using System;
using System.Collections.Generic;

namespace Sharprompt
{
    public class SelectOptions<T>
    {
        public string Message { get; set; }

        public IEnumerable<T> Items { get; set; }

        public object DefaultValue { get; set; }

        public int? PageSize { get; set; }

        public Func<T, string> TextSelector { get; set; } = x => x.ToString();
    }
}
