using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;

using Sharprompt.Forms;
using Sharprompt.Internal;
using Sharprompt.Strings;

namespace Sharprompt.Prompts;

public class DefaultPrompt : IPrompt
{
    public bool ThrowExceptionOnCancel { get; set; } = false;

    public CultureInfo Culture
    {
        get => Resource.Culture;
        set => Resource.Culture = value;
    }

    #region BasicPrompt

    public T Input<T>(InputOptions<T> options)
    {
        using var form = new InputForm<T>(options);

        return form.Start();
    }

    public T Input<T>(Action<InputOptions<T>> configure)
    {
        var options = new InputOptions<T>();

        configure(options);

        return Input(options);
    }

    public T Input<T>(string message, object defaultValue = default, string placeholder = default, IList<Func<object, ValidationResult>> validators = default)
    {
        return Input<T>(options =>
        {
            options.Message = message;
            options.Placeholder = placeholder;
            options.DefaultValue = defaultValue;

            options.Validators.Merge(validators);
        });
    }

    public string Password(PasswordOptions options)
    {
        using var form = new PasswordForm(options);

        return form.Start();
    }

    public string Password(Action<PasswordOptions> configure)
    {
        var options = new PasswordOptions();

        configure(options);

        return Password(options);
    }

    public string Password(string message, string passwordChar = "*", string placeholder = default, IList<Func<object, ValidationResult>> validators = default)
    {
        return Password(options =>
        {
            options.Message = message;
            options.Placeholder = placeholder;
            options.PasswordChar = passwordChar;

            options.Validators.Merge(validators);
        });
    }

    public bool Confirm(ConfirmOptions options)
    {
        using var form = new ConfirmForm(options);

        return form.Start();
    }

    public bool Confirm(Action<ConfirmOptions> configure)
    {
        var options = new ConfirmOptions();

        configure(options);

        return Confirm(options);
    }

    public bool Confirm(string message, bool? defaultValue = default)
    {
        return Confirm(options =>
        {
            options.Message = message;
            options.DefaultValue = defaultValue;
        });
    }

    public T Select<T>(SelectOptions<T> options)
    {
        using var form = new SelectForm<T>(options);

        return form.Start();
    }

    public T Select<T>(Action<SelectOptions<T>> configure)
    {
        var options = new SelectOptions<T>();

        configure(options);

        return Select(options);
    }

    public T Select<T>(string message, IEnumerable<T> items = default, int? pageSize = default, object defaultValue = default, Func<T, string> textSelector = default)
    {
        return Select<T>(options =>
        {
            options.Message = message;
            options.Items = items;
            options.DefaultValue = defaultValue;
            options.PageSize = pageSize;
            options.TextSelector = textSelector;
        });
    }

    public IEnumerable<T> MultiSelect<T>(MultiSelectOptions<T> options)
    {
        using var form = new MultiSelectForm<T>(options);

        return form.Start();
    }

    public IEnumerable<T> MultiSelect<T>(Action<MultiSelectOptions<T>> configure)
    {
        var options = new MultiSelectOptions<T>();

        configure(options);

        return MultiSelect(options);
    }

    public IEnumerable<T> MultiSelect<T>(string message, IEnumerable<T> items = null, int? pageSize = default, int minimum = 1, int maximum = int.MaxValue, IEnumerable<T> defaultValues = default, Func<T, string> textSelector = default)
    {
        return MultiSelect<T>(options =>
        {
            options.Message = message;
            options.Items = items;
            options.DefaultValues = defaultValues;
            options.PageSize = pageSize;
            options.Minimum = minimum;
            options.Maximum = maximum;
            options.TextSelector = textSelector;
        });
    }

    public IEnumerable<T> List<T>(ListOptions<T> options)
    {
        using var form = new ListForm<T>(options);

        return form.Start();
    }

    public IEnumerable<T> List<T>(Action<ListOptions<T>> configure)
    {
        var options = new ListOptions<T>();

        configure(options);

        return List(options);
    }

    public IEnumerable<T> List<T>(string message, int minimum = 1, int maximum = int.MaxValue, IList<Func<object, ValidationResult>> validators = default)
    {
        return List<T>(options =>
        {
            options.Message = message;
            options.Minimum = minimum;
            options.Maximum = maximum;

            options.Validators.Merge(validators);
        });
    }

