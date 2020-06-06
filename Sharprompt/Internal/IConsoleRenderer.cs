using System;

namespace Sharprompt.Internal
{
    internal interface IConsoleRenderer
    {
        void Close();
        void Reset();
        void Write(string value);
        void Write(string value, ConsoleColor color);
        void WriteLine();
        void WriteErrorMessage(string errorMessage);
    }
}
