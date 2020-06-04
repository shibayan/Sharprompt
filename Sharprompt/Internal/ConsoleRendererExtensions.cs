using System;

namespace Sharprompt.Internal
{
    internal static class ConsoleRendererExtensions
    {
        public static void WriteMessage(this DefaultConsoleRenderer consoleRenderer, string message)
        {
            consoleRenderer.Write("?", ConsoleColor.Green);
            consoleRenderer.Write($" {message}: ");
        }
    }
}
