using System;
using System.ComponentModel.DataAnnotations;

namespace Sharprompt.Fluent
{
    public static class PasswordOptionsExtensions
    {
        public static PasswordOptions SetMessage(this PasswordOptions options, string message)
        {
            options.Message = message;

            return options;
        }

        public static PasswordOptions SetPlaceholder(this PasswordOptions options, string placeholder)
        {
            options.Placeholder = placeholder;

            return options;
        }

        public static PasswordOptions SetPasswordChar(this PasswordOptions options, string passwordChar)
        {
            options.PasswordChar = passwordChar;

            return options;
        }

        public static PasswordOptions AddValidators(this PasswordOptions options, params Func<object, ValidationResult>[] validators)
        {
            foreach (var validator in validators)
            {
                options.Validators.Add(validator);
            }

            return options;
        }
    }
}
