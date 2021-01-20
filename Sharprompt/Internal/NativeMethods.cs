using System;
using System.Runtime.InteropServices;

namespace Sharprompt.Internal
{
    internal static class NativeMethods
    {
        public const int STD_OUTPUT_HANDLE = -11;
        public const int ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        public static extern bool GetConsoleMode(IntPtr hConsoleHandle, out int dwMode);

        [DllImport("kernel32.dll")]
        public static extern bool SetConsoleMode(IntPtr hConsoleHandle, int dwMode);
    }
}
