using System;

using Sharprompt.Internal;

namespace Sharprompt.Forms;

internal abstract class TextFormBase<T> : FormBase<T>
{
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

    protected virtual bool HandleLeftArrow()
    {
        if (InputBuffer.IsStart)
        {
            return false;
        }

        InputBuffer.MoveBackward();

        return true;
    }

    protected virtual bool HandleCtrlLeftArrow()
    {
        if (InputBuffer.IsStart)
        {
            return false;
        }

        InputBuffer.MoveToPreviousWord();

        return true;
    }

    protected virtual bool HandleRightArrow()
    {
        if (InputBuffer.IsEnd)
        {
            return false;
        }

        InputBuffer.MoveForward();

        return true;
    }

    protected virtual bool HandleCtrlRightArrow()
    {
        if (InputBuffer.IsEnd)
        {
            return false;
        }

        InputBuffer.MoveToNextWord();

        return true;
    }

    protected virtual bool HandleHome()
    {
        if (InputBuffer.IsStart)
        {
            return false;
        }

        InputBuffer.MoveToStart();

        return true;
    }

    protected virtual bool HandleEnd()
    {
        if (InputBuffer.IsEnd)
        {
            return false;
        }

        InputBuffer.MoveToEnd();

        return true;
    }

    protected virtual bool HandleBackspace()
    {
        if (InputBuffer.IsStart)
        {
            return false;
        }

        InputBuffer.Backspace();

        return true;
    }

    protected virtual bool HandleCtrlBackspace()
    {
        if (InputBuffer.IsStart)
        {
            return false;
        }

        InputBuffer.BackspaceWord();

        return true;
    }

    protected virtual bool HandleDelete()
    {
        if (InputBuffer.IsEnd)
        {
            return false;
        }

        InputBuffer.Delete();

        return true;
    }

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
