using System;
using System.Collections.Generic;
using System.Linq;

using Sharprompt.Drivers;

namespace Sharprompt.Internal
{
    internal class OffscreenBuffer
    {
        public OffscreenBuffer(IConsoleDriver consoleDriver)
        {
            _consoleDriver = consoleDriver;

            _consoleDriver.RequestCancellation = RequestCancellation;
        }

        private readonly IConsoleDriver _consoleDriver;
        private readonly List<List<TextInfo>> _outputBuffer = new();

        private int _cursorBottom;
        private Cursor _savedCursor;

        public void Clear()
        {
            _outputBuffer.Clear();
            _outputBuffer.Add(new List<TextInfo>());

            _savedCursor = null;
        }

        public void Write(string text)
        {
            _outputBuffer.Last().Add(new TextInfo(text, Console.ForegroundColor));
        }

        public void Write(string text, ConsoleColor color)
        {
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
            if (_savedCursor != null)
            {
                return;
            }

            _savedCursor = new Cursor
            {
                Left = _outputBuffer.Last().Sum(x => x.Width),
                Top = _outputBuffer.Count
            };
        }

        public void RenderToConsole()
        {
            var cursorTop = _consoleDriver.CursorTop;

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

            if (_savedCursor != null)
            {
                _consoleDriver.SetCursorPosition(_savedCursor.Left, cursorTop + _savedCursor.Top - 1);
            }
        }

        public void ClearConsole()
        {
            var lineCount = _outputBuffer.Count + _outputBuffer.Sum(x => (x.Sum(xs => xs.Width) - 1) / _consoleDriver.BufferWidth);

            for (var i = 0; i < lineCount; i++)
            {
                _consoleDriver.ClearLine(_cursorBottom - i);
            }

            Clear();
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
