using System;

using Sharprompt.Drivers;

namespace Sharprompt.Internal;

/// <summary>
/// Captures the previous prompt region, clears the pending buffer, and redraws that region on disposal.
/// </summary>
internal class RenderScope : IDisposable
{
    /// <summary>
    /// Captures the previous prompt region within current terminal bounds and clears pending output.
    /// </summary>
    /// <param name="offscreenBuffer">The pending output buffer to write or render.</param>
    /// <param name="consoleDriver">The console input, output, and cursor operations used for rendering.</param>
    /// <param name="cursorBottom">The zero-based bottom row of the previous prompt in buffer coordinates.</param>
    /// <param name="writtenLineCount">The zero-based span of rows occupied by the previous prompt, excluding its first row.</param>
    public RenderScope(OffscreenBuffer offscreenBuffer, IConsoleDriver consoleDriver, int cursorBottom, int writtenLineCount)
    {
        _offscreenBuffer = offscreenBuffer;
        _consoleDriver = consoleDriver;
        // Cursor rows are buffer-relative, so clamp to the buffer rather than the window: on
        // Windows conhost the scrollback buffer is far taller than the window, and clamping to the
        // window would redraw over scrollback instead of the prompt. Where the buffer is the window
        // (Unix terminals, Windows Terminal) this still handles the window shrinking.
        _cursorBottom = Math.Min(cursorBottom, _consoleDriver.BufferHeight - 1);
        _writtenLineCount = Math.Min(writtenLineCount, _consoleDriver.WindowHeight - 1);

        _offscreenBuffer.ClearBuffer();
    }

    private readonly OffscreenBuffer _offscreenBuffer;
    private readonly IConsoleDriver _consoleDriver;
    private readonly int _cursorBottom;
    private readonly int _writtenLineCount;

    /// <summary>
    /// Hides the cursor, clears the previous prompt region, renders pending output, and shows the cursor again.
    /// </summary>
    public void Dispose()
    {
        _consoleDriver.CursorVisible = false;

        _offscreenBuffer.ClearConsole(_cursorBottom, _writtenLineCount);
        _offscreenBuffer.RenderToConsole();

        _consoleDriver.CursorVisible = true;
    }
}
