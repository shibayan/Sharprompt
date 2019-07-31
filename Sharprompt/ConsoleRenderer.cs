using System;
using System.Collections.Generic;

namespace Sharprompt
{
    internal class ConsoleRenderer
    {
        public ConsoleRenderer(int initialTop, int previousBottom)
        {
            _initialTop = initialTop;
            _previousBottom = previousBottom;
        }

        private readonly int _initialTop;
        private readonly int _previousBottom;
        private readonly Stack<(int left, int top)> _positions = new Stack<(int left, int top)>();

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

            for (int top = _previousBottom; top >= _initialTop; top--)
            {
                Console.SetCursorPosition(0, top);

                Console.Write(space);
            }

            Console.SetCursorPosition(0, _initialTop);
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