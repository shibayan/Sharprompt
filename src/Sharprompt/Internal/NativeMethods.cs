using System;
using System.Runtime.InteropServices;

namespace Sharprompt.Internal;

/// <summary>
/// Declares the Windows console functions and constants used to enable virtual terminal processing.
/// </summary>
internal static class NativeMethods
{
    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// The Windows identifier used to obtain the standard output handle.
    /// </summary>
    public const int STD_OUTPUT_HANDLE = -11;

    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// The console mode flag that enables interpretation of virtual terminal output sequences.
    /// </summary>
    public const int ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;

    /// <summary>
    /// Obtains the Windows handle associated with a standard input, output, or error identifier.
    /// </summary>
    /// <param name="nStdHandle">The Windows identifier of the requested standard handle.</param>
    /// <returns>The requested standard handle, or the Windows failure or missing-handle value.</returns>
    [DllImport("kernel32.dll")]
    public static extern IntPtr GetStdHandle(int nStdHandle);

    /// <summary>
    /// Reads the input or output mode flags of a Windows console handle.
    /// </summary>
    /// <param name="hConsoleHandle">The console handle whose mode is queried or changed.</param>
    /// <param name="dwMode">The console mode flags read or written.</param>
    /// <returns>True if the mode was read successfully; otherwise false.</returns>
    [DllImport("kernel32.dll")]
    public static extern bool GetConsoleMode(IntPtr hConsoleHandle, out int dwMode);

    /// <summary>
    /// Sets the input or output mode flags of a Windows console handle.
    /// </summary>
    /// <param name="hConsoleHandle">The console handle whose mode is queried or changed.</param>
    /// <param name="dwMode">The console mode flags read or written.</param>
    /// <returns>True if the mode was changed successfully; otherwise false.</returns>
    [DllImport("kernel32.dll")]
    public static extern bool SetConsoleMode(IntPtr hConsoleHandle, int dwMode);
}
