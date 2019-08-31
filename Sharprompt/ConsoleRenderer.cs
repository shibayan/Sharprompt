using System;
using System.Runtime.InteropServices;

namespace Sharprompt
{
    internal class ConsoleRenderer
    {
        public ConsoleRenderer()
        {
        }

        private int _lineCount;
        private int _errorLineCount;

        public void Close()
        {
            Console.ResetColor();

            if (Console.CursorLeft != 0)
            {
                Console.WriteLine();
            }
        }

        public void Reset()
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

        public void Write(string value)
        {
            _lineCount += (Console.CursorLeft + value.Length) / Console.BufferWidth;

            Console.Write(value);
        }

        public void Write(string value, ConsoleColor color)
        {
            var previousColor = Console.ForegroundColor;

            Console.ForegroundColor = color;

            Write(value);

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
            var left = Console.CursorLeft;

            Console.WriteLine();
            Write($">> {errorMessage}", ConsoleColor.Red);

            Console.SetCursorPosition(left, Console.CursorTop - 1);

            _errorLineCount = 1;
        }

        public void EraseLine(int y)
        {
            Console.SetCursorPosition(0, y);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                FillConsoleOutputCharacter(GetStdHandle(-11), ' ', Console.BufferWidth, new COORD { x = 0, y = (short)y }, out _);
            }
            else
            {
                Console.Write("\x1b[2K");
            }
        }

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        private static extern int FillConsoleOutputCharacter(IntPtr hConsoleOutput, char cCharactor, int nLength, COORD dwWriteCoord, out int lpNumberOfCharsWritten);

        [StructLayout(LayoutKind.Sequential)]
        private struct COORD
        {
            public short x;
            public short y;
        }
    }
}