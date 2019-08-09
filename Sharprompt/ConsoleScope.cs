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
            _renderer.Close();

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
            var left = Console.CursorLeft;

            var line = Console.ReadLine();

            if (line != null)
            {
                Console.SetCursorPosition(left, Console.CursorTop - 1);
            }

            return line;
        }

        public void SetError(ValidationError error)
        {
            _errorMessage = error.Message;
        }

        public bool Validate(object input, IList<Func<object, ValidationError>> validators)
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

        public void Render<TModel>(Action<ConsoleRenderer, TModel> template, TModel model)
        {
            Console.CursorVisible = false;

            _renderer.Reset();

            template(_renderer, model);

            if (_errorMessage != null)
            {
                _renderer.WriteErrorMessage(_errorMessage);

                _errorMessage = null;
            }

            Console.CursorVisible = _cursorVisible;
        }
    }
}