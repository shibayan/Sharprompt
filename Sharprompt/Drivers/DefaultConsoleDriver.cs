using System;
using System.Text;

namespace Sharprompt.Drivers
{
    internal class DefaultConsoleDriver : IConsoleDriver
    {
        #region IDisposable

        public virtual void Dispose()
        {
            Reset();
        }

        #endregion

        #region IConsoleDriver

        public virtual void Beep() => Console.Beep();

        public void Reset()
        {
            Console.CursorVisible = true;
            Console.ResetColor();

            if (CursorLeft != 0)
            {
                Console.WriteLine();
            }
        }

        public virtual void ClearLine(int top)
        {
            Console.SetCursorPosition(0, top);

            Console.Write("\x1b[2K");

            Console.SetCursorPosition(0, top);
        }

        public virtual ConsoleKeyInfo ReadKey() => Console.ReadKey(true);

        public virtual string ReadLine()
        {
            int startIndex = 0;
            var inputBuffer = new StringBuilder();

            while (true)
            {
                var keyInfo = ReadKey();

                if (keyInfo.Key == ConsoleKey.Enter)
                {
                    break;
                }

                if (keyInfo.Key == ConsoleKey.LeftArrow)
                {
                    if (startIndex > 0)
                    {
                        startIndex -= 1;

                        Console.CursorLeft -= 1;
                    }
                    else
                    {
                        Beep();
                    }
                }
                else if (keyInfo.Key == ConsoleKey.RightArrow)
                {
                    if (startIndex < inputBuffer.Length)
                    {
                        startIndex += 1;

                        Console.CursorLeft += 1;
                    }
                    else
                    {
                        Beep();
                    }
                }
                else if (keyInfo.Key == ConsoleKey.Backspace)
                {
                    if (startIndex > 0)
                    {
                        startIndex -= 1;

                        inputBuffer.Remove(startIndex, 1);

                        Console.CursorLeft -= 1;

                        var (left, top) = GetCursorPosition();

                        for (int i = startIndex; i < inputBuffer.Length; i++)
                        {
                            Console.Write(inputBuffer[i]);
                        }

                        Console.Write(' ');

                        SetCursorPosition(left, top);
                    }
                    else
                    {
                        Beep();
                    }
                }
                else if (keyInfo.Key == ConsoleKey.Delete)
                {
                    if (startIndex < inputBuffer.Length)
                    {
                        inputBuffer.Remove(startIndex, 1);

                        var (left, top) = GetCursorPosition();

                        for (int i = startIndex; i < inputBuffer.Length; i++)
                        {
                            Console.Write(inputBuffer[i]);
                        }

                        Console.Write(' ');

                        SetCursorPosition(left, top);
                    }
                    else
                    {
                        Beep();
                    }
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    inputBuffer.Insert(startIndex, keyInfo.KeyChar);

                    startIndex += 1;

                    Console.Write(keyInfo.KeyChar);

                    var (left, top) = GetCursorPosition();

                    for (int i = startIndex; i < inputBuffer.Length; i++)
                    {
                        Console.Write(inputBuffer[i]);
                    }

                    SetCursorPosition(left, top);
                }
            }

            return inputBuffer.ToString();
        }

        public virtual int Write(string value)
        {
            var writtenLines = (CursorLeft + value.Length) / Console.BufferWidth;

            Console.Write(value);

            return writtenLines;
        }

        public virtual int Write(string value, ConsoleColor color)
        {
            var previousColor = Console.ForegroundColor;

            Console.ForegroundColor = color;

            var writtenLines = Write(value);

            Console.ForegroundColor = previousColor;

            return writtenLines;
        }

        public virtual int WriteLine()
        {
            Console.WriteLine();

            return 1;
        }

        public (int left, int top) GetCursorPosition()
        {
            return (Console.CursorLeft, Console.CursorTop);
        }

        public virtual void SetCursorPosition(int left, int top)
        {
            Console.SetCursorPosition(left, top);
        }

        public virtual bool CursorVisible
        {
            get => Console.CursorVisible;
            set => Console.CursorVisible = value;
        }

        public virtual int CursorLeft => Console.CursorLeft;

        public virtual int CursorTop => Console.CursorTop;

        #endregion
    }
}
