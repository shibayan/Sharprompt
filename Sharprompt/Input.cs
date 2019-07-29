using System;

namespace Sharprompt
{
    internal class Input<T>
    {
        public Input(string message)
        {
            _message = message;
        }

        public Input(string message, T defaultValue)
        {
            _message = message;
            _defaultValue = defaultValue;
        }

        private readonly string _message;
        private readonly T _defaultValue;

        public T Start()
        {
            using (var scope = new ConsoleScope(true))
            {
                scope.Render(Template);

                var input = scope.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                {
                    if (_defaultValue != null)
                    {
                        return _defaultValue;
                    }
                }

                return (T)Convert.ChangeType(input, typeof(T));
            }
        }

        private void Template(ConsoleRenderer renderer)
        {
            renderer.WriteMessage(_message);

            if (_defaultValue != null)
            {
                renderer.Write($"({_defaultValue}) ");
            }
        }
    }
}
