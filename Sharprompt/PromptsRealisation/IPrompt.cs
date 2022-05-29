using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Sharprompt.Prompts;

public interface IPromptBasic
{
    public T Input<T>(InputOptions<T> options);

    public T Input<T>(Action<InputOptions<T>> configure);

    public T Input<T>(string message, object defaultValue = default, string placeholder = default,
        IList<Func<object, ValidationResult>> validators = default);

    public string Password(PasswordOptions options);

    public string Password(Action<PasswordOptions> configure);

    public string Password(string message, string passwordChar = "*", string placeholder = default,
        IList<Func<object, ValidationResult>> validators = default);

    public bool Confirm(ConfirmOptions options);

    public bool Confirm(Action<ConfirmOptions> configure);

    public bool Confirm(string message, bool? defaultValue = default);

    public T Select<T>(SelectOptions<T> options);

    public T Select<T>(Action<SelectOptions<T>> configure);

    public T Select<T>(string message, IEnumerable<T> items = default, int? pageSize = default,
        object defaultValue = default, Func<T, string> textSelector = default);

    public IEnumerable<T> MultiSelect<T>(MultiSelectOptions<T> options);

    public IEnumerable<T> MultiSelect<T>(Action<MultiSelectOptions<T>> configure);

    public IEnumerable<T> MultiSelect<T>(string message, IEnumerable<T> items = null, int? pageSize = default,
        int minimum = 1, int maximum = int.MaxValue, IEnumerable<T> defaultValues = default,
        Func<T, string> textSelector = default);

    public IEnumerable<T> List<T>(ListOptions<T> options);

    public IEnumerable<T> List<T>(Action<ListOptions<T>> configure);

    public IEnumerable<T> List<T>(string message, int minimum = 1, int maximum = int.MaxValue,
        IList<Func<object, ValidationResult>> validators = default);
}

public interface IPromptBind
{
    public T Bind<T>() where T : new();

    public T Bind<T>(T model);
}

public interface IPromptConfig
{
    public bool ThrowExceptionOnCancel { get; set; }

    public CultureInfo Culture { get; set; }
}

public interface IPrompt : IPromptBasic, IPromptBind, IPromptConfig { }
