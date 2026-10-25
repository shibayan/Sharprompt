using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;

using Sharprompt.Forms;
using Sharprompt.Internal;

namespace Sharprompt;

/// <summary>
/// Displays synchronous interactive prompts. The default driver requires terminal input and output.
/// </summary>
/// <remarks>
/// Settings are process-wide. Ctrl+C and Escape exit the process unless ThrowExceptionOnCancel is enabled.
/// Use an options or configure overload to pass a CancellationToken; cancellation throws OperationCanceledException.
/// </remarks>
/// <example>
/// <code>
/// using Sharprompt;
/// var name = Prompt.Input&lt;string&gt;("Name", validators: new[] { Validators.Required() });
/// var city = Prompt.Select("City", new[] { "Tokyo", "London" });
/// </code>
/// </example>
public static partial class Prompt
{
    /// <summary>
    /// Reads text and converts it to T using an invariant-culture TypeConverter.
    /// </summary>
    /// <typeparam name="T">The input or item type.</typeparam>
    /// <param name="options">The prompt settings, including the required message.</param>
    /// <returns>The converted input or accepted default value.</returns>
    /// <exception cref="InvalidOperationException">The default driver cannot use redirected input or output, or the driver factory returns null.</exception>
    /// <exception cref="PromptCanceledException">The user presses Ctrl+C or Escape and ThrowExceptionOnCancel is true.</exception>
    public static T Input<T>(InputOptions<T> options)
    {
        return Input(options, CancellationToken.None);
    }

    /// <summary>
    /// Reads text and converts it to T using an invariant-culture TypeConverter.
    /// </summary>
    /// <typeparam name="T">The input or item type.</typeparam>
    /// <param name="options">The prompt settings, including the required message.</param>
    /// <param name="cancellationToken">Cancels the prompt while waiting for input, independently of ThrowExceptionOnCancel.</param>
    /// <returns>The converted input or accepted default value.</returns>
    /// <exception cref="InvalidOperationException">The default driver cannot use redirected input or output, or the driver factory returns null.</exception>
    /// <exception cref="PromptCanceledException">The user presses Ctrl+C or Escape and ThrowExceptionOnCancel is true.</exception>
    /// <exception cref="OperationCanceledException">The cancellation token is canceled.</exception>
    public static T Input<T>(InputOptions<T> options, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var form = new InputForm<T>(options, s_configuration);

        return form.Start(cancellationToken);
    }

    /// <summary>
    /// Reads text and converts it to T using an invariant-culture TypeConverter.
    /// </summary>
    /// <typeparam name="T">The input or item type.</typeparam>
    /// <param name="configure">Configures a new options instance before the prompt starts.</param>
    /// <returns>The converted input or accepted default value.</returns>
    /// <exception cref="InvalidOperationException">The default driver cannot use redirected input or output, or the driver factory returns null.</exception>
    /// <exception cref="PromptCanceledException">The user presses Ctrl+C or Escape and ThrowExceptionOnCancel is true.</exception>
    public static T Input<T>(Action<InputOptions<T>> configure)
    {
        return Input(configure, CancellationToken.None);
    }

    /// <summary>
    /// Reads text and converts it to T using an invariant-culture TypeConverter.
    /// </summary>
    /// <typeparam name="T">The input or item type.</typeparam>
    /// <param name="configure">Configures a new options instance before the prompt starts.</param>
    /// <param name="cancellationToken">Cancels the prompt while waiting for input, independently of ThrowExceptionOnCancel.</param>
    /// <returns>The converted input or accepted default value.</returns>
    /// <exception cref="InvalidOperationException">The default driver cannot use redirected input or output, or the driver factory returns null.</exception>
    /// <exception cref="PromptCanceledException">The user presses Ctrl+C or Escape and ThrowExceptionOnCancel is true.</exception>
    /// <exception cref="OperationCanceledException">The cancellation token is canceled.</exception>
    public static T Input<T>(Action<InputOptions<T>> configure, CancellationToken cancellationToken)
    {
        var options = new InputOptions<T>();

        configure(options);

        return Input(options, cancellationToken);
    }

