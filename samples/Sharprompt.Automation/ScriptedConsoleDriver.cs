using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using Sharprompt.Drivers;

namespace Sharprompt.Automation;

// A small test driver for the short, ASCII input in this sample. It records rendered text,
// rather than emulating a terminal screen, ANSI styles, Unicode widths, or scrolling.
internal sealed class ScriptedConsoleDriver : IConsoleDriver
{
    private readonly Queue<ConsoleKeyInfo> _keys = new();
    private readonly StringBuilder _output = new();

    public string Output => _output.ToString();

    public Action? OnRead { get; init; }

    public ScriptedConsoleDriver Key(ConsoleKey key, char keyChar = '\0')
    {
        _keys.Enqueue(new ConsoleKeyInfo(keyChar, key, false, false, false));

        return this;
    }

    public ScriptedConsoleDriver Type(string text)
    {
        foreach (var character in text)
        {
            Key(default, character);
        }

        return this;
    }

    public ScriptedConsoleDriver Enter() => Key(ConsoleKey.Enter, '\r');

    public ConsoleKeyInfo ReadKey() => ReadKey(CancellationToken.None);

    public ConsoleKeyInfo ReadKey(CancellationToken cancellationToken)
    {
        OnRead?.Invoke();
        cancellationToken.ThrowIfCancellationRequested();

        // Fail promptly when a prompt unexpectedly needs more input, instead of hanging CI.
        return _keys.Count > 0 ? _keys.Dequeue() : throw new InvalidOperationException("Scripted input exhausted.");
    }

    public void Write(string value, ConsoleColor color)
    {
        _output.Append(value);
        CursorLeft += value.Length;
    }

    public void WriteLine()
    {
        _output.Append('\n');
        CursorLeft = 0;
        CursorTop++;
    }

    public void SetCursorPosition(int left, int top)
    {
        CursorLeft = left;
        CursorTop = top;
    }

    public void ClearLine(int top) => SetCursorPosition(0, top);

    public void Beep() { }

    public void Reset() { }

    public void Dispose() { }

    // Render after each key. ReadKey(token) is overridden so cancellation never polls here.
    public bool KeyAvailable => false;

    public bool CursorVisible { set { } }

    public int CursorLeft { get; private set; }

    public int CursorTop { get; private set; }

    public int BufferWidth => 500;

    public int BufferHeight => 1000;

    public int WindowWidth => 500;

    public int WindowHeight => 1000;

    public Action CancellationCallback { get; set; } = () => { };
}
