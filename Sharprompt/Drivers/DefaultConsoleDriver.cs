using System;

namespace Sharprompt.Drivers
{
    internal class DefaultConsoleDriver : IConsoleDriver
    {
        private int _lineCount;
        private int _errorLineCount;

        #region IDisposable

        public virtual void Dispose()
        {
            Console.ResetColor();

            if (Console.CursorLeft != 0)
            {
                Console.WriteLine();
            }
        }

        #endregion

        #region IConsoleDriver

        public void Beep() => Console.Beep();

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

        public ConsoleKeyInfo ReadKey() => Console.ReadKey(true);

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

        public virtual int Write(string value)
        {
            var writtenLines = (Console.CursorLeft + value.Length) / Console.BufferWidth;

            _lineCount += writtenLines;

            Console.Write(value);

            return writtenLines;
        }

        public virtual int Write(string value, ConsoleColor color)
        {
            var previousColor = Console.ForegroundColor;

            Console.ForegroundColor = color;

            var writtenLines = Write(value);

            Console.ForegroundColor = previousColor;

            return writtenLines;
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

        public bool CursorVisible
        {
            get => Console.CursorVisible;
            set => Console.CursorVisible = value;
        }

        #endregion

        protected virtual void EraseLine(int y)
        {
            Console.SetCursorPosition(0, y);

            Console.Write("\x1b[2K");
        }
    }
}
