using System;

namespace Sharprompt
{
    internal class ConsoleScope : IDisposable
    {
        public ConsoleScope(bool cursorVisible)
        {
            _cursorVisible = cursorVisible;
        }

        private readonly int _initialTop = Console.CursorTop;
        private readonly bool _cursorVisible;

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
            return Console.ReadLine();
        }

        public void SetError(string errorMessage)
        {
            _errorMessage = errorMessage;
        }

        public void SetError(Exception exception)
        {
            _errorMessage = exception.Message;
        }

        public void Render(Action<ConsoleRenderer> template)
        {
            Console.CursorVisible = false;

            var renderer = new ConsoleRenderer(_initialTop);

            renderer.Clear();

            template(renderer);

            if (_errorMessage != null)
            {
                renderer.WriteErrorMessage(_errorMessage);

                _errorMessage = null;
            }

            if (_cursorVisible)
            {
                Console.CursorVisible = true;
            }
        }
    }
}