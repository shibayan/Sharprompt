using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sharprompt
{
    public class ListOptions<T>
    {
        public string Message { get; set; }

        public bool AllowDuplicate { get; set; } = true;

        public bool ShowKeyNavigation { get; set; } = true;

        public IEnumerable<T> DefaultValues { get; set; }

        public bool ShowPagination { get; set; } = true;

        public int? PageSize { get; set; }

        public int Minimum { get; set; } = 1;

        public int Maximum { get; set; } = int.MaxValue;

        public bool RemoveAllMatch { get; set; } = false;

        public IList<Func<object, ValidationResult>> Validators { get; } = new List<Func<object, ValidationResult>>();

        public Func<T, string> TextSelector { get; set; } = x => x.ToString();

    }
}
