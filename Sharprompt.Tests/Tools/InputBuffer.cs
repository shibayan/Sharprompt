using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Sharprompt.Tests
{
    public class InputBuffer : ConcurrentQueue<ConsoleKeyInfo>
    {
        private Dictionary<char, ConsoleKeyInfo> _inputList = SetupKeyMapping();

        public void Write(string input)
        {
            foreach (var keyChar in input)
            {
                if (_inputList.TryGetValue(keyChar, out var keyInfo))
                {
                    Enqueue(keyInfo);
                }
                else
                {
                    throw new InvalidOperationException("Unknown character to key mapping");
                }
            }
        }

        public void WriteLine(string input)
        {
            Write(input);
            Write("\n");
        }

        private static Dictionary<char, ConsoleKeyInfo> SetupKeyMapping()
        {
            var keyMapping  = new Dictionary<char, ConsoleKeyInfo>()
            {
                {(char)3, new ConsoleKeyInfo((char)3, ConsoleKey.C, false, false, true)},
                {' ', new ConsoleKeyInfo(' ', ConsoleKey.Spacebar, false, false, false)},
                {'\n', new ConsoleKeyInfo('\n', ConsoleKey.Enter, false, false, false)},
                {'\b', new ConsoleKeyInfo('\b', ConsoleKey.Backspace, false, false, false)}
            };

            for (char c = 'a'; c < 'z'; c++)
            {
                keyMapping.Add(c, new ConsoleKeyInfo(c, (c - 'a') + ConsoleKey.A, false, false, false));
            }

            for (char c = 'A'; c < 'Z'; c++)
            {
                keyMapping.Add(c, new ConsoleKeyInfo(c, (c - 'Z') + ConsoleKey.A, true, false, false));
            }

            for (char c = '0'; c < '9'; c++)
            {
                keyMapping.Add(c, new ConsoleKeyInfo(c, (c - '0') + ConsoleKey.D0, false, false, false));
            }

            return keyMapping;
        }
    }
}
