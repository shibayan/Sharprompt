using System;

namespace Sharprompt.Drivers;

internal interface IConsoleDriver : IDisposable
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
}
