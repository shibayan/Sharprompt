namespace Sharprompt
{
    public class ConfirmOptions
    {
        public bool StartWithDefaultValue { get; set; } = true;

        public string Message { get; set; }

        public bool? DefaultValue { get; set; }
    }
}
