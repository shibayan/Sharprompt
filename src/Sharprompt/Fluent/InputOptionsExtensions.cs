using System;
using System.ComponentModel.DataAnnotations;

namespace Sharprompt.Fluent;

public static class InputOptionsExtensions
{
    public static InputOptions<T> WithMessage<T>(this InputOptions<T> options, string message)
    {
        options.Message = message;

        return options;
    }

    public static InputOptions<T> WithPlaceholder<T>(this InputOptions<T> options, string placeholder)
    {
        options.Placeholder = placeholder;

        return options;
    }

    public static InputOptions<T> WithDefaultValue<T>(this InputOptions<T> options, T defaultValue)
    {
        options.DefaultValue = defaultValue;

        return options;
    }

    public static InputOptions<T> AddValidators<T>(this InputOptions<T> options, params Func<object?, ValidationResult?>[] validators)
    {
        foreach (var validator in validators)
        {
            options.Validators.Add(validator);
        }

        return options;
    }
}
