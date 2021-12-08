namespace Sharprompt
{
    public class ConfirmOptions
    {
        public string Message { get; set; }

        public bool? DefaultValue { get; set; }

        public ConfirmOptions SetMessage(string message)
        {
            Message = message;

            return this;
        }

        public ConfirmOptions SetDefaultValue(bool? defaultValue)
        {
            DefaultValue = defaultValue;

            return this;
        }
    }
}
