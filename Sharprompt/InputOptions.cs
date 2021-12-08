using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sharprompt
{
    public class InputOptions
    {
        public string Message { get; set; }

        public string Placeholder { get; set; }

        public object DefaultValue { get; set; }

        public IList<Func<object, ValidationResult>> Validators { get; } = new List<Func<object, ValidationResult>>();

        public InputOptions SetMessage(string message)
        {
            Message = message;

            return this;
        }

        public InputOptions SetPlaceholder(string placeholder)
        {
            Placeholder = placeholder;

            return this;
        }

        public InputOptions SetDefaultValue(object defaultValue)
        {
            DefaultValue = defaultValue;

            return this;
        }

        public InputOptions AddValidators(params Func<object, ValidationResult>[] validators)
        {
            foreach (var validator in validators)
            {
                Validators.Add(validator);
            }

            return this;
        }
    }
}
