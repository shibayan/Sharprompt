using System;

using Sharprompt.Drivers;

namespace Sharprompt.Internal;

internal class RenderScope : IDisposable
{
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

    public void Dispose()
    {
        _consoleDriver.CursorVisible = false;

        _offscreenBuffer.ClearConsole(_cursorBottom, _writtenLineCount);
        _offscreenBuffer.RenderToConsole();

        _consoleDriver.CursorVisible = true;
    }
}
