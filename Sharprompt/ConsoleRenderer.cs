using System;
using System.Collections.Generic;

namespace Sharprompt
{
    internal class ConsoleRenderer
    {
        public ConsoleRenderer(int lineCount, int previousBottom)
        {
            _lineCount = lineCount;
            _previousBottom = previousBottom;
        }

        private int _lineCount;
        private readonly int _previousBottom;
        private readonly Stack<(int left, int top)> _positions = new Stack<(int left, int top)>();

        public int WrittenRowCount => _lineCount;

        public void PushCursor()
        {
            _positions.Push((Console.CursorLeft, Console.CursorTop));
        }

        public void PopCursor()
        {
            var (left, top) = _positions.Pop();

            Console.SetCursorPosition(left, top);
        }

        public void Clear()
        {
            var space = new string(' ', Console.WindowWidth - 1);

            for (int i = 0; i <= _lineCount; i++)
            {
                Console.SetCursorPosition(0, _previousBottom - i);

                Console.Write(space);
            }

            Console.SetCursorPosition(0, _previousBottom - _lineCount);

            _lineCount = 0;
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
            PushCursor();

            WriteLine();
            Write($">> {errorMessage}", ConsoleColor.Red);

            PopCursor();
        }
    }
}