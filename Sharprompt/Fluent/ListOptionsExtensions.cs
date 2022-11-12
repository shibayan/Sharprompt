using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sharprompt.Fluent;

public static class ListOptionsExtensions
{
    public static ListOptions<T> WithMessage<T>(this ListOptions<T> options, string message) where T : notnull
    {
        options.Message = message;

        return options;
    }

    public static ListOptions<T> WithDefaultValues<T>(this ListOptions<T> options, IEnumerable<T> defaultValues) where T : notnull
    {
        options.DefaultValues = defaultValues;

        return options;
    }

    public static ListOptions<T> WithMinimum<T>(this ListOptions<T> options, int minimum) where T : notnull
    {
        options.Minimum = minimum;

        return options;
    }

    public static ListOptions<T> WithMaximum<T>(this ListOptions<T> options, int maximum) where T : notnull
    {
        options.Maximum = maximum;

        return options;
    }

    public static ListOptions<T> AddValidators<T>(this ListOptions<T> options, params Func<object?, ValidationResult?>[] validators) where T : notnull
    {
        foreach (var validator in validators)
        {
            options.Validators.Add(validator);
        }

        return options;
    }
}
