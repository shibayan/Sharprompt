using System;
using System.Collections.Generic;

namespace Sharprompt
{
    internal class ConsoleScope : IDisposable
    {
        public ConsoleScope(bool cursorVisible)
        {
            _cursorVisible = cursorVisible;
        }

        private readonly bool _cursorVisible;

        private string _errorMessage;
        private int _rowCount = 0;
        private int _previousBottom = Console.CursorTop;

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
            if (Console.CursorTop == Console.BufferHeight - 1)
            {
                _rowCount += 1;
            }
            else
            {
                _rowCount += 0;
            }

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
            Console.CursorVisible = false;

            var renderer = new ConsoleRenderer(_rowCount, _previousBottom);

            renderer.Clear();

            template(renderer);

            _previousBottom = Console.CursorTop;

            if (_errorMessage != null)
            {
                renderer.WriteErrorMessage(_errorMessage);

                _errorMessage = null;

                _previousBottom += 1;
            }

            _rowCount = renderer.WrittenRowCount;

            if (_cursorVisible)
            {
                Console.CursorVisible = true;
            }
        }
    }
}