using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Sharprompt.Forms;
using Sharprompt.Internal;

namespace Sharprompt;

public static partial class Prompt
{
    public static T Input<T>(InputOptions<T> options)
    {
        using var form = new InputForm<T>(options);

        return form.Start();
    }

    public static T Input<T>(Action<InputOptions<T>> configure)
    {
        var options = new InputOptions<T>();

        configure(options);

        return Input(options);
    }

    public static T Input<T>(string message, object? defaultValue = default, string? placeholder = default, IList<Func<object?, ValidationResult?>>? validators = default)
    {
        return Input<T>(options =>
        {
            options.Message = message;
            options.Placeholder = placeholder;
            options.DefaultValue = defaultValue;

            options.Validators.Merge(validators);
        });
    }

    public static string Password(PasswordOptions options)
    {
        using var form = new PasswordForm(options);

        return form.Start();
    }

    public static string Password(Action<PasswordOptions> configure)
    {
        var options = new PasswordOptions();

        configure(options);

        return Password(options);
    }

    public static string Password(string message, string passwordChar = "*", string? placeholder = default, IList<Func<object?, ValidationResult?>>? validators = default)
    {
        return Password(options =>
        {
            options.Message = message;
            options.Placeholder = placeholder;
            options.PasswordChar = passwordChar;

            options.Validators.Merge(validators);
        });
    }

    public static bool Confirm(ConfirmOptions options)
    {
        using var form = new ConfirmForm(options);

        return form.Start();
    }

    public static bool Confirm(Action<ConfirmOptions> configure)
    {
        var options = new ConfirmOptions();

        configure(options);

        return Confirm(options);
    }

    public static bool Confirm(string message, bool? defaultValue = default)
    {
        return Confirm(options =>
        {
            options.Message = message;
            options.DefaultValue = defaultValue;
        });
    }

    public static T Select<T>(SelectOptions<T> options) where T : notnull
    {
        using var form = new SelectForm<T>(options);

        return form.Start();
    }

    public static T Select<T>(Action<SelectOptions<T>> configure) where T : notnull
    {
        var options = new SelectOptions<T>();

        configure(options);

        return Select(options);
    }

    public static T Select<T>(string message, IEnumerable<T>? items = default, int pageSize = int.MaxValue, object? defaultValue = default, Func<T, string>? textSelector = default) where T : notnull
    {
        return Select<T>(options =>
        {
            options.Message = message;

            if (items is not null)
            {
                options.Items = items;
            }

            options.DefaultValue = defaultValue;
            options.PageSize = pageSize;

            if (textSelector is not null)
            {
                options.TextSelector = textSelector;
            }
        });
    }

    public static IEnumerable<T> MultiSelect<T>(MultiSelectOptions<T> options) where T : notnull
    {
        using var form = new MultiSelectForm<T>(options);

        return form.Start();
    }

    public static IEnumerable<T> MultiSelect<T>(Action<MultiSelectOptions<T>> configure) where T : notnull
    {
        var options = new MultiSelectOptions<T>();

        configure(options);

        return MultiSelect(options);
    }

    public static IEnumerable<T> MultiSelect<T>(string message, IEnumerable<T>? items = null, int pageSize = int.MaxValue, int minimum = 1, int maximum = int.MaxValue, IEnumerable<T>? defaultValues = default, Func<T, string>? textSelector = default) where T : notnull
    {
        return MultiSelect<T>(options =>
        {
            options.Message = message;

            if (items is not null)
            {
                options.Items = items;
            }

            if (defaultValues is not null)
            {
                options.DefaultValues = defaultValues;
            }

            options.PageSize = pageSize;
            options.Minimum = minimum;
            options.Maximum = maximum;

            if (textSelector is not null)
            {
                options.TextSelector = textSelector;
            }
        });
    }

    public static IEnumerable<T> List<T>(ListOptions<T> options) where T : notnull
    {
        using var form = new ListForm<T>(options);

        return form.Start();
    }

    public static IEnumerable<T> List<T>(Action<ListOptions<T>> configure) where T : notnull
    {
        var options = new ListOptions<T>();

        configure(options);

        return List(options);
    }

    public static IEnumerable<T> List<T>(string message, int minimum = 1, int maximum = int.MaxValue, IList<Func<object?, ValidationResult?>>? validators = default) where T : notnull
    {
        return List<T>(options =>
        {
            options.Message = message;
            options.Minimum = minimum;
            options.Maximum = maximum;

            options.Validators.Merge(validators);
        });
    }
}
