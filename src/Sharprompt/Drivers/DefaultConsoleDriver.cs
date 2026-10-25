using System;
using System.IO;
using System.Runtime.InteropServices;

using Sharprompt.Internal;
using Sharprompt.Strings;

namespace Sharprompt.Drivers;

/// <summary>
/// Uses System.Console to read keys and render prompts in an interactive terminal.
/// </summary>
public sealed class DefaultConsoleDriver : IConsoleDriver
{
    // Must never throw: an exception here would surface as TypeInitializationException
    // and make the type unusable for the rest of the process.
    static DefaultConsoleDriver()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var hConsole = NativeMethods.GetStdHandle(NativeMethods.STD_OUTPUT_HANDLE);

            if (!NativeMethods.GetConsoleMode(hConsole, out var mode))
            {
                return;
            }

            NativeMethods.SetConsoleMode(hConsole, mode | NativeMethods.ENABLE_VIRTUAL_TERMINAL_PROCESSING);
        }
    }

    /// <summary>
    /// Creates a terminal driver and saves the previous Ctrl+C input mode.
    /// </summary>
    /// <exception cref="InvalidOperationException">Standard input or output is redirected, or the terminal cannot enable Ctrl+C input.</exception>
    public DefaultConsoleDriver()
    {
        if (Console.IsInputRedirected || Console.IsOutputRedirected)
        {
            throw new InvalidOperationException(Resource.Message_NotSupportedEnvironment);
        }

        _previousTreatControlCAsInput = Console.TreatControlCAsInput;

        try
        {
            Console.TreatControlCAsInput = true;
        }
        catch (IOException)
        {
            throw new InvalidOperationException(Resource.Message_NotSupportedEnvironment);
        }
    }

    private readonly bool _previousTreatControlCAsInput;

    #region IDisposable

    /// <summary>
    /// Restores cursor visibility, console colors, and the Ctrl+C input mode saved by the constructor.
    /// </summary>
    public void Dispose() => Reset();

    #endregion

    #region IConsoleDriver

    // ReSharper disable once LocalizableElement
    /// <inheritdoc cref="IConsoleDriver.Beep"/>
    public void Beep() => Console.Write("\a");

    /// <inheritdoc cref="IConsoleDriver.Reset"/>
    public void Reset()
    {
        Console.CursorVisible = true;
        Console.ResetColor();

        Console.TreatControlCAsInput = _previousTreatControlCAsInput;
    }

    /// <inheritdoc cref="IConsoleDriver.ClearLine"/>
    public void ClearLine(int top)
    {
        SetCursorPosition(0, top);

        // ReSharper disable once LocalizableElement
        Console.Write("\x1b[2K");
    }

    /// <inheritdoc cref="IConsoleDriver.ReadKey()"/>
    public ConsoleKeyInfo ReadKey()
    {
        var keyInfo = Console.ReadKey(true);

        if (keyInfo is { Key: ConsoleKey.C, Modifiers: ConsoleModifiers.Control })
        {
            CancellationCallback.Invoke();
        }

        return keyInfo;
    }

    /// <inheritdoc cref="IConsoleDriver.Write"/>
    public void Write(string value, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.Write(value);
        Console.ResetColor();
    }

    /// <inheritdoc cref="IConsoleDriver.WriteLine"/>
    public void WriteLine() => Console.WriteLine();

    /// <inheritdoc cref="IConsoleDriver.SetCursorPosition"/>
    public void SetCursorPosition(int left, int top)
    {
        // The window may have been resized after the coordinates were calculated,
        // so clamp them to the current buffer size to avoid ArgumentOutOfRangeException.
        left = Math.Clamp(left, 0, Console.BufferWidth - 1);
        top = Math.Clamp(top, 0, Console.BufferHeight - 1);

        try
        {
            Console.SetCursorPosition(left, top);
        }
        catch (ArgumentOutOfRangeException)
        {
            // The buffer was shrunk between the clamp and the call. Skip repositioning;
            // the next render pass will use the updated buffer size.
        }
    }

    /// <inheritdoc cref="IConsoleDriver.KeyAvailable"/>
    public bool KeyAvailable => Console.KeyAvailable;

    /// <inheritdoc cref="IConsoleDriver.CursorVisible"/>
    public bool CursorVisible
    {
        set => Console.CursorVisible = value;
    }

    /// <inheritdoc cref="IConsoleDriver.CursorLeft"/>
    public int CursorLeft => Console.CursorLeft;

    /// <inheritdoc cref="IConsoleDriver.CursorTop"/>
    public int CursorTop => Console.CursorTop;

    /// <inheritdoc cref="IConsoleDriver.BufferWidth"/>
    public int BufferWidth => Console.BufferWidth;

    /// <inheritdoc cref="IConsoleDriver.BufferHeight"/>
    public int BufferHeight => Console.BufferHeight;

    /// <inheritdoc cref="IConsoleDriver.WindowWidth"/>
    public int WindowWidth => Console.WindowWidth;

    /// <inheritdoc cref="IConsoleDriver.WindowHeight"/>
    public int WindowHeight => Console.WindowHeight;

    /// <inheritdoc cref="IConsoleDriver.CancellationCallback"/>
    public Action CancellationCallback { get; set; } = () => { };

    #endregion
}
