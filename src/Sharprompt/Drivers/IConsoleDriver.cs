using System;
using System.Threading;

namespace Sharprompt.Drivers;

public interface IConsoleDriver : IDisposable
{
    void Beep();
    void Reset();
    void ClearLine(int top);
    ConsoleKeyInfo ReadKey();
    void Write(string value, ConsoleColor color);
    void WriteLine();
    void SetCursorPosition(int left, int top);
    bool KeyAvailable { get; }
    bool CursorVisible { set; }
    int CursorLeft { get; }
    int CursorTop { get; }
    int BufferWidth { get; }
    int BufferHeight { get; }
    int WindowWidth { get; }
    int WindowHeight { get; }
    Action CancellationCallback { get; set; }

    /// <summary>
    /// Waits for the next key press, observing <paramref name="cancellationToken"/> while waiting.
    /// </summary>
    /// <param name="cancellationToken">The token to observe while waiting for a key press.</param>
    /// <returns>The key that was pressed.</returns>
    /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was canceled before a key was read.</exception>
    /// <remarks>
    /// <para>
    /// When <paramref name="cancellationToken"/> cannot be canceled, the default implementation calls <see cref="ReadKey()"/> directly.
    /// Otherwise it polls <see cref="KeyAvailable"/> while waiting on the token, and calls <see cref="ReadKey()"/> only once
    /// <see cref="KeyAvailable"/> returns <see langword="true"/>, so a driver relying on the default implementation must report
    /// <see cref="KeyAvailable"/> accurately for cancelable prompts to receive input.
    /// </para>
    /// <para>
    /// Drivers whose input source supports cancellation natively, or that cannot report <see cref="KeyAvailable"/>, should override this method.
    /// </para>
    /// </remarks>
    ConsoleKeyInfo ReadKey(CancellationToken cancellationToken)
    {
        if (!cancellationToken.CanBeCanceled)
        {
            return ReadKey();
        }

        // ReadKey blocks and cannot be interrupted, so poll for available input while waiting on the token instead.
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (KeyAvailable)
            {
                return ReadKey();
            }

            cancellationToken.WaitHandle.WaitOne(s_keyPollingInterval);
        }
    }

    private static readonly TimeSpan s_keyPollingInterval = TimeSpan.FromMilliseconds(16);
}
