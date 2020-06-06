using System;

namespace Sharprompt.Drivers
{
    internal interface IConsoleDriver : IDisposable
    {
        void Beep();
        void Clear();
        ConsoleKeyInfo ReadKey();
        string ReadLine();
        int Write(string value);
        int Write(string value, ConsoleColor color);
        void WriteLine();
        void WriteErrorMessage(string errorMessage);
        void EraseLine(int y);
        bool CursorVisible { get; set; }
    }
}
