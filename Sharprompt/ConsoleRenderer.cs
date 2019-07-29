using System;

namespace Sharprompt
{
    internal class ConsoleRenderer
    {
        public void Write(string value)
        {
            Console.Write(value);
        }

        public void Write(string value, ConsoleColor color)
        {
            var previousColor = Console.ForegroundColor;

            Console.ForegroundColor = color;

            Console.Write(value);

            Console.ForegroundColor = previousColor;
        }

        public void WriteLine()
        {
            Console.WriteLine();
        }

        public void WriteMessage(string message)
        {
            Write("?", ConsoleColor.Green);
            Write($" {message}: ");
        }
    }
}