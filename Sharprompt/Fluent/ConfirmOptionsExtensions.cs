namespace Sharprompt.Fluent
{
    public static class ConfirmOptionsExtensions
    {
        public static ConfirmOptions SetMessage(this ConfirmOptions options, string message)
        {
            options.Message = message;

            return options;
        }

        public static ConfirmOptions SetDefaultValue(this ConfirmOptions options, bool? defaultValue)
        {
            options.DefaultValue = defaultValue;

            return options;
        }
    }
}
