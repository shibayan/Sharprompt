using System;

namespace Sharprompt.Drivers
{
    internal class DefaultConsoleDriver : IConsoleDriver
    {
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

        public virtual void Beep() => Console.Beep();

        public virtual void ClearLine(int top)
        {
            Console.SetCursorPosition(0, top);

            Console.Write("\x1b[2K");
        }

        public virtual ConsoleKeyInfo ReadKey() => Console.ReadKey(true);

        public virtual string ReadLine()
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

        public virtual int WriteLine()
        {
            Console.WriteLine();

            return 1;
        }

        public virtual void SetCursorPosition(int left, int top)
        {
            Console.SetCursorPosition(left, top);
        }

        public virtual bool CursorVisible
        {
            get => Console.CursorVisible;
            set => Console.CursorVisible = value;
        }

        public virtual int CursorLeft => Console.CursorLeft;

        public virtual int CursorTop => Console.CursorTop;

        #endregion
    }
}
