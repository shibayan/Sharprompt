using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Sharprompt.Internal
{
    internal class ConsoleScope : IDisposable
    {
        public ConsoleScope(bool cursorVisible = true)
        {
            _cursorVisible = cursorVisible;

            _renderer = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? new WindowsConsoleRenderer() : new DefaultConsoleRenderer();
        }

        private readonly bool _cursorVisible;
        private readonly IConsoleRenderer _renderer;

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


        public void Render(Action<IConsoleRenderer> template)
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

        public void Render<TModel>(Action<IConsoleRenderer, TModel> template, TModel model)
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