    /// <summary>
    /// Reads text and converts it to T using an invariant-culture TypeConverter.
    /// </summary>
    /// <typeparam name="T">The input or item type.</typeparam>
    /// <param name="message">The message shown before the input.</param>
    /// <param name="defaultValue">The default value; null means no default. For Input and Select it must be compatible with T; compatible primitive conversions and integral enum values are accepted.</param>
    /// <param name="placeholder">The hint shown when no text has been entered.</param>
    /// <param name="validators">Validators applied to the converted value or password; null means no additional validators.</param>
    /// <returns>The converted input or accepted default value.</returns>
    /// <exception cref="InvalidOperationException">The default driver cannot use redirected input or output, or the driver factory returns null.</exception>
    /// <exception cref="PromptCanceledException">The user presses Ctrl+C or Escape and ThrowExceptionOnCancel is true.</exception>
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

    /// <summary>
    /// Reads a masked password and returns the unencrypted string.
    /// </summary>
    /// <param name="options">The prompt settings, including the required message.</param>
    /// <returns>The entered password.</returns>
    /// <exception cref="InvalidOperationException">The default driver cannot use redirected input or output, or the driver factory returns null.</exception>
    /// <exception cref="PromptCanceledException">The user presses Ctrl+C or Escape and ThrowExceptionOnCancel is true.</exception>
    public static string Password(PasswordOptions options)
    {
        return Password(options, CancellationToken.None);
    }

    /// <summary>
    /// Reads a masked password and returns the unencrypted string.
    /// </summary>
    /// <param name="options">The prompt settings, including the required message.</param>
    /// <param name="cancellationToken">Cancels the prompt while waiting for input, independently of ThrowExceptionOnCancel.</param>
    /// <returns>The entered password.</returns>
    /// <exception cref="InvalidOperationException">The default driver cannot use redirected input or output, or the driver factory returns null.</exception>
    /// <exception cref="PromptCanceledException">The user presses Ctrl+C or Escape and ThrowExceptionOnCancel is true.</exception>
    /// <exception cref="OperationCanceledException">The cancellation token is canceled.</exception>
    public static string Password(PasswordOptions options, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var form = new PasswordForm(options, s_configuration);

        return form.Start(cancellationToken);
    }

    /// <summary>
    /// Reads a masked password and returns the unencrypted string.
    /// </summary>
    /// <param name="configure">Configures a new options instance before the prompt starts.</param>
    /// <returns>The entered password.</returns>
    /// <exception cref="InvalidOperationException">The default driver cannot use redirected input or output, or the driver factory returns null.</exception>
    /// <exception cref="PromptCanceledException">The user presses Ctrl+C or Escape and ThrowExceptionOnCancel is true.</exception>
    public static string Password(Action<PasswordOptions> configure)
    {
        return Password(configure, CancellationToken.None);
    }

    /// <summary>
    /// Reads a masked password and returns the unencrypted string.
    /// </summary>
    /// <param name="configure">Configures a new options instance before the prompt starts.</param>
    /// <param name="cancellationToken">Cancels the prompt while waiting for input, independently of ThrowExceptionOnCancel.</param>
    /// <returns>The entered password.</returns>
    /// <exception cref="InvalidOperationException">The default driver cannot use redirected input or output, or the driver factory returns null.</exception>
    /// <exception cref="PromptCanceledException">The user presses Ctrl+C or Escape and ThrowExceptionOnCancel is true.</exception>
    /// <exception cref="OperationCanceledException">The cancellation token is canceled.</exception>
    public static string Password(Action<PasswordOptions> configure, CancellationToken cancellationToken)
    {
        var options = new PasswordOptions();

        configure(options);

        return Password(options, cancellationToken);
    }

    /// <summary>
    /// Reads a masked password and returns the unencrypted string.
    /// </summary>
    /// <param name="message">The message shown before the input.</param>
    /// <param name="placeholder">The hint shown when no text has been entered.</param>
    /// <param name="validators">Validators applied to the converted value or password; null means no additional validators.</param>
    /// <param name="passwordChar">The string displayed in place of each password character. Defaults to "*".</param>
    /// <returns>The entered password.</returns>
    /// <exception cref="InvalidOperationException">The default driver cannot use redirected input or output, or the driver factory returns null.</exception>
    /// <exception cref="PromptCanceledException">The user presses Ctrl+C or Escape and ThrowExceptionOnCancel is true.</exception>
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

