using System;
using System.Runtime.InteropServices;

namespace Sharprompt.Drivers
{
    internal class WindowsConsoleDriver : DefaultConsoleDriver
    {
        protected override void EraseLine(int y)
        {
            Console.SetCursorPosition(0, y);

            FillConsoleOutputCharacter(GetStdHandle(-11), ' ', Console.BufferWidth, new COORD { X = 0, Y = (short)y }, out _);
        }

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
