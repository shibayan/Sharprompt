using System;
using System.Collections.Generic;
using System.Linq;

using Sharprompt.Drivers;

namespace Sharprompt.Internal
{
    internal class ScreenBuffer
    {
        public ScreenBuffer(IConsoleDriver consoleDriver)
        {
            _consoleDriver = consoleDriver;
        }

        private readonly IConsoleDriver _consoleDriver;
        private readonly List<List<TextInfo>> _outputBuffer = new List<List<TextInfo>>();

        private int _cursorLeft;
        private int _cursorTop;

        public int CursorBottom { get; set; }

        public int BufferWidth => _consoleDriver.BufferWidth;

        public int LineCount => _outputBuffer.Count + _outputBuffer.Sum(x => (x.Sum(xs => EastAsianWidth.GetWidth(xs.Text)) - 1) / _consoleDriver.BufferWidth);

        public void Clear()
        {
            _outputBuffer.Clear();
            _outputBuffer.Add(new List<TextInfo>());

            _cursorLeft = 0;
            _cursorTop = 0;
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

        public void WriteMessage(string message)
        {
            Write(Symbol.Prompt, ConsoleColor.Green);
            Write($" {message}: ");
        }

        public void WriteFinishMessage(string message)
        {
            Write(Symbol.Done, ConsoleColor.Green);
            Write($" {message}: ");
        }

        public void WriteErrorMessage(string errorMessage)
        {
            WriteLine();
            Write($"{Symbol.Error} {errorMessage}", ConsoleColor.Red);
        }

        public (int left, int top) GetCursorPosition()
        {
            var left = _outputBuffer.Last().Sum(x => EastAsianWidth.GetWidth(x.Text)) % _consoleDriver.BufferWidth;
            var top = LineCount - 1;

            return (left, top);
        }

        public void SetCursorPosition()
        {
            var (left, top) = GetCursorPosition();

            SetCursorPosition(left, top);
        }

        public void SetCursorPosition(int left, int top)
        {
            _cursorLeft = left;
            _cursorTop = top;
        }

        public void RenderToConsole()
        {
            for (int i = 0; i < _outputBuffer.Count; i++)
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

            CursorBottom = _consoleDriver.CursorTop;

            _consoleDriver.SetCursorPosition(_cursorLeft, _consoleDriver.CursorTop - (LineCount - _cursorTop - 1));
        }

        private class TextInfo
        {
            public TextInfo(string text, ConsoleColor color)
            {
                Text = text;
                Color = color;
            }

            public string Text { get; }
            public ConsoleColor Color { get; }
        }
    }
}
