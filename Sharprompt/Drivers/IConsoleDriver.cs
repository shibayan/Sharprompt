using System;

namespace Sharprompt.Drivers
{
    internal interface IConsoleDriver
    {
        void Close();
        void Reset();
        void Write(string value);
        void Write(string value, ConsoleColor color);
        void WriteLine();
        void WriteErrorMessage(string errorMessage);
    }
}
