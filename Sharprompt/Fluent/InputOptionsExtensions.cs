using System;
using System.ComponentModel.DataAnnotations;

namespace Sharprompt.Fluent
{
    public static class InputOptionsExtensions
    {
        public static InputOptions SetMessage(this InputOptions options, string message)
        {
            options.Message = message;

            return options;
        }

        public static InputOptions SetPlaceholder(this InputOptions options, string placeholder)
        {
            options.Placeholder = placeholder;

            return options;
        }

        public static InputOptions SetDefaultValue(this InputOptions options, object defaultValue)
        {
            options.DefaultValue = defaultValue;

            return options;
        }

        public static InputOptions AddValidators(this InputOptions options, params Func<object, ValidationResult>[] validators)
        {
            foreach (var validator in validators)
            {
                options.Validators.Add(validator);
            }

            return options;
        }
    }
}
