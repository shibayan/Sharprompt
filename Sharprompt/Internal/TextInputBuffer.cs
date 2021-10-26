﻿using System.Text;

namespace Sharprompt.Internal
{
    internal class TextInputBuffer
    {
        private readonly StringBuilder _inputBuffer = new();

        private int _position;

        public int Length => _inputBuffer.Length;

        public bool IsStart => _position == 0;

        public bool IsEnd => _position == _inputBuffer.Length;

        public void Insert(char value) => _inputBuffer.Insert(_position++, value);

        public void Backspace()
        {
            var count = 1;

            if (char.IsLowSurrogate(_inputBuffer[--_position]))
            {
                count++;
                _position--;
            }

            _inputBuffer.Remove(_position, count);
        }

        public void Delete()
        {
            var count = 1;

            if (char.IsHighSurrogate(_inputBuffer[_position]))
            {
                count++;
            }

            _inputBuffer.Remove(_position, count);
        }

        public void Clear()
        {
            _position = 0;
            _inputBuffer.Clear();
        }

        public void MoveBackward()
        {
            if (char.IsLowSurrogate(_inputBuffer[--_position]))
            {
                _position--;
            }
        }

        public void MoveForward()
        {
            if (char.IsHighSurrogate(_inputBuffer[_position++]))
            {
                _position++;
            }
        }

        public string ToBackwardString() => _inputBuffer.ToString(0, _position);

        public string ToForwardString() => _inputBuffer.ToString(_position, _inputBuffer.Length - _position);

        public override string ToString() => _inputBuffer.ToString();
    }
}
