using System;
using System.Collections.Generic;
using System.Text;

using Sharprompt.Drivers;

namespace Sharprompt.NativeAot;

// Replays scripted keys instead of reading the terminal and records everything the prompts write,
// so the published binary can be run unattended in CI.
internal sealed class ScriptedConsoleDriver : IConsoleDriver
{
    private readonly Queue<ConsoleKeyInfo> _keys = new();
    private readonly StringBuilder _output = new();

    private int _cursorLeft;
    private int _cursorTop;

    public string Output => _output.ToString();

    public int RemainingKeys => _keys.Count;

    public ScriptedConsoleDriver Key(ConsoleKey key, char keyChar = '\0', ConsoleModifiers modifiers = 0)
    {
        _keys.Enqueue(new ConsoleKeyInfo(keyChar, key, modifiers.HasFlag(ConsoleModifiers.Shift), modifiers.HasFlag(ConsoleModifiers.Alt), modifiers.HasFlag(ConsoleModifiers.Control)));

        return this;
    }

    public ScriptedConsoleDriver Type(string text)
    {
        foreach (var c in text)
        {
            Key(default, c);
        }

        return this;
    }

    public ScriptedConsoleDriver Enter() => Key(ConsoleKey.Enter, '\r');

    public ScriptedConsoleDriver Space() => Key(ConsoleKey.Spacebar, ' ');

    public ScriptedConsoleDriver Tab() => Key(ConsoleKey.Tab, '\t');

    public ScriptedConsoleDriver Down(int count = 1)
    {
        for (var i = 0; i < count; i++)
        {
            Key(ConsoleKey.DownArrow);
        }

        return this;
    }

    public ScriptedConsoleDriver Backspace(int count = 1)
    {
        for (var i = 0; i < count; i++)
        {
            Key(ConsoleKey.Backspace, '\b');
        }

        return this;
    }

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
        => _keys.Count > 0 ? _keys.Dequeue() : throw new VerificationException("No more scripted keys. The prompt is still waiting for input.");

    public void Write(string value, ConsoleColor color)
    {
        _output.Append(value);

        // Approximate the cursor with one column per character; the buffer is wide enough
        // that the scenarios never wrap, so OffscreenBuffer's cursor math stays consistent.
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

    public bool CursorVisible { set { } }

    public int CursorLeft => _cursorLeft;

    public int CursorTop => _cursorTop;

    public int BufferWidth => 500;

    public int BufferHeight => 1000;

    public int WindowWidth => 500;

    public int WindowHeight => 1000;

    public Action CancellationCallback { get; set; } = () => { };
}
