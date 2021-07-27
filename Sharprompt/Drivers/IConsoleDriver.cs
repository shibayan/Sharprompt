﻿using System;
using System.Threading;

namespace Sharprompt.Drivers
{
    internal interface IConsoleDriver : IDisposable
    {
        int IdleReadKey { get; set; }
        ConsoleKeyInfo WaitKeypress(CancellationToken cancellationToken);
        void Beep();
        void Reset();
        void ClearLine(int top);
        ConsoleKeyInfo ReadKey();
        void Write(string value, ConsoleColor color);
        void WriteLine();
        (int left, int top) GetCursorPosition();
        void SetCursorPosition(int left, int top);
        bool KeyAvailable { get; }
        bool CursorVisible { get; set; }
        int CursorLeft { get; }
        int CursorTop { get; }
        int BufferWidth { get; }
        int BufferHeight { get; }
    }
}
