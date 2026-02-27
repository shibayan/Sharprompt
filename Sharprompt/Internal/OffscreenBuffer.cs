using System;
using System.Collections.Generic;

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
    private readonly List<List<TextInfo>> _outputBuffer = [[]];

    private int _cursorBottom;
    private Cursor? _pushedCursor;
    private bool _isWrittenLineCountDirty = true;
    private int _cachedWrittenLineCount;
    private int _cachedBufferWidth;

    private int WrittenLineCount
    {
        get
        {
            var bufferWidth = _consoleDriver.BufferWidth;

            if (!_isWrittenLineCountDirty && _cachedBufferWidth == bufferWidth)
            {
                return _cachedWrittenLineCount;
            }

            var count = 0;

            foreach (var line in _outputBuffer)
            {
                var lineWidth = 0;

                foreach (var textInfo in line)
                {
                    lineWidth += textInfo.Width;
                }

                count += (lineWidth - 1) / bufferWidth + 1;
            }

            _cachedBufferWidth = bufferWidth;
            _cachedWrittenLineCount = count - 1;
            _isWrittenLineCountDirty = false;

            return _cachedWrittenLineCount;
        }
    }

    public void Dispose() => _consoleDriver.Dispose();

    public void Write(string text) => Write(text, Console.ForegroundColor);

    public void Write(string text, ConsoleColor color)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        _outputBuffer[^1].Add(new TextInfo(text, color));
        _isWrittenLineCountDirty = true;
    }

    public void WriteLine()
    {
        _outputBuffer.Add([]);
        _isWrittenLineCountDirty = true;
    }

    public void PushCursor()
    {
        if (_pushedCursor is not null)
        {
            return;
        }

        var lastLine = _outputBuffer[^1];
        var width = 0;

        foreach (var textInfo in lastLine)
        {
            width += textInfo.Width;
        }

        _pushedCursor = new Cursor(width, _outputBuffer.Count - 1);
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
        _isWrittenLineCountDirty = true;
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