    #endregion

    #region Bind

    public T Bind<T>() where T : new()
    {
        var model = new T();

        return Bind(model);
    }

    public T Bind<T>(T model)
    {
        StartBind(model);

        return model;
    }

    private void StartBind<T>(T model)
    {
        var propertyMetadatas = PropertyMetadataFactory.Create(model);

        foreach (var propertyMetadata in propertyMetadatas)
        {
            var formType = propertyMetadata.DetermineFormType();

            var result = formType switch
            {
                FormType.Confirm => MakeConfirm(propertyMetadata),
                FormType.Input => MakeInput(propertyMetadata),
                FormType.List => MakeList(propertyMetadata),
                FormType.MultiSelect => MakeMultiSelect(propertyMetadata),
                FormType.Password => MakePassword(propertyMetadata),
                FormType.Select => MakeSelect(propertyMetadata),
                _ => null
            };

            propertyMetadata.PropertyInfo.SetValue(model, result);
        }
    }

    private bool MakeConfirm(PropertyMetadata propertyMetadata)
    {
        return Confirm(options =>
        {
            options.Message = propertyMetadata.Message;
            options.DefaultValue = (bool?)propertyMetadata.DefaultValue;
        });
    }

    private object MakeInput(PropertyMetadata propertyMetadata) => InvokeMethod(nameof(MakeInputCore), propertyMetadata, propertyMetadata.PropertyInfo.PropertyType);

    private T MakeInputCore<T>(PropertyMetadata propertyMetadata)
    {
        return Input<T>(options =>
        {
            options.Message = propertyMetadata.Message;
            options.DefaultValue = propertyMetadata.DefaultValue;
            options.Placeholder = propertyMetadata.Placeholder;

            options.Validators.Merge(propertyMetadata.Validators);
        });
    }

    private object MakeList(PropertyMetadata propertyMetadata) => InvokeMethod(nameof(MakeListCore), propertyMetadata, propertyMetadata.ElementType);

    private IEnumerable<T> MakeListCore<T>(PropertyMetadata propertyMetadata)
    {
        return List<T>(options =>
        {
            options.Message = propertyMetadata.Message;
            options.DefaultValues = (IEnumerable<T>)propertyMetadata.DefaultValue;

            options.Validators.Merge(propertyMetadata.Validators);
        });
    }

    private object MakeMultiSelect(PropertyMetadata propertyMetadata) => InvokeMethod(nameof(MakeMultiSelectCore), propertyMetadata, propertyMetadata.ElementType);

    private IEnumerable<T> MakeMultiSelectCore<T>(PropertyMetadata propertyMetadata)
    {
        return MultiSelect<T>(options =>
        {
            options.Message = propertyMetadata.Message;
            options.Items = propertyMetadata.ItemsProvider?.GetItems<T>(propertyMetadata.PropertyInfo);
            options.DefaultValues = (IEnumerable<T>)propertyMetadata.DefaultValue;
        });
    }

    private string MakePassword(PropertyMetadata propertyMetadata)
    {
        return Password(options =>
        {
            options.Message = propertyMetadata.Message;
            options.Placeholder = propertyMetadata.Placeholder;

            options.Validators.Merge(propertyMetadata.Validators);
        });
    }

    private object MakeSelect(PropertyMetadata propertyMetadata) => InvokeMethod(nameof(MakeSelectCore), propertyMetadata);

    private T MakeSelectCore<T>(PropertyMetadata propertyMetadata)
    {
        return Select<T>(options =>
        {
            options.Message = propertyMetadata.Message;
            options.Items = propertyMetadata.ItemsProvider?.GetItems<T>(propertyMetadata.PropertyInfo);
            options.DefaultValue = propertyMetadata.DefaultValue;
        });
    }

    private object InvokeMethod(string name, PropertyMetadata propertyMetadata, Type genericType = default)
    {
        var method = typeof(Prompt).GetMethod(name, BindingFlags.NonPublic)
                                   .MakeGenericMethod(genericType ?? propertyMetadata.Type);

        return method.Invoke(null, new object[] { propertyMetadata });
    }

    #endregion
}
