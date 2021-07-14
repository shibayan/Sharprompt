using System;
using System.Runtime.InteropServices;
using System.Text;

using Sharprompt.Internal;

namespace Sharprompt.Drivers
{
    internal sealed class DefaultConsoleDriver : IConsoleDriver
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

        public void Dispose()
        {
            Reset();

            Console.CancelKeyPress -= RequestCancellation;
        }

        #endregion

        #region IConsoleDriver

        public void Beep() => Console.Write("\a");

        public void Reset()
        {
            Console.CursorVisible = true;
            Console.ResetColor();

            if (CursorLeft != 0)
            {
                WriteLine();
            }
        }

        public void ClearLine(int top)
        {
            SetCursorPosition(0, top);

            Console.Write("\x1b[2K");
        }

        public ConsoleKeyInfo ReadKey() => Console.ReadKey(true);

        public string ReadLine()
        {
            var startIndex = 0;
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

                        HandleLeftAllow(width);
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

                        HandleRightAllow(width);

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

                        HandleLeftAllow(width);

                        inputBuffer.Remove(startIndex, 1);

                        var (left, top) = GetCursorPosition();

                        for (var i = startIndex; i < inputBuffer.Length; i++)
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

                        for (var i = startIndex; i < inputBuffer.Length; i++)
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

                    for (var i = startIndex; i < inputBuffer.Length; i++)
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

        public void Write(string value, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write(value);
            Console.ResetColor();
        }

        public void WriteLine() => Console.WriteLine();

        public (int left, int top) GetCursorPosition() => (Console.CursorLeft, Console.CursorTop);

        public void SetCursorPosition(int left, int top)
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

        public bool KeyAvailable => Console.KeyAvailable;

        public bool CursorVisible
        {
            get => Console.CursorVisible;
            set => Console.CursorVisible = value;
        }

        public int CursorLeft => Console.CursorLeft;

        public int CursorTop => Console.CursorTop;

        public int BufferWidth => Console.BufferWidth;

        public int BufferHeight => Console.BufferHeight;

        #endregion

        private void RequestCancellation(object sender, ConsoleCancelEventArgs e)
        {
            Reset();
        }

        private void HandleLeftAllow(int width)
        {
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

        private void HandleRightAllow(int width)
        {
            if (Console.CursorLeft + width >= Console.BufferWidth)
            {
                Console.CursorTop += 1;
                Console.CursorLeft = 0;
            }
            else
            {
                Console.CursorLeft += width;
            }
        }
    }
}
