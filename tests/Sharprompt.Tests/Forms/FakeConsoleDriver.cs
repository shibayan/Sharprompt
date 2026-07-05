using System;
using System.Collections.Generic;
using System.Text;

using Sharprompt.Drivers;

namespace Sharprompt.Tests;

// Scriptable console driver for form interaction tests. Keys are consumed from a queue,
// and cursor movement is emulated just enough for OffscreenBuffer's rendering logic.
internal sealed class FakeConsoleDriver : IConsoleDriver
{
    private readonly Queue<ConsoleKeyInfo> _keys = new();
    private readonly StringBuilder _output = new();

    private int _cursorLeft;
    private int _cursorTop;

    public string Output => _output.ToString();

    public void EnqueueKey(ConsoleKey key, ConsoleModifiers modifiers = 0, char keyChar = '\0')
        => _keys.Enqueue(new ConsoleKeyInfo(keyChar, key, modifiers.HasFlag(ConsoleModifiers.Shift), modifiers.HasFlag(ConsoleModifiers.Alt), modifiers.HasFlag(ConsoleModifiers.Control)));

    public void EnqueueText(string text)
    {
        foreach (var c in text)
        {
            _keys.Enqueue(new ConsoleKeyInfo(c, default, false, false, false));
        }
    }

    public void EnqueueEnter() => EnqueueKey(ConsoleKey.Enter, keyChar: '\r');

    public void Dispose()
    {
    }

    public void Beep()
    {
    }

    public void Reset()
    {
    }

    public void ClearLine(int top) => SetCursorPosition(0, top);

    public ConsoleKeyInfo ReadKey()
        => _keys.Count > 0 ? _keys.Dequeue() : throw new InvalidOperationException("No more keys are queued. The form is still waiting for input.");

    public void Write(string value, ConsoleColor color)
    {
        _output.Append(value);
        _cursorLeft += value.Length;
    }

    public void WriteLine()
    {
        _output.Append('\n');
        _cursorLeft = 0;
        _cursorTop++;
    }

    public void SetCursorPosition(int left, int top)
    {
        _cursorLeft = left;
        _cursorTop = top;
    }

    public bool KeyAvailable => _keys.Count > 0;

    public bool CursorVisible { get; set; } = true;

    public int CursorLeft => _cursorLeft;

    public int CursorTop => _cursorTop;

    public int BufferWidth => 80;

    public int BufferHeight => 300;

    public int WindowWidth => 80;

    public int WindowHeight => 300;

    public Action CancellationCallback { get; set; } = () => { };
}
