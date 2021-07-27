﻿using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

using Sharprompt.Internal;

namespace Sharprompt.Drivers
{
    internal sealed class DefaultConsoleDriver : IConsoleDriver
    {
        private int _idleReadKey = 30;
        public int IdleReadKey
        {
            get
            {
                if (_idleReadKey == 0)
                {
                    return 30;
                }
                return _idleReadKey;
            }
            set
            {
                if (value < 10)
                {
                    _idleReadKey = 10;
                }
                else if (value > 100)
                {
                    _idleReadKey = 100;
                }
                else
                {
                    _idleReadKey = value;
                }
            }
        }

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

        public ConsoleKeyInfo WaitKeypress(CancellationToken cancellationToken)
        {
            while (!KeyAvailable && !cancellationToken.IsCancellationRequested)
            {
                cancellationToken.WaitHandle.WaitOne(IdleReadKey);
            }
            if (KeyAvailable && !cancellationToken.IsCancellationRequested)
            {
                return ReadKey();
            }
            return new ConsoleKeyInfo();
        }

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
    }
}
