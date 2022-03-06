using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sharprompt.Fluent;

public static class ListOptionsExtensions
{
    public static ListOptions<T> WithMessage<T>(this ListOptions<T> options, string message)
    {
        options.Message = message;

        return options;
    }

    public static ListOptions<T> WithDefaultValues<T>(this ListOptions<T> options, IEnumerable<T> defaultValues)
    {
        options.DefaultValues = defaultValues;

        return options;
    }

    public static ListOptions<T> WithMinimum<T>(this ListOptions<T> options, int minimum)
    {
        options.Minimum = minimum;

        return options;
    }

    public static ListOptions<T> WithMaximum<T>(this ListOptions<T> options, int maximum)
    {
        options.Maximum = maximum;

        return options;
    }

    public static ListOptions<T> AddValidators<T>(this ListOptions<T> options, params Func<object, ValidationResult>[] validators)
    {
        foreach (var validator in validators)
        {
            options.Validators.Add(validator);
        }

        return options;
    }
}
