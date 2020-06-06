using System;

namespace Sharprompt.Drivers
{
    internal interface IConsoleDriver : IDisposable
    {
        void Beep();
        void Reset();
        ConsoleKeyInfo ReadKey();
        string ReadLine();
        int Write(string value);
        int Write(string value, ConsoleColor color);
        void WriteLine();
        void WriteErrorMessage(string errorMessage);
        bool CursorVisible { get; set; }
    }
}
