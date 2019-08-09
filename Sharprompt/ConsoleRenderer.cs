using System;

namespace Sharprompt
{
    internal class ConsoleRenderer
    {
        public ConsoleRenderer()
        {
        }

        private int _lineCount;
        private int _errorLineCount;

        public void Close()
        {
            Console.ResetColor();

            if (Console.CursorLeft != 0)
            {
                Console.WriteLine();
            }
        }

        public void Reset()
        {
            var space = new string(' ', Console.WindowWidth - 1);

            var bottom = Console.CursorTop + _errorLineCount;

            for (int i = 0; i <= _lineCount + _errorLineCount; i++)
            {
                Console.SetCursorPosition(0, bottom - i);

                Console.Write(space);
            }

            Console.SetCursorPosition(0, Console.CursorTop);

            _lineCount = 0;
            _errorLineCount = 0;
        }

        public void Write(string value)
        {
            Console.Write(value);
        }

        public void Write(string value, ConsoleColor color)
        {
            var previousColor = Console.ForegroundColor;

            Console.ForegroundColor = color;

            Console.Write(value);

            Console.ForegroundColor = previousColor;
        }

        public void WriteLine()
        {
            Console.WriteLine();

            _lineCount += 1;
        }

        public void WriteMessage(string message)
        {
            Write("?", ConsoleColor.Green);
            Write($" {message}: ");
        }

        public void WriteErrorMessage(string errorMessage)
        {
            var left = Console.CursorLeft;

            Console.WriteLine();
            Write($">> {errorMessage}", ConsoleColor.Red);

            Console.SetCursorPosition(left, Console.CursorTop - 1);

            _errorLineCount = 1;
        }
    }
}