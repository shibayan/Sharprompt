using System;

namespace Sharprompt.Forms;

internal abstract class TextFormBase<T> : FormBase<T>
{
    protected TextFormBase()
    {
        KeyHandlerMaps = new()
        {
            [ConsoleKey.LeftArrow] = HandleLeftArrow,
            [ConsoleKey.RightArrow] = HandleRightArrow,
            [ConsoleKey.Home] = HandleHome,
            [ConsoleKey.End] = HandleEnd,
            [ConsoleKey.Backspace] = HandleBackspace,
            [ConsoleKey.Delete] = HandleDelete
        };
    }

    protected virtual bool HandleLeftArrow(ConsoleKeyInfo keyInfo)
    {
        if (InputBuffer.IsStart)
        {
            return false;
        }

        if (keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control))
        {
            InputBuffer.MoveToPreviousWord();
        }
        else
        {
            InputBuffer.MoveBackward();
        }

        return true;
    }

    protected virtual bool HandleRightArrow(ConsoleKeyInfo keyInfo)
    {
        if (InputBuffer.IsEnd)
        {
            return false;
        }

        if (keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control))
        {
            InputBuffer.MoveToNextWord();
        }
        else
        {
            InputBuffer.MoveForward();
        }

        return true;
    }

    protected virtual bool HandleHome(ConsoleKeyInfo keyInfo)
    {
        if (InputBuffer.IsStart)
        {
            return false;
        }

        InputBuffer.MoveToStart();

        return true;
    }

    protected virtual bool HandleEnd(ConsoleKeyInfo keyInfo)
    {
        if (InputBuffer.IsEnd)
        {
            return false;
        }

        InputBuffer.MoveToEnd();

        return true;
    }

    protected virtual bool HandleBackspace(ConsoleKeyInfo keyInfo)
    {
        if (InputBuffer.IsStart)
        {
            return false;
        }

        if (keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control))
        {
            InputBuffer.BackspaceWord();
        }
        else
        {
            InputBuffer.Backspace();
        }

        return true;
    }

    protected virtual bool HandleDelete(ConsoleKeyInfo keyInfo)
    {
        if (InputBuffer.IsEnd)
        {
            return false;
        }

        if (keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control))
        {
            InputBuffer.DeleteWord();
        }
        else
        {
            InputBuffer.Delete();
        }

        return true;
    }
}
