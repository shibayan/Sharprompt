using System;
using System.Threading;

using Sharprompt.Drivers;

namespace Sharprompt.Tests;

// Console driver that overrides ReadKey(CancellationToken) and never reports KeyAvailable,
// like a driver whose input source supports cancellation but cannot be peeked.
// Keys and rendering are delegated to the wrapped FakeConsoleDriver.
internal sealed class CancelableConsoleDriver : IConsoleDriver
{
    private readonly FakeConsoleDriver _inner = new();

    public FakeConsoleDriver Inner => _inner;

    public int ReadKeyWithTokenCount { get; private set; }

    public bool ResetCalled { get; private set; }

    // Overrides the exception thrown when the token is canceled, to emulate drivers that throw for a different token.
    public Func<CancellationToken, Exception>? CanceledExceptionFactory { get; set; }

    public ConsoleKeyInfo ReadKey(CancellationToken cancellationToken)
    {
        ReadKeyWithTokenCount++;

        // Inner.KeyAvailable invokes KeysExhausted when the queue is empty, so tests can cancel the token here.
        if (!_inner.KeyAvailable)
        {
            cancellationToken.WaitHandle.WaitOne(TimeSpan.FromSeconds(5));
        }

        if (cancellationToken.IsCancellationRequested)
        {
            throw CanceledExceptionFactory?.Invoke(cancellationToken) ?? new OperationCanceledException(cancellationToken);
        }

        return _inner.ReadKey();
    }

    public void Dispose() => _inner.Dispose();

    public void Beep() => _inner.Beep();

    public void Reset()
    {
        ResetCalled = true;

        _inner.Reset();
    }

    public void ClearLine(int top) => _inner.ClearLine(top);

    public ConsoleKeyInfo ReadKey() => throw new InvalidOperationException("ReadKey(CancellationToken) must be used instead.");

    public void Write(string value, ConsoleColor color) => _inner.Write(value, color);

    public void WriteLine() => _inner.WriteLine();

    public void SetCursorPosition(int left, int top) => _inner.SetCursorPosition(left, top);

    public bool KeyAvailable => false;

    public bool CursorVisible { set => _inner.CursorVisible = value; }

    public int CursorLeft => _inner.CursorLeft;

    public int CursorTop => _inner.CursorTop;

    public int BufferWidth => _inner.BufferWidth;

    public int BufferHeight => _inner.BufferHeight;

    public int WindowWidth => _inner.WindowWidth;

    public int WindowHeight => _inner.WindowHeight;

    public Action CancellationCallback { get; set; } = () => { };
}
