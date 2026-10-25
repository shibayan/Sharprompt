using System;

using Sharprompt.Internal;

namespace Sharprompt.Forms;

/// <summary>
/// Adds grapheme-aware cursor movement and text-editing key bindings to a prompt.
/// </summary>
/// <typeparam name="T">The result type returned by the prompt or template.</typeparam>
internal abstract class TextFormBase<T> : FormBase<T>
{
    /// <summary>
    /// Creates a text prompt and registers cursor movement, character deletion, and word-editing bindings.
    /// </summary>
    /// <param name="configuration">The symbols, colors, console factory, and cancellation settings to use.</param>
    protected TextFormBase(PromptConfiguration configuration) : base(configuration)
    {
        KeyHandlerMaps = new(KeyHandlerMaps)
        {
            [new ConsoleKeyBinding(ConsoleKey.LeftArrow)] = HandleLeftArrow,
            [new ConsoleKeyBinding(ConsoleKey.LeftArrow, ConsoleModifiers.Control)] = HandleCtrlLeftArrow,
            [new ConsoleKeyBinding(ConsoleKey.RightArrow)] = HandleRightArrow,
            [new ConsoleKeyBinding(ConsoleKey.RightArrow, ConsoleModifiers.Control)] = HandleCtrlRightArrow,
            [new ConsoleKeyBinding(ConsoleKey.Home)] = HandleHome,
            [new ConsoleKeyBinding(ConsoleKey.End)] = HandleEnd,
            [new ConsoleKeyBinding(ConsoleKey.Backspace)] = HandleBackspace,
            [new ConsoleKeyBinding(ConsoleKey.Backspace, ConsoleModifiers.Control)] = HandleCtrlBackspace,
            [new ConsoleKeyBinding(ConsoleKey.Delete)] = HandleDelete,
            [new ConsoleKeyBinding(ConsoleKey.Delete, ConsoleModifiers.Control)] = HandleCtrlDelete
        };
    }

    /// <summary>
    /// Moves the cursor to the previous Unicode text element unless it is already at the start.
    /// </summary>
    /// <returns>True when the key changed or handled the input; otherwise false.</returns>
    protected virtual bool HandleLeftArrow()
    {
        if (InputBuffer.IsStart)
        {
            return false;
        }

        InputBuffer.MoveBackward();

        return true;
    }

    /// <summary>
    /// Moves the cursor to the preceding word boundary unless it is already at the start.
    /// </summary>
    /// <returns>True when the key changed or handled the input; otherwise false.</returns>
    protected virtual bool HandleCtrlLeftArrow()
    {
        if (InputBuffer.IsStart)
        {
            return false;
        }

        InputBuffer.MoveToPreviousWord();

        return true;
    }

    /// <summary>
    /// Moves the cursor to the next Unicode text element unless it is already at the end.
    /// </summary>
    /// <returns>True when the key changed or handled the input; otherwise false.</returns>
    protected virtual bool HandleRightArrow()
    {
        if (InputBuffer.IsEnd)
        {
            return false;
        }

        InputBuffer.MoveForward();

        return true;
    }

    /// <summary>
    /// Moves the cursor to the next word boundary unless it is already at the end.
    /// </summary>
    /// <returns>True when the key changed or handled the input; otherwise false.</returns>
    protected virtual bool HandleCtrlRightArrow()
    {
        if (InputBuffer.IsEnd)
        {
            return false;
        }

        InputBuffer.MoveToNextWord();

        return true;
    }

    /// <summary>
    /// Moves the edit cursor to the start of the input.
    /// </summary>
    /// <returns>True when the key changed or handled the input; otherwise false.</returns>
    protected virtual bool HandleHome()
    {
        if (InputBuffer.IsStart)
        {
            return false;
        }

        InputBuffer.MoveToStart();

        return true;
    }

    /// <summary>
    /// Moves the edit cursor to the end of the input.
    /// </summary>
    /// <returns>True when the key changed or handled the input; otherwise false.</returns>
    protected virtual bool HandleEnd()
    {
        if (InputBuffer.IsEnd)
        {
            return false;
        }

        InputBuffer.MoveToEnd();

        return true;
    }

    /// <summary>
    /// Deletes the Unicode text element before the cursor unless the cursor is at the start.
    /// </summary>
    /// <returns>True when the key changed or handled the input; otherwise false.</returns>
    protected virtual bool HandleBackspace()
    {
        if (InputBuffer.IsStart)
        {
            return false;
        }

        InputBuffer.Backspace();

        return true;
    }

    /// <summary>
    /// Deletes preceding whitespace and the preceding word unless the cursor is at the start.
    /// </summary>
    /// <returns>True when the key changed or handled the input; otherwise false.</returns>
    protected virtual bool HandleCtrlBackspace()
    {
        if (InputBuffer.IsStart)
        {
            return false;
        }

        InputBuffer.BackspaceWord();

        return true;
    }

    /// <summary>
    /// Deletes the Unicode text element at the cursor unless the cursor is at the end.
    /// </summary>
    /// <returns>True when the key changed or handled the input; otherwise false.</returns>
    protected virtual bool HandleDelete()
    {
        if (InputBuffer.IsEnd)
        {
            return false;
        }

        InputBuffer.Delete();

        return true;
    }

    /// <summary>
    /// Deletes the following word and its trailing whitespace unless the cursor is at the end.
    /// </summary>
    /// <returns>True when the key changed or handled the input; otherwise false.</returns>
    protected virtual bool HandleCtrlDelete()
    {
        if (InputBuffer.IsEnd)
        {
            return false;
        }

        InputBuffer.DeleteWord();

        return true;
    }
}
