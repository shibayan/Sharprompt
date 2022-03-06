namespace Sharprompt.Fluent;

public static class ConfirmOptionsExtensions
{
    public static ConfirmOptions WithMessage(this ConfirmOptions options, string message)
    {
        options.Message = message;

        return options;
    }

    public static ConfirmOptions WithDefaultValue(this ConfirmOptions options, bool defaultValue)
    {
        options.DefaultValue = defaultValue;

        return options;
    }
}
