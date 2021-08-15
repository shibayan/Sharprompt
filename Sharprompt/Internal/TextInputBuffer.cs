using System.Text;

namespace Sharprompt.Internal
{
    internal class TextInputBuffer
    {
        private readonly StringBuilder _inputBuffer = new();

        private int _position;

        public int Length => _inputBuffer.Length;
        public bool Eol => _position == _inputBuffer.Length;
        public bool Head => _position == 0;

        public void Insert(char value) => _inputBuffer.Insert(_position++, value);

        public void Back() => _position--;

        public void Next() => _position++;

        public void Backspace() => _inputBuffer.Remove(--_position, 1);

        public void Delete() => _inputBuffer.Remove(_position, 1);

        public void Clear()
        {
            _position = 0;
            _inputBuffer.Clear();
        }

        public string ToFrontString() => _inputBuffer.ToString(0, _position);

        public string ToBackString() => _inputBuffer.ToString(_position, _inputBuffer.Length - _position);

        public override string ToString() => _inputBuffer.ToString();
    }
}
