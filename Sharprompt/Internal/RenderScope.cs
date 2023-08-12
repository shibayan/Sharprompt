using System;

using Sharprompt.Drivers;

namespace Sharprompt.Internal;

internal class RenderScope : IDisposable
{
    public RenderScope(OffscreenBuffer offscreenBuffer, IConsoleDriver consoleDriver, int cursorBottom, int writtenLineCount)
    {
        _offscreenBuffer = offscreenBuffer;
        _consoleDriver = consoleDriver;
        _cursorBottom = Math.Min(cursorBottom, _consoleDriver.WindowHeight - 1);
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
