using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sharprompt
{
    public class InputOptions
    {
        public string Message { get; set; }

        public object DefaultValue { get; set; }

        public IList<Func<object, ValidationResult>> Validators { get; } = new List<Func<object, ValidationResult>>();
    }
}
