using System;
using System.Runtime.InteropServices;

namespace Sharprompt.Internal;

internal static class NativeMethods
{
    // ReSharper disable once InconsistentNaming
    public const int STD_OUTPUT_HANDLE = -11;

    // ReSharper disable once InconsistentNaming
    public const int ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;

    // ReSharper disable once InconsistentNaming
    public const uint CF_UNICODETEXT = 13;

    [DllImport("kernel32.dll")]
    public static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll")]
    public static extern bool GetConsoleMode(IntPtr hConsoleHandle, out int dwMode);

    [DllImport("kernel32.dll")]
    public static extern bool SetConsoleMode(IntPtr hConsoleHandle, int dwMode);

    [DllImport("user32.dll")]
    public static extern bool IsClipboardFormatAvailable(uint format);

    [DllImport("user32.dll")]
    public static extern bool OpenClipboard(IntPtr hWndNewOwner);

    [DllImport("user32.dll")]
    public static extern IntPtr GetClipboardData(uint uFormat);

    [DllImport("user32.dll")]
    public static extern bool CloseClipboard();

    [DllImport("kernel32.dll")]
    public static extern IntPtr GlobalLock(IntPtr hMem);

    [DllImport("kernel32.dll")]
    public static extern bool GlobalUnlock(IntPtr hMem);
}
