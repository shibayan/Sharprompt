using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Sharprompt.Drivers;

namespace Sharprompt.Tests
{
    public class MockConsoleDriver : IConsoleDriver
    {
        private char[,] _buffer;
        public InputBuffer InputBuffer { get; } = new InputBuffer();

        public event EventHandler AwaitingKeyPress;

        /// <summary>
        /// Initializes a new instance of the <see cref="MockConsoleDriver" /> class.
        /// </summary>
        public MockConsoleDriver()
        {
            Reset();
        }

        public bool KeyAvailable => InputBuffer.Any();
        public bool CursorVisible { get; set; } = true;
        public int CursorLeft { get; set; } = 0;
        public int CursorTop { get; set; } = 0;
        public int BufferWidth { get; } = 120;
        public int BufferHeight { get; } = 30;
        public Action CancellationCallback { get; set; }

        public void Dispose()
        {
        }

        public void Beep()
        {
        }

        public void Reset()
        {
            _buffer = new char[BufferHeight, BufferWidth];
            CursorLeft = CursorTop = 0;
        }

        public void ClearLine(int top)
        {
            for (var x = 0; x < BufferWidth; x++)
            {
                _buffer[top, x] = ' ';
            }

            if (CursorTop == top) CursorLeft = 0;
        }

        public ConsoleKeyInfo ReadKey()
        {
            if (!KeyAvailable)
            {
                AwaitingKeyPress?.Invoke(this, EventArgs.Empty);
            }

            var consoleKey = new ConsoleKeyInfo();
            SpinWait.SpinUntil(() => KeyAvailable && InputBuffer.TryDequeue(out consoleKey), 1000);
            return consoleKey;
        }

        public void Write(string value, ConsoleColor color)
        {
            for (int x = 0; x < value.Length; x++, CursorLeft++)
            {
                _buffer[CursorTop, CursorLeft] = value[x];
            }
        }

        public void WriteLine()
        {
            Write("\n", ConsoleColor.Black);
            CursorLeft = 0;
            CursorTop++;
        }

        public void SetCursorPosition(int left, int top)
        {
            CursorLeft = left;
            CursorTop = top;
        }

        public string GetOutputBuffer()
        {
            List<string> lines = new List<string>();
            for (int y = 0; y < BufferHeight; y++)
            {
                var stringBuilder = new StringBuilder();
                for (int x = 0; x < BufferWidth && _buffer[y,x] != '\0'; x++)
                {
                    stringBuilder.Append(_buffer[y,x]);
                }

                lines.Add(stringBuilder.ToString().TrimEnd());
            }

            var idx = lines.FindLastIndex((l) => !string.IsNullOrEmpty(l));

            return string.Join('\n', lines.Take(idx + 1));
        }
    }
}
