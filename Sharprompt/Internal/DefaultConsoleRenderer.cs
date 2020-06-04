using System;

namespace Sharprompt.Internal
{
    internal class DefaultConsoleRenderer
    {
        private int _lineCount;
        private int _errorLineCount;

        public virtual void Close()
        {
            Console.ResetColor();

            if (Console.CursorLeft != 0)
            {
                Console.WriteLine();
            }
        }

        public virtual void Reset()
        {
            var bottom = Console.CursorTop + _errorLineCount;

            for (int i = 0; i <= _lineCount + _errorLineCount; i++)
            {
                EraseLine(bottom - i);
            }

            Console.SetCursorPosition(0, Console.CursorTop);

            _lineCount = 0;
            _errorLineCount = 0;
        }

        public virtual void Write(string value)
        {
            _lineCount += (Console.CursorLeft + value.Length) / Console.BufferWidth;

            Console.Write(value);
        }

        public virtual void Write(string value, ConsoleColor color)
        {
            var previousColor = Console.ForegroundColor;

            Console.ForegroundColor = color;

            Write(value);

            Console.ForegroundColor = previousColor;
        }

        public virtual void WriteLine()
        {
            Console.WriteLine();

            _lineCount += 1;
        }

        public virtual void WriteErrorMessage(string errorMessage)
        {
            var left = Console.CursorLeft;

            Console.WriteLine();
            Write($">> {errorMessage}", ConsoleColor.Red);

            Console.SetCursorPosition(left, Console.CursorTop - 1);

            _errorLineCount = 1;
        }

        public virtual void EraseLine(int y)
        {
            Console.SetCursorPosition(0, y);

            Console.Write("\x1b[2K");
        }
    }
}
