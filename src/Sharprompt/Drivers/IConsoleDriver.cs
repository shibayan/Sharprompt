using System;
using System.Threading;

namespace Sharprompt.Drivers;

/// <summary>
/// Terminal input, output, and cursor operations used by prompts. A prompt owns and disposes its driver.
/// </summary>
/// <remarks>Use Prompt.ConsoleDriverFactory to replace the terminal in tests. Implement accurate dimensions and cursor positions, and invoke CancellationCallback for Ctrl+C.</remarks>
public interface IConsoleDriver : IDisposable
{
    /// <summary>
    /// Signals an invalid key or operation.
    /// </summary>
    void Beep();
    /// <summary>
    /// Restores terminal state after a prompt.
    /// </summary>
    void Reset();
    /// <summary>
    /// Clears the buffer row at the given zero-based position.
    /// </summary>
    /// <param name="top">The zero-based buffer row.</param>
    void ClearLine(int top);
    /// <summary>
    /// Reads a key without echoing it. Invoke CancellationCallback when Ctrl+C is read.
    /// </summary>
    /// <returns>The key that was read.</returns>
    ConsoleKeyInfo ReadKey();
    /// <summary>
    /// Writes text in the given color and updates cursor positions.
    /// </summary>
    /// <param name="value">The text to display.</param>
    /// <param name="color">The foreground color.</param>
    void Write(string value, ConsoleColor color);
    /// <summary>
    /// Starts a new line and updates cursor positions.
    /// </summary>
    void WriteLine();
    /// <summary>
    /// Moves the cursor to zero-based buffer coordinates.
    /// </summary>
    /// <param name="left">The zero-based buffer column.</param>
    /// <param name="top">The zero-based buffer row.</param>
    void SetCursorPosition(int left, int top);
    /// <summary>
    /// Whether a key can be read without blocking. The default cancelable ReadKey implementation polls this property.
    /// </summary>
    bool KeyAvailable { get; }
    /// <summary>
    /// Sets whether the cursor is visible.
    /// </summary>
    bool CursorVisible { set; }
    /// <summary>
    /// The zero-based cursor column in the buffer.
    /// </summary>
    int CursorLeft { get; }
    /// <summary>
    /// The zero-based cursor row in the buffer.
    /// </summary>
    int CursorTop { get; }
    /// <summary>
    /// The buffer width in terminal columns.
    /// </summary>
    int BufferWidth { get; }
    /// <summary>
    /// The buffer height in rows.
    /// </summary>
    int BufferHeight { get; }
    /// <summary>
    /// The visible terminal width in columns.
    /// </summary>
    int WindowWidth { get; }
    /// <summary>
    /// The visible terminal height in rows.
    /// </summary>
    int WindowHeight { get; }
    /// <summary>
    /// The cancellation action assigned by the prompt. Invoke it when Ctrl+C is read.
    /// </summary>
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
