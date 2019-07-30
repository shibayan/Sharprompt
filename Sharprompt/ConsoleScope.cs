using System;

namespace Sharprompt
{
    internal class ConsoleScope : IDisposable
    {
        public ConsoleScope(bool cursorVisible)
        {
            _cursorVisible = cursorVisible;
        }

        private readonly int _initialTop = Console.CursorTop;

        private readonly bool _cursorVisible;

        public void Dispose()
        {
            if (Console.CursorLeft != 0)
            {
                Console.WriteLine();
            }

            Console.ResetColor();
        }

        public void Beep()
        {
            Console.Beep();
        }

        public ConsoleKeyInfo ReadKey()
        {
            return Console.ReadKey(true);
        }

        public string ReadLine()
        {
            return Console.ReadLine();
        }

        public void Render(Action<ConsoleRenderer> template)
        {
            Console.CursorVisible = false;

            Clear();

            template(new ConsoleRenderer());

            if (_cursorVisible)
            {
                Console.CursorVisible = true;
            }
        }

        private void Clear()
        {
            var space = new string(' ', Console.WindowWidth - 1);

            for (int top = Console.CursorTop; top >= _initialTop; top--)
            {
                Console.SetCursorPosition(0, top);

                Console.Write(space);
            }

            Console.SetCursorPosition(0, _initialTop);
        }
    }
}