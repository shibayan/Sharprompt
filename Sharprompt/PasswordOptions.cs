using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sharprompt
{
    public class PasswordOptions
    {
        public string Message { get; set; }

        public string Placeholder { get; set; }

        public string PasswordChar { get; set; } = "*";

        public IList<Func<object, ValidationResult>> Validators { get; } = new List<Func<object, ValidationResult>>();

        public PasswordOptions SetMessage(string message)
        {
            Message = message;

            return this;
        }

        public PasswordOptions SetPlaceholder(string placeholder)
        {
            Placeholder = placeholder;

            return this;
        }

        public PasswordOptions SetPasswordChar(string passwordChar)
        {
            PasswordChar = passwordChar;

            return this;
        }

        public PasswordOptions AddValidators(params Func<object, ValidationResult>[] validators)
        {
            foreach (var validator in validators)
            {
                Validators.Add(validator);
            }

            return this;
        }
    }
}
