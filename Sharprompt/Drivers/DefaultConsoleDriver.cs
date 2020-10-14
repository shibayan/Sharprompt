using System;
using System.Text;

using Sharprompt.Internal;

namespace Sharprompt.Drivers
{
    internal class DefaultConsoleDriver : IConsoleDriver
    {
        public DefaultConsoleDriver()
        {
            Console.CancelKeyPress += RequestCancellation;
        }

        #region IDisposable

        public virtual void Dispose()
        {
            Reset();

            Console.CancelKeyPress -= RequestCancellation;
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
                WriteLine();
            }
        }

        public virtual void ClearLine(int top)
        {
            SetCursorPosition(0, top);

            Write("\x1b[2K");

            SetCursorPosition(0, top);
        }

        public virtual ConsoleKeyInfo ReadKey() => Console.ReadKey(true);

        public virtual string ReadLine()
        {
            int startIndex = 0;
            var inputBuffer = new StringBuilder();

            while (true)
            {
                var keyInfo = ReadKey();

                if (keyInfo.Key == ConsoleKey.Enter)
                {
                    break;
                }

                if (keyInfo.Key == ConsoleKey.LeftArrow)
                {
                    if (startIndex > 0)
                    {
                        startIndex -= 1;

                        var width = EastAsianWidth.GetWidth(inputBuffer[startIndex]);

                        if (Console.CursorLeft - width < 0)
                        {
                            Console.CursorTop -= 1;
                            Console.CursorLeft = Console.BufferWidth - width;
                        }
                        else
                        {
                            Console.CursorLeft -= width;
                        }
                    }
                    else
                    {
                        Beep();
                    }
                }
                else if (keyInfo.Key == ConsoleKey.RightArrow)
                {
                    if (startIndex < inputBuffer.Length)
                    {
                        var width = EastAsianWidth.GetWidth(inputBuffer[startIndex]);

                        if (Console.CursorLeft + width >= Console.BufferWidth)
                        {
                            Console.CursorTop += 1;
                            Console.CursorLeft = 0;
                        }
                        else
                        {
                            Console.CursorLeft += width;
                        }

                        startIndex += 1;
                    }
                    else
                    {
                        Beep();
                    }
                }
                else if (keyInfo.Key == ConsoleKey.Backspace)
                {
                    if (startIndex > 0)
                    {
                        startIndex -= 1;

                        var width = EastAsianWidth.GetWidth(inputBuffer[startIndex]);

                        if (Console.CursorLeft - width < 0)
                        {
                            Console.CursorTop -= 1;
                            Console.CursorLeft = Console.BufferWidth - 1;
                        }
                        else
                        {
                            Console.CursorLeft -= width;
                        }

                        inputBuffer.Remove(startIndex, 1);

                        var (left, top) = GetCursorPosition();

                        for (int i = startIndex; i < inputBuffer.Length; i++)
                        {
                            Console.Write(inputBuffer[i]);
                        }

                        Console.Write(width == 1 ? " " : "  ");

                        SetCursorPosition(left, top);
                    }
                    else
                    {
                        Beep();
                    }
                }
                else if (keyInfo.Key == ConsoleKey.Delete)
                {
                    if (startIndex < inputBuffer.Length)
                    {
                        var width = EastAsianWidth.GetWidth(inputBuffer[startIndex]);

                        inputBuffer.Remove(startIndex, 1);

                        var (left, top) = GetCursorPosition();

                        for (int i = startIndex; i < inputBuffer.Length; i++)
                        {
                            Console.Write(inputBuffer[i]);
                        }

                        Console.Write(width == 1 ? " " : "  ");

                        SetCursorPosition(left, top);
                    }
                    else
                    {
                        Beep();
                    }
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    inputBuffer.Insert(startIndex, keyInfo.KeyChar);

                    var (left, top) = GetCursorPosition();

                    for (int i = startIndex; i < inputBuffer.Length; i++)
                    {
                        Console.Write(inputBuffer[i]);
                    }

                    left += EastAsianWidth.GetWidth(keyInfo.KeyChar);

                    if (left >= Console.BufferWidth)
                    {
                        left = 0;
                        top += 1;

                        if (top >= Console.BufferHeight)
                        {
                            Console.BufferHeight += 1;
                        }
                    }

                    SetCursorPosition(left, top);

                    startIndex += 1;
                }
            }

            return inputBuffer.ToString();
        }

        public virtual int Write(string value)
        {
            var writtenLines = (CursorLeft + EastAsianWidth.GetWidth(value)) / Console.BufferWidth;

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

        public (int left, int top) GetCursorPosition() => (Console.CursorLeft, Console.CursorTop);

        public virtual void SetCursorPosition(int left, int top)
        {
            if (top >= Console.BufferHeight)
            {
                Console.BufferHeight += 1;
            }

            Console.SetCursorPosition(left, top);
        }

        public virtual bool CursorVisible
        {
            get => Console.CursorVisible;
            set => Console.CursorVisible = value;
        }

        public virtual int CursorLeft => Console.CursorLeft;

        public virtual int CursorTop => Console.CursorTop;

        public virtual int BufferWidth => Console.BufferWidth;

        public virtual int BufferHeight => Console.BufferHeight;

        #endregion

        private void RequestCancellation(object sender, ConsoleCancelEventArgs e)
        {
            Reset();
        }
    }
}
