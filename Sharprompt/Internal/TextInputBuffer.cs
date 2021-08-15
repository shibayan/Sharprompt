using System.Text;

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

        public void Backward() => _position--;

        public void Forward() => _position++;

        public void Backspace() => _inputBuffer.Remove(--_position, 1);

        public void Delete() => _inputBuffer.Remove(_position, 1);

        public void Clear()
        {
            _position = 0;
            _inputBuffer.Clear();
        }

        public string ToBackwardString() => _inputBuffer.ToString(0, _position);

        public string ToForwardString() => _inputBuffer.ToString(_position, _inputBuffer.Length - _position);

        public override string ToString() => _inputBuffer.ToString();
    }
}
