namespace Sharprompt.Fluent;

/// <summary>
/// Fluent configuration methods for ConfirmOptions.
/// </summary>
public static class ConfirmOptionsExtensions
{
    /// <summary>
    /// Sets Message and returns the same options instance.
    /// </summary>
    /// <param name="options">The options instance to update.</param>
    /// <param name="message">The prompt message.</param>
    /// <returns>The same options instance for chaining.</returns>
    public static ConfirmOptions WithMessage(this ConfirmOptions options, string message)
    {
        options.Message = message;

        return options;
    }

    /// <summary>
    /// Sets DefaultValue and returns the same options instance.
    /// </summary>
    /// <param name="options">The options instance to update.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>The same options instance for chaining.</returns>
    public static ConfirmOptions WithDefaultValue(this ConfirmOptions options, bool defaultValue)
    {
        options.DefaultValue = defaultValue;

        return options;
    }
}
