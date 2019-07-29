namespace Sharprompt
{
    internal class Confirm
    {
        public Confirm(string message)
        {
            _message = message;
        }

        public Confirm(string message, bool defaultValue)
        {
            _message = message;
            _defaultValue = defaultValue;
        }

        private readonly string _message;
        private readonly bool? _defaultValue;

        public bool Start()
        {
            using (var scope = new ConsoleScope(true))
            {
                scope.Render(Template);

                var input = scope.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                {
                    if (_defaultValue != null)
                    {
                        return _defaultValue.Value;
                    }
                }

                return input.ToLower() == "y" || input.ToLower() == "yes";
            }
        }

        private void Template(ConsoleRenderer renderer)
        {
            renderer.WriteMessage(_message);

            if (_defaultValue != null)
            {
                renderer.Write($"({(_defaultValue.Value ? "yes" : "no")}) ");
            }
        }
    }
}
