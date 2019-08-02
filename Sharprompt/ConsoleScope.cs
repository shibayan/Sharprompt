using System;
using System.Collections.Generic;

namespace Sharprompt
{
    internal class ConsoleScope : IDisposable
    {
        public ConsoleScope(bool cursorVisible = true)
        {
            _cursorVisible = cursorVisible;
        }

        private readonly bool _cursorVisible;
        private readonly ConsoleRenderer _renderer = new ConsoleRenderer();

        private string _errorMessage;

        public void Dispose()
        {
            if (Console.CursorLeft != 0)
            {
                Console.WriteLine();
            }

            Console.ResetColor();
            Console.CursorVisible = true;
        }

        public void Beep()
        {
            Console.Beep();
        }

        public ConsoleKeyInfo ReadKey()
        {
            return Console.ReadKey(true);
        }

        public string ReadLine()
        {
            var line = Console.ReadLine();

            if (line != null)
            {
                Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop - 1);
            }

            return line;
        }

        public void SetError(Error error)
        {
            _errorMessage = error.Message;
        }

        public bool Validate(object input, IList<Func<object, Error>> validators)
        {
            if (validators == null)
            {
                return true;
            }

            foreach (var validator in validators)
            {
                var error = validator(input);

                if (error != null)
                {
                    _errorMessage = error.Message;

                    return false;
                }
            }

            return true;
        }

        public void SetException(Exception exception)
        {
            _errorMessage = exception.Message;
        }

        public void Render(Action<ConsoleRenderer> template)
        {
            Console.CursorVisible = false;

            _renderer.Reset();

            template(_renderer);

            if (_errorMessage != null)
            {
                _renderer.WriteErrorMessage(_errorMessage);

                _errorMessage = null;
            }

            Console.CursorVisible = _cursorVisible;
        }
    }
}