    /// <summary>
    /// Asks for a yes-or-no answer.
    /// </summary>
    /// <param name="options">The prompt settings, including the required message.</param>
    /// <returns>True for yes; false for no.</returns>
    /// <exception cref="InvalidOperationException">The default driver cannot use redirected input or output, or the driver factory returns null.</exception>
    /// <exception cref="PromptCanceledException">The user presses Ctrl+C or Escape and ThrowExceptionOnCancel is true.</exception>
    public static bool Confirm(ConfirmOptions options)
    {
        return Confirm(options, CancellationToken.None);
    }

    /// <summary>
    /// Asks for a yes-or-no answer.
    /// </summary>
    /// <param name="options">The prompt settings, including the required message.</param>
    /// <param name="cancellationToken">Cancels the prompt while waiting for input, independently of ThrowExceptionOnCancel.</param>
    /// <returns>True for yes; false for no.</returns>
    /// <exception cref="InvalidOperationException">The default driver cannot use redirected input or output, or the driver factory returns null.</exception>
    /// <exception cref="PromptCanceledException">The user presses Ctrl+C or Escape and ThrowExceptionOnCancel is true.</exception>
    /// <exception cref="OperationCanceledException">The cancellation token is canceled.</exception>
    public static bool Confirm(ConfirmOptions options, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var form = new ConfirmForm(options, s_configuration);

        return form.Start(cancellationToken);
    }

    /// <summary>
    /// Asks for a yes-or-no answer.
    /// </summary>
    /// <param name="configure">Configures a new options instance before the prompt starts.</param>
    /// <returns>True for yes; false for no.</returns>
    /// <exception cref="InvalidOperationException">The default driver cannot use redirected input or output, or the driver factory returns null.</exception>
    /// <exception cref="PromptCanceledException">The user presses Ctrl+C or Escape and ThrowExceptionOnCancel is true.</exception>
    public static bool Confirm(Action<ConfirmOptions> configure)
    {
        return Confirm(configure, CancellationToken.None);
    }

    /// <summary>
    /// Asks for a yes-or-no answer.
    /// </summary>
    /// <param name="configure">Configures a new options instance before the prompt starts.</param>
    /// <param name="cancellationToken">Cancels the prompt while waiting for input, independently of ThrowExceptionOnCancel.</param>
    /// <returns>True for yes; false for no.</returns>
    /// <exception cref="InvalidOperationException">The default driver cannot use redirected input or output, or the driver factory returns null.</exception>
    /// <exception cref="PromptCanceledException">The user presses Ctrl+C or Escape and ThrowExceptionOnCancel is true.</exception>
    /// <exception cref="OperationCanceledException">The cancellation token is canceled.</exception>
    public static bool Confirm(Action<ConfirmOptions> configure, CancellationToken cancellationToken)
    {
        var options = new ConfirmOptions();

        configure(options);

        return Confirm(options, cancellationToken);
    }

    /// <summary>
    /// Asks for a yes-or-no answer.
    /// </summary>
    /// <param name="message">The message shown before the input.</param>
    /// <param name="defaultValue">The answer accepted by Enter; null requires an explicit answer.</param>
    /// <returns>True for yes; false for no.</returns>
    /// <exception cref="InvalidOperationException">The default driver cannot use redirected input or output, or the driver factory returns null.</exception>
    /// <exception cref="PromptCanceledException">The user presses Ctrl+C or Escape and ThrowExceptionOnCancel is true.</exception>
    public static bool Confirm(string message, bool? defaultValue = default)
    {
        return Confirm(options =>
        {
            options.Message = message;
            options.DefaultValue = defaultValue;
        });
    }

    /// <summary>
    /// Lets the user select one item and returns the original item.
    /// </summary>
    /// <typeparam name="T">The input or item type.</typeparam>
    /// <param name="options">The prompt settings, including the required message.</param>
    /// <returns>The selected item.</returns>
    /// <exception cref="InvalidOperationException">The default driver cannot use redirected input or output, or the driver factory returns null.</exception>
    /// <exception cref="PromptCanceledException">The user presses Ctrl+C or Escape and ThrowExceptionOnCancel is true.</exception>
    public static T Select<T>(SelectOptions<T> options) where T : notnull
    {
        return Select(options, CancellationToken.None);
    }

