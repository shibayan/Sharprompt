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
        int Write(string value);
        int Write(string value, ConsoleColor color);
        int WriteLine();
        (int left, int top) GetCursorPosition();
        void SetCursorPosition(int left, int top);
        bool CursorVisible { get; set; }
        int CursorLeft { get; }
        int CursorTop { get; }
        int BufferWidth { get; }
        int BufferHeight { get; }
    }
}
