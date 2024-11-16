using System;
using System.Collections.Generic;
using System.Linq;

using Sharprompt.Drivers;

namespace Sharprompt.Internal;

internal class OffscreenBuffer : IDisposable
{
    public OffscreenBuffer(IConsoleDriver consoleDriver)
    {
        _consoleDriver = consoleDriver;

        _cursorBottom = _consoleDriver.CursorTop;
    }

    private readonly IConsoleDriver _consoleDriver;
    private readonly List<List<TextInfo>> _outputBuffer = [new()];

    private int _cursorBottom;
    private Cursor? _pushedCursor;

    private int WrittenLineCount => _outputBuffer.Sum(x => (x.Sum(xs => xs.Width) - 1) / _consoleDriver.BufferWidth + 1) - 1;

    public void Dispose() => _consoleDriver.Dispose();

    public void Write(string text) => Write(text, Console.ForegroundColor);

    public void Write(string text, ConsoleColor color)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        _outputBuffer.Last().Add(new TextInfo(text, color));
    }

    public void WriteLine() => _outputBuffer.Add([]);

    public void PushCursor()
    {
        if (_pushedCursor is not null)
        {
            return;
        }

        _pushedCursor = new Cursor(_outputBuffer.Last().Sum(x => x.Width), _outputBuffer.Count - 1);
    }

    public IDisposable BeginRender() => new RenderScope(this, _consoleDriver, _cursorBottom, WrittenLineCount);

    public void RenderToConsole()
    {
        for (var i = 0; i < _outputBuffer.Count; i++)
        {
            var lineBuffer = _outputBuffer[i];

            foreach (var textInfo in lineBuffer)
            {
                _consoleDriver.Write(textInfo.Text, textInfo.Color);
            }

            if (i < _outputBuffer.Count - 1)
            {
                _consoleDriver.WriteLine();
            }
        }

        _cursorBottom = _consoleDriver.CursorTop;

        if (_pushedCursor is null)
        {
            return;
        }

        var physicalLeft = _pushedCursor.Left % _consoleDriver.BufferWidth;
        var physicalTop = _pushedCursor.Top + (_pushedCursor.Left / _consoleDriver.BufferWidth);

        var consoleTop = _cursorBottom - WrittenLineCount + physicalTop;

        if (_pushedCursor.Left > 0 && physicalLeft == 0)
        {
            _consoleDriver.WriteLine();

            if (consoleTop == _consoleDriver.BufferHeight)
            {
                _cursorBottom--;
                consoleTop--;
            }
        }

        _consoleDriver.SetCursorPosition(physicalLeft, consoleTop);
    }

    public void ClearConsole(int cursorBottom, int writtenLineCount)
    {
        for (var i = 0; i <= writtenLineCount; i++)
        {
            _consoleDriver.ClearLine(cursorBottom - i);
        }
    }

    public void ClearBuffer()
    {
        _outputBuffer.Clear();
        _outputBuffer.Add([]);

        _pushedCursor = null;
    }

    public void Cancel()
    {
        _consoleDriver.Reset();
        _consoleDriver.SetCursorPosition(0, _cursorBottom);

        _consoleDriver.WriteLine();
    }

    private record Cursor(int Left, int Top);

    private readonly struct TextInfo
    {
        public TextInfo(string text, ConsoleColor color)
        {
            ArgumentNullException.ThrowIfNull(text);

            Text = text;
            Color = color;
            Width = text.GetWidth();
        }

        public string Text { get; }
        public ConsoleColor Color { get; }
        public int Width { get; }
    }
}
