using System;

namespace Sharprompt.Drivers
{
    internal interface IConsoleDriver : IDisposable
    {
        void Beep();
        void Reset();
        void ClearLine(int top);
        ConsoleKeyInfo ReadKey();
        string ReadLine();
        void Write(string value, ConsoleColor color);
        void WriteLine();
        (int left, int top) GetCursorPosition();
        void SetCursorPosition(int left, int top);
        bool CursorVisible { get; set; }
        int CursorLeft { get; }
        int CursorTop { get; }
        int BufferWidth { get; }
        int BufferHeight { get; }
    }
}