    /// <summary>
    /// Lets the user select one item and returns the original item.
    /// </summary>
    /// <typeparam name="T">The input or item type.</typeparam>
    /// <param name="options">The prompt settings, including the required message.</param>
    /// <param name="cancellationToken">Cancels the prompt while waiting for input, independently of ThrowExceptionOnCancel.</param>
    /// <returns>The selected item.</returns>
    /// <exception cref="InvalidOperationException">The default driver cannot use redirected input or output, or the driver factory returns null.</exception>
    /// <exception cref="PromptCanceledException">The user presses Ctrl+C or Escape and ThrowExceptionOnCancel is true.</exception>
    /// <exception cref="OperationCanceledException">The cancellation token is canceled.</exception>
    public static T Select<T>(SelectOptions<T> options, CancellationToken cancellationToken) where T : notnull
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var form = new SelectForm<T>(options, s_configuration);

        return form.Start(cancellationToken);
    }

    /// <summary>
    /// Lets the user select one item and returns the original item.
    /// </summary>
    /// <typeparam name="T">The input or item type.</typeparam>
    /// <param name="configure">Configures a new options instance before the prompt starts.</param>
    /// <returns>The selected item.</returns>
    /// <exception cref="InvalidOperationException">The default driver cannot use redirected input or output, or the driver factory returns null.</exception>
    /// <exception cref="PromptCanceledException">The user presses Ctrl+C or Escape and ThrowExceptionOnCancel is true.</exception>
    public static T Select<T>(Action<SelectOptions<T>> configure) where T : notnull
    {
        return Select(configure, CancellationToken.None);
    }

    /// <summary>
    /// Lets the user select one item and returns the original item.
    /// </summary>
    /// <typeparam name="T">The input or item type.</typeparam>
    /// <param name="configure">Configures a new options instance before the prompt starts.</param>
    /// <param name="cancellationToken">Cancels the prompt while waiting for input, independently of ThrowExceptionOnCancel.</param>
    /// <returns>The selected item.</returns>
    /// <exception cref="InvalidOperationException">The default driver cannot use redirected input or output, or the driver factory returns null.</exception>
    /// <exception cref="PromptCanceledException">The user presses Ctrl+C or Escape and ThrowExceptionOnCancel is true.</exception>
    /// <exception cref="OperationCanceledException">The cancellation token is canceled.</exception>
    public static T Select<T>(Action<SelectOptions<T>> configure, CancellationToken cancellationToken) where T : notnull
    {
        var options = new SelectOptions<T>();

        configure(options);

        return Select(options, cancellationToken);
    }

    /// <summary>
    /// Lets the user select one item and returns the original item.
    /// </summary>
    /// <typeparam name="T">The input or item type.</typeparam>
    /// <param name="message">The message shown before the input.</param>
    /// <param name="defaultValue">The default value; null means no default. For Input and Select it must be compatible with T; compatible primitive conversions and integral enum values are accepted.</param>
    /// <param name="items">The available items. May be omitted for enum types; otherwise provide an item sequence.</param>
    /// <param name="pageSize">The requested items per page, limited by terminal height.</param>
    /// <param name="textSelector">Formats items for display and filtering without changing the returned items.</param>
    /// <returns>The selected item.</returns>
    /// <exception cref="InvalidOperationException">The default driver cannot use redirected input or output, or the driver factory returns null.</exception>
    /// <exception cref="PromptCanceledException">The user presses Ctrl+C or Escape and ThrowExceptionOnCancel is true.</exception>
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

    /// <summary>
    /// Lets the user select multiple items and returns the original items.
    /// </summary>
    /// <typeparam name="T">The input or item type.</typeparam>
    /// <param name="options">The prompt settings, including the required message.</param>
    /// <returns>The selected items.</returns>
    /// <exception cref="InvalidOperationException">The default driver cannot use redirected input or output, or the driver factory returns null.</exception>
    /// <exception cref="PromptCanceledException">The user presses Ctrl+C or Escape and ThrowExceptionOnCancel is true.</exception>
    public static IEnumerable<T> MultiSelect<T>(MultiSelectOptions<T> options) where T : notnull
    {
        return MultiSelect(options, CancellationToken.None);
    }

    /// <summary>
    /// Lets the user select multiple items and returns the original items.
    /// </summary>
    /// <typeparam name="T">The input or item type.</typeparam>
    /// <param name="options">The prompt settings, including the required message.</param>
    /// <param name="cancellationToken">Cancels the prompt while waiting for input, independently of ThrowExceptionOnCancel.</param>
    /// <returns>The selected items.</returns>
    /// <exception cref="InvalidOperationException">The default driver cannot use redirected input or output, or the driver factory returns null.</exception>
    /// <exception cref="PromptCanceledException">The user presses Ctrl+C or Escape and ThrowExceptionOnCancel is true.</exception>
    /// <exception cref="OperationCanceledException">The cancellation token is canceled.</exception>
    public static IEnumerable<T> MultiSelect<T>(MultiSelectOptions<T> options, CancellationToken cancellationToken) where T : notnull
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var form = new MultiSelectForm<T>(options, s_configuration);

        return form.Start(cancellationToken);
    }

    /// <summary>
    /// Lets the user select multiple items and returns the original items.
    /// </summary>
    /// <typeparam name="T">The input or item type.</typeparam>
    /// <param name="configure">Configures a new options instance before the prompt starts.</param>
    /// <returns>The selected items.</returns>
    /// <exception cref="InvalidOperationException">The default driver cannot use redirected input or output, or the driver factory returns null.</exception>
    /// <exception cref="PromptCanceledException">The user presses Ctrl+C or Escape and ThrowExceptionOnCancel is true.</exception>
    public static IEnumerable<T> MultiSelect<T>(Action<MultiSelectOptions<T>> configure) where T : notnull
    {
        return MultiSelect(configure, CancellationToken.None);
    }

    /// <summary>
    /// Lets the user select multiple items and returns the original items.
    /// </summary>
    /// <typeparam name="T">The input or item type.</typeparam>
    /// <param name="configure">Configures a new options instance before the prompt starts.</param>
    /// <param name="cancellationToken">Cancels the prompt while waiting for input, independently of ThrowExceptionOnCancel.</param>
    /// <returns>The selected items.</returns>
    /// <exception cref="InvalidOperationException">The default driver cannot use redirected input or output, or the driver factory returns null.</exception>
    /// <exception cref="PromptCanceledException">The user presses Ctrl+C or Escape and ThrowExceptionOnCancel is true.</exception>
    /// <exception cref="OperationCanceledException">The cancellation token is canceled.</exception>
    public static IEnumerable<T> MultiSelect<T>(Action<MultiSelectOptions<T>> configure, CancellationToken cancellationToken) where T : notnull
    {
        var options = new MultiSelectOptions<T>();

        configure(options);

        return MultiSelect(options, cancellationToken);
    }

    /// <summary>
    /// Lets the user select multiple items and returns the original items.
    /// </summary>
    /// <typeparam name="T">The input or item type.</typeparam>
    /// <param name="message">The message shown before the input.</param>
    /// <param name="items">The available items. May be omitted for enum types; otherwise provide an item sequence.</param>
    /// <param name="pageSize">The requested items per page, limited by terminal height.</param>
    /// <param name="textSelector">Formats items for display and filtering without changing the returned items.</param>
    /// <param name="minimum">The minimum item count. Defaults to 1; use 0 to allow an empty result.</param>
    /// <param name="maximum">The maximum item count. Must be at least minimum.</param>
    /// <param name="defaultValues">The initially selected items; null means no initial selection.</param>
    /// <returns>The selected items.</returns>
    /// <exception cref="InvalidOperationException">The default driver cannot use redirected input or output, or the driver factory returns null.</exception>
    /// <exception cref="PromptCanceledException">The user presses Ctrl+C or Escape and ThrowExceptionOnCancel is true.</exception>
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

    /// <summary>
    /// Reads and converts items using an invariant-culture TypeConverter until an empty line is submitted.
    /// </summary>
    /// <typeparam name="T">The input or item type.</typeparam>
    /// <param name="options">The prompt settings, including the required message.</param>
    /// <returns>The entered items.</returns>
    /// <exception cref="InvalidOperationException">The default driver cannot use redirected input or output, or the driver factory returns null.</exception>
    /// <exception cref="PromptCanceledException">The user presses Ctrl+C or Escape and ThrowExceptionOnCancel is true.</exception>
    public static IEnumerable<T> List<T>(ListOptions<T> options) where T : notnull
    {
        return List(options, CancellationToken.None);
    }

    /// <summary>
    /// Reads and converts items using an invariant-culture TypeConverter until an empty line is submitted.
    /// </summary>
    /// <typeparam name="T">The input or item type.</typeparam>
    /// <param name="options">The prompt settings, including the required message.</param>
    /// <param name="cancellationToken">Cancels the prompt while waiting for input, independently of ThrowExceptionOnCancel.</param>
    /// <returns>The entered items.</returns>
    /// <exception cref="InvalidOperationException">The default driver cannot use redirected input or output, or the driver factory returns null.</exception>
    /// <exception cref="PromptCanceledException">The user presses Ctrl+C or Escape and ThrowExceptionOnCancel is true.</exception>
    /// <exception cref="OperationCanceledException">The cancellation token is canceled.</exception>
    public static IEnumerable<T> List<T>(ListOptions<T> options, CancellationToken cancellationToken) where T : notnull
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var form = new ListForm<T>(options, s_configuration);

        return form.Start(cancellationToken);
    }

    /// <summary>
    /// Reads and converts items using an invariant-culture TypeConverter until an empty line is submitted.
    /// </summary>
    /// <typeparam name="T">The input or item type.</typeparam>
    /// <param name="configure">Configures a new options instance before the prompt starts.</param>
    /// <returns>The entered items.</returns>
    /// <exception cref="InvalidOperationException">The default driver cannot use redirected input or output, or the driver factory returns null.</exception>
    /// <exception cref="PromptCanceledException">The user presses Ctrl+C or Escape and ThrowExceptionOnCancel is true.</exception>
    public static IEnumerable<T> List<T>(Action<ListOptions<T>> configure) where T : notnull
    {
        return List(configure, CancellationToken.None);
    }

    /// <summary>
    /// Reads and converts items using an invariant-culture TypeConverter until an empty line is submitted.
    /// </summary>
    /// <typeparam name="T">The input or item type.</typeparam>
    /// <param name="configure">Configures a new options instance before the prompt starts.</param>
    /// <param name="cancellationToken">Cancels the prompt while waiting for input, independently of ThrowExceptionOnCancel.</param>
    /// <returns>The entered items.</returns>
    /// <exception cref="InvalidOperationException">The default driver cannot use redirected input or output, or the driver factory returns null.</exception>
    /// <exception cref="PromptCanceledException">The user presses Ctrl+C or Escape and ThrowExceptionOnCancel is true.</exception>
    /// <exception cref="OperationCanceledException">The cancellation token is canceled.</exception>
    public static IEnumerable<T> List<T>(Action<ListOptions<T>> configure, CancellationToken cancellationToken) where T : notnull
    {
        var options = new ListOptions<T>();

        configure(options);

        return List(options, cancellationToken);
    }

    /// <summary>
    /// Reads and converts items using an invariant-culture TypeConverter until an empty line is submitted.
    /// </summary>
    /// <typeparam name="T">The input or item type.</typeparam>
    /// <param name="message">The message shown before the input.</param>
    /// <param name="validators">Validators applied to the converted value or password; null means no additional validators.</param>
    /// <param name="minimum">The minimum item count. Defaults to 1; use 0 to allow an empty result.</param>
    /// <param name="maximum">The maximum item count. Must be at least minimum.</param>
    /// <returns>The entered items.</returns>
    /// <exception cref="InvalidOperationException">The default driver cannot use redirected input or output, or the driver factory returns null.</exception>
    /// <exception cref="PromptCanceledException">The user presses Ctrl+C or Escape and ThrowExceptionOnCancel is true.</exception>
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
