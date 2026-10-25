using System;
using System.Collections.Generic;

using Sharprompt.Drivers;

namespace Sharprompt.Internal;

/// <summary>
/// Accumulates colored, normalized text and maps its logical cursor to physical terminal coordinates.
/// </summary>
internal class OffscreenBuffer
{
    /// <summary>
    /// Creates a pending output buffer anchored at the driver current cursor row.
    /// </summary>
    /// <param name="consoleDriver">The console input, output, and cursor operations used for rendering.</param>
    /// <param name="configuration">The symbols, colors, console factory, and cancellation settings to use.</param>
    public OffscreenBuffer(IConsoleDriver consoleDriver, PromptConfiguration configuration)
    {
        _consoleDriver = consoleDriver;
        _configuration = configuration;

        _cursorBottom = _consoleDriver.CursorTop;
    }

    private readonly IConsoleDriver _consoleDriver;
    private readonly PromptConfiguration _configuration;
    private readonly List<List<TextInfo>> _outputBuffer = [[]];

    private int _cursorBottom;
    private Cursor? _pushedCursor;
    private bool _isWrittenLineCountDirty = true;
    private int _cachedWrittenLineCount;
    private int _cachedBufferWidth;

    /// <summary>
    /// The symbols and colors used by prompt-component writers.
    /// </summary>
    internal PromptConfiguration Configuration => _configuration;

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
                count += (GetWidth(line) - 1) / bufferWidth + 1;
            }

            _cachedBufferWidth = bufferWidth;
            _cachedWrittenLineCount = count - 1;
            _isWrittenLineCountDirty = false;

            return _cachedWrittenLineCount;
        }
    }

    /// <summary>
    /// Appends normalized text using the current console foreground color.
    /// </summary>
    /// <param name="text">The text to normalize, measure, or buffer.</param>
    public void Write(string text) => Write(text, Console.ForegroundColor);

    /// <summary>
    /// Appends normalized text in the supplied color, splitting embedded line breaks into logical lines.
    /// </summary>
    /// <param name="text">The text to normalize, measure, or buffer.</param>
    /// <param name="color">The foreground color to associate with the buffered text.</param>
    public void Write(string text, ConsoleColor color)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        // Control characters are normalized before measuring, so the line count and the cursor position
        // match what the terminal shows; line breaks in the text become lines of the buffer.
        var lines = ConsoleText.Normalize(text, GetWidth(_outputBuffer[^1]));

        for (var i = 0; i < lines.Count; i++)
        {
            if (i > 0)
            {
                _outputBuffer.Add([]);
            }

            if (lines[i].Length > 0)
            {
                _outputBuffer[^1].Add(new TextInfo(lines[i], color));
            }
        }

        _isWrittenLineCountDirty = true;
    }

    /// <summary>
    /// Starts a new logical output line and invalidates the cached physical line count.
    /// </summary>
    public void WriteLine()
    {
        _outputBuffer.Add([]);
        _isWrittenLineCountDirty = true;
    }

    /// <summary>
    /// Records the first requested edit-cursor position in the pending output.
    /// </summary>
    public void PushCursor()
    {
        if (_pushedCursor is not null)
        {
            return;
        }

        _pushedCursor = new Cursor(GetWidth(_outputBuffer[^1]), _outputBuffer.Count - 1);
    }

    private static int GetWidth(List<TextInfo> line)
    {
        var width = 0;

        foreach (var textInfo in line)
        {
            width += textInfo.Width;
        }

        return width;
    }

    /// <summary>
    /// Starts a render scope that clears the pending buffer and redraws the previous prompt region on disposal.
    /// </summary>
    /// <returns>A scope that redraws the prompt when disposed.</returns>
    public IDisposable BeginRender() => new RenderScope(this, _consoleDriver, _cursorBottom, WrittenLineCount);

    /// <summary>
    /// Writes buffered colored lines and translates the saved logical cursor into physical terminal coordinates.
    /// </summary>
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

        var bufferWidth = _consoleDriver.BufferWidth;
        var physicalLeft = _pushedCursor.Left % bufferWidth;
        var physicalTop = _pushedCursor.Left / bufferWidth;

        // Lines before the cursor's line can wrap onto several rows, such as those of a multi-line message.
        for (var i = 0; i < _pushedCursor.Top; i++)
        {
            physicalTop += (GetWidth(_outputBuffer[i]) - 1) / bufferWidth + 1;
        }

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

    /// <summary>
    /// Clears the previous prompt rows upwards from its bottom buffer row, including both endpoints.
    /// </summary>
    /// <param name="cursorBottom">The zero-based bottom row of the previous prompt in buffer coordinates.</param>
    /// <param name="writtenLineCount">The zero-based span of rows occupied by the previous prompt, excluding its first row.</param>
    public void ClearConsole(int cursorBottom, int writtenLineCount)
    {
        for (var i = 0; i <= writtenLineCount; i++)
        {
            _consoleDriver.ClearLine(cursorBottom - i);
        }
    }

    /// <summary>
    /// Discards pending text and the saved edit cursor while retaining the buffer configuration.
    /// </summary>
    public void ClearBuffer()
    {
        _outputBuffer.Clear();
        _outputBuffer.Add([]);

        _pushedCursor = null;
        _isWrittenLineCountDirty = true;
    }

    /// <summary>
    /// Resets terminal state and starts a new line at the bottom of the canceled prompt.
    /// </summary>
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
            Width = ConsoleText.MeasureWidth(text);
        }

        public string Text { get; }
        public ConsoleColor Color { get; }
        public int Width { get; }
    }
}
