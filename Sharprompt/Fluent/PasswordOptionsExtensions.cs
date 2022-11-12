using System;
using System.ComponentModel.DataAnnotations;

namespace Sharprompt.Fluent;

public static class PasswordOptionsExtensions
{
    public static PasswordOptions WithMessage(this PasswordOptions options, string message)
    {
        options.Message = message;

        return options;
    }

    public static PasswordOptions WithPlaceholder(this PasswordOptions options, string placeholder)
    {
        options.Placeholder = placeholder;

        return options;
    }

    public static PasswordOptions WithPasswordChar(this PasswordOptions options, string passwordChar)
    {
        options.PasswordChar = passwordChar;

        return options;
    }

    public static PasswordOptions AddValidators(this PasswordOptions options, params Func<object?, ValidationResult?>[] validators)
    {
        foreach (var validator in validators)
        {
            options.Validators.Add(validator);
        }

        return options;
    }
}
