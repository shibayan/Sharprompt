using System;
using System.Collections.Generic;
using System.Linq;

using Sharprompt.Drivers;

namespace Sharprompt.Internal
{
    internal class OffscreenBuffer : IDisposable
    {
        public OffscreenBuffer(IConsoleDriver consoleDriver)
        {
            _consoleDriver = consoleDriver;

            _cursorBottom = _consoleDriver.CursorTop;
            _consoleDriver.RequestCancellation = RequestCancellation;
        }

        private readonly IConsoleDriver _consoleDriver;
        private readonly List<List<TextInfo>> _outputBuffer = new() { new List<TextInfo>() };

        private int _cursorBottom;
        private Cursor _pushedCursor;

        public int WrittenLineCount => _outputBuffer.Sum(x => (x.Sum(xs => xs.Width) - 1) / _consoleDriver.BufferWidth + 1) - 1;

        public void Dispose() => _consoleDriver.Dispose();

        public void Write(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            _outputBuffer.Last().Add(new TextInfo(text, Console.ForegroundColor));
        }

        public void Write(string text, ConsoleColor color)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            _outputBuffer.Last().Add(new TextInfo(text, color));
        }

        public void WriteLine()
        {
            _outputBuffer.Add(new List<TextInfo>());
        }

        public void WritePrompt(string message)
        {
            Write(Prompt.Symbols.Prompt, ConsoleColor.Green);
            Write($" {message}: ");
        }

        public void WriteFinish(string message)
        {
            Write(Prompt.Symbols.Done, ConsoleColor.Green);
            Write($" {message}: ");
        }

        public void WriteErrorMessage(string errorMessage)
        {
            WriteLine();
            Write($"{Prompt.Symbols.Error} {errorMessage}", ConsoleColor.Red);
        }

        public void PushCursor()
        {
            if (_pushedCursor != null)
            {
                return;
            }

            _pushedCursor = new Cursor
            {
                Left = _outputBuffer.Last().Sum(x => x.Width),
                Top = _outputBuffer.Count - 1
            };
        }

        public IDisposable BeginRender()
        {
            return new RenderScope(this, _consoleDriver, _cursorBottom, WrittenLineCount);
        }

        public void RenderToConsole()
        {
            for (var i = 0; i < _outputBuffer.Count; i++)
            {
                var lineBuffer = _outputBuffer[i];

                foreach (var textInfo in lineBuffer)
                {
                    _consoleDriver.Write(textInfo.Text, textInfo.Color);
                }

                if (i < _outputBuffer.Count - 1)
                {
                    _consoleDriver.WriteLine();
                }
            }

            _cursorBottom = _consoleDriver.CursorTop;

            if (_pushedCursor != null)
            {
                var physicalLeft = _pushedCursor.Left % _consoleDriver.BufferWidth;
                var physicalTop = _pushedCursor.Top + (_pushedCursor.Left / _consoleDriver.BufferWidth);

                _consoleDriver.SetCursorPosition(physicalLeft, _cursorBottom - WrittenLineCount + physicalTop);
            }
        }

        public void ClearConsole(int cursorBottom, int writtenLineCount)
        {
            for (var i = 0; i <= writtenLineCount; i++)
            {
                _consoleDriver.ClearLine(cursorBottom - i);
            }
        }

        public void ClearBuffer()
        {
            _outputBuffer.Clear();
            _outputBuffer.Add(new List<TextInfo>());

            _pushedCursor = null;
        }

        private void RequestCancellation()
        {
            _consoleDriver.SetCursorPosition(0, _cursorBottom);
            _consoleDriver.Reset();

            Environment.Exit(1);
        }

        private class TextInfo
        {
            public TextInfo(string text, ConsoleColor color)
            {
                Text = text;
                Color = color;
                Width = text.GetWidth();
            }

            public string Text { get; }
            public ConsoleColor Color { get; }
            public int Width { get; }
        }
    }
}
