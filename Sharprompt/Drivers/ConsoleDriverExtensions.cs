using System;

namespace Sharprompt.Drivers
{
    internal static class ConsoleDriverExtensions
    {
        public static void WriteMessage(this IConsoleDriver consoleDriver, string message)
        {
            consoleDriver.Write("?", ConsoleColor.Green);
            consoleDriver.Write($" {message}: ");
        }
    }
}
