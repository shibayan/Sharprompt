using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Sharprompt.Prompts;

namespace Sharprompt;

public static partial class Prompt
{
    public static IPrompt PromptRealisation { get; set; } = new DefaultPrompt();

    public static T Input<T>(InputOptions<T> options)
        => PromptRealisation.Input(options);

    public static T Input<T>(Action<InputOptions<T>> configure)
        => PromptRealisation.Input(configure);

    public static T Input<T>(string message, object defaultValue = default,
        string placeholder = default, IList<Func<object, ValidationResult>> validators = default)
        => PromptRealisation.Input<T>(message, defaultValue, placeholder, validators);

    public static string Password(PasswordOptions options)
        => PromptRealisation.Password(options);

    public static string Password(Action<PasswordOptions> configure)
        => PromptRealisation.Password(configure);

    public static string Password(string message, string passwordChar = "*",
        string placeholder = default, IList<Func<object, ValidationResult>> validators = default)
        => PromptRealisation.Password(message, passwordChar, placeholder, validators);

    public static bool Confirm(ConfirmOptions options)
        => PromptRealisation.Confirm(options);

    public static bool Confirm(Action<ConfirmOptions> configure)
        => PromptRealisation.Confirm(configure);

    public static bool Confirm(string message, bool? defaultValue = default)
        => PromptRealisation.Confirm(message, defaultValue);

    public static T Select<T>(SelectOptions<T> options)
        => PromptRealisation.Select(options);

    public static T Select<T>(Action<SelectOptions<T>> configure)
        => PromptRealisation.Select(configure);

    public static T Select<T>(string message, IEnumerable<T> items = default,
        int? pageSize = default, object defaultValue = default, Func<T, string> textSelector = default)
        => PromptRealisation.Select(message, items, pageSize, defaultValue, textSelector);

    public static IEnumerable<T> MultiSelect<T>(MultiSelectOptions<T> options)
        => PromptRealisation.MultiSelect(options);

    public static IEnumerable<T> MultiSelect<T>(Action<MultiSelectOptions<T>> configure)
        => PromptRealisation.MultiSelect(configure);

    public static IEnumerable<T> MultiSelect<T>(string message, IEnumerable<T> items = null, int? pageSize = default,
        int minimum = 1, int maximum = int.MaxValue, IEnumerable<T> defaultValues = default,
        Func<T, string> textSelector = default)
        => PromptRealisation.MultiSelect(message, items, pageSize, minimum, maximum, defaultValues, textSelector);

    public static IEnumerable<T> List<T>(ListOptions<T> options)
        => PromptRealisation.List(options);

    public static IEnumerable<T> List<T>(Action<ListOptions<T>> configure)
        => PromptRealisation.List(configure);

    public static IEnumerable<T> List<T>(string message, int minimum = 1, int maximum = int.MaxValue,
        IList<Func<object, ValidationResult>> validators = default)
        => PromptRealisation.List<T>(message, minimum, maximum, validators);
}
