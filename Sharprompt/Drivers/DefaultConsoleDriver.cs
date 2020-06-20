using System;
using System.Text;

namespace Sharprompt.Drivers
{
    internal class DefaultConsoleDriver : IConsoleDriver
    {
        #region IDisposable

        public virtual void Dispose()
        {
            Reset();
        }

        #endregion

        #region IConsoleDriver

        public virtual void Beep() => Console.Beep();

        public void Reset()
        {
            Console.CursorVisible = true;
            Console.ResetColor();

            if (CursorLeft != 0)
            {
                Console.WriteLine();
            }
        }

        public virtual void ClearLine(int top)
        {
            Console.SetCursorPosition(0, top);

            Console.Write("\x1b[2K");
        }

        public virtual ConsoleKeyInfo ReadKey() => Console.ReadKey(true);

        public virtual string ReadLine()
        {
            int cursor = 0;
            var buffer = new StringBuilder();

            while (true)
            {
                var keyInfo = ReadKey();

                if (keyInfo.Key == ConsoleKey.Enter)
                {
                    break;
                }

                if (keyInfo.Key == ConsoleKey.LeftArrow)
                {
                    if (cursor == 0)
                    {
                        Console.Beep();
                    }
                    else
                    {
                        Console.CursorLeft -= 1;
                        cursor -= 1;
                    }
                }
                else if (keyInfo.Key == ConsoleKey.RightArrow)
                {
                    if (cursor == buffer.Length)
                    {
                        Console.Beep();
                    }
                    else
                    {
                        Console.CursorLeft += 1;
                        cursor += 1;
                    }
                }
                else if (keyInfo.Key == ConsoleKey.Backspace)
                {
                    if (buffer.Length > 0)
                    {
                        Console.CursorLeft -= 1;
                        Console.Write(" ");

                        Console.CursorLeft -= 1;
                        cursor -= 1;

                        buffer.Length -= 1;
                    }
                    else
                    {
                        Beep();
                    }
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    Console.Write(keyInfo.KeyChar);

                    buffer.Insert(cursor, keyInfo.KeyChar);

                    cursor += 1;
                }
            }

            return buffer.ToString();
        }

        public virtual int Write(string value)
        {
            var writtenLines = (CursorLeft + value.Length) / Console.BufferWidth;

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
