using System;
using System.Runtime.InteropServices;

namespace Sharprompt.Drivers
{
    internal class WindowsConsoleDriver : DefaultConsoleDriver
    {
        public override void ClearLine(int top)
        {
            SetCursorPosition(0, top);

            FillConsoleOutputCharacter(GetStdHandle(-11), ' ', Console.BufferWidth, new COORD { X = 0, Y = (short)top }, out _);

            SetCursorPosition(0, top);
        }

        public override bool IsUnicodeSupported => Console.OutputEncoding.CodePage == 1200 || Console.OutputEncoding.CodePage == 65001;

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        private static extern int FillConsoleOutputCharacter(IntPtr hConsoleOutput, char cCharactor, int nLength, COORD dwWriteCoord, out int lpNumberOfCharsWritten);

        [StructLayout(LayoutKind.Sequential)]
        private struct COORD
        {
            public short X;
            public short Y;
        }
    }
}
