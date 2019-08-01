using System;
using System.Collections.Generic;

namespace Sharprompt
{
    internal class ConsoleRenderer
    {
        public ConsoleRenderer()
        {
            _previousBottom = Console.CursorTop;
        }

        private readonly Stack<(int left, int top)> _positions = new Stack<(int left, int top)>();

        private int _lineCount;
        private int _errorLineCount;
        private int _previousBottom;

        public void PushCursor()
        {
            _positions.Push((Console.CursorLeft, Console.CursorTop));
        }

        public void PopCursor()
        {
            var (left, top) = _positions.Pop();

            Console.SetCursorPosition(left, top);
        }

        public void BeginRender()
        {
            Console.CursorVisible = false;

            var space = new string(' ', Console.WindowWidth - 1);

            for (int i = 0; i <= _lineCount + _errorLineCount; i++)
            {
                Console.SetCursorPosition(0, _previousBottom - i);

                Console.Write(space);
            }

            Console.SetCursorPosition(0, _previousBottom - _lineCount - _errorLineCount);

            _lineCount = 0;
            _errorLineCount = 0;
        }

        public void EndRender()
        {
            _previousBottom = Console.CursorTop + _lineCount + _errorLineCount;

            Console.CursorVisible = true;
        }

        public void CheckBufferBoundary()
        {
            if (_previousBottom == Console.BufferHeight - 1)
            {
                _lineCount += 1;
            }
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

            Console.SetCursorPosition(0, Console.CursorTop + _lineCount);

            Console.WriteLine();
            Write($">> {errorMessage}", ConsoleColor.Red);

            _errorLineCount += 1;

            PopCursor();
        }
    }
}