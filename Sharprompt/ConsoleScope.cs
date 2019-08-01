using System;
using System.Collections.Generic;

namespace Sharprompt
{
    internal class ConsoleScope : IDisposable
    {
        public ConsoleScope()
        {
        }

        private readonly ConsoleRenderer _renderer = new ConsoleRenderer();

        private string _errorMessage;

        public void Dispose()
        {
            if (Console.CursorLeft != 0)
            {
                Console.WriteLine();
            }

            Console.ResetColor();
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
            _renderer.CheckBufferBoundary();

            return Console.ReadLine();
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
            _renderer.BeginRender();

            template(_renderer);

            if (_errorMessage != null)
            {
                _renderer.WriteErrorMessage(_errorMessage);

                _errorMessage = null;
            }

            _renderer.EndRender();
        }
    }
}