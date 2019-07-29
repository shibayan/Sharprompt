using System;

namespace Sharprompt
{
    internal class ConsoleScope : IDisposable
    {
        public ConsoleScope(bool cursorVisible)
        {
            _cursorVisible = cursorVisible;
        }

        private readonly int _initialLeft = Console.CursorLeft;
        private readonly int _initialTop = Console.CursorTop;

        private readonly bool _cursorVisible;

        private int _cursorBottom = Console.CursorTop + 1;

        public void Dispose()
        {
            Console.CursorLeft = 0;
            Console.CursorTop = _cursorBottom;
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

            _cursorBottom = Console.CursorTop + 1;

            if (_cursorVisible)
            {
                Console.CursorVisible = true;
            }
        }

        private void Rewind()
        {
            Console.CursorLeft = _initialLeft;
            Console.CursorTop = _initialTop;
        }

        private void Clear()
        {
            var space = new string(' ', Console.WindowWidth);

            for (int i = _initialTop; i < _cursorBottom; i++)
            {
                Console.CursorLeft = 0;
                Console.CursorTop = i;

                Console.Write(space);
            }

            Rewind();
        }
    }
}