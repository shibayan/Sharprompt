using System;
using System.Runtime.InteropServices;
using System.Text;

using Sharprompt.Internal;

namespace Sharprompt.Drivers
{
    internal class DefaultConsoleDriver : IConsoleDriver
    {
        static DefaultConsoleDriver()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var hConsole = NativeMethods.GetStdHandle(NativeMethods.STD_OUTPUT_HANDLE);

                if (!NativeMethods.GetConsoleMode(hConsole, out var mode))
                {
                    return;
                }

                NativeMethods.SetConsoleMode(hConsole, mode | NativeMethods.ENABLE_VIRTUAL_TERMINAL_PROCESSING);
            }
        }

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

        public virtual void Write(string value)
        {
            Console.Write(value);
        }

        public virtual void Write(string value, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write(value);
            Console.ResetColor();
        }

        public virtual void WriteLine()
        {
            Console.WriteLine();
        }

        public (int left, int top) GetCursorPosition() => (Console.CursorLeft, Console.CursorTop);

        public virtual void SetCursorPosition(int left, int top)
        {
            if (top < 0)
            {
                top = 0;
            }
            else if (top >= Console.BufferHeight)
            {
                top = Console.BufferHeight - 1;
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
