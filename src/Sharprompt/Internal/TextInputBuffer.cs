using System.Globalization;
using System.Text;

namespace Sharprompt.Internal;

internal class TextInputBuffer
{
    private readonly StringBuilder _inputBuffer = new();
    private int[] _textElementStarts = [];

    private int _position;
    private bool _isTextElementStartsDirty = true;

    public int Length => _inputBuffer.Length;

    public bool IsStart => _position == 0;

    public bool IsEnd => _position == _inputBuffer.Length;

    public void Insert(char value)
    {
        _inputBuffer.Insert(_position++, value);
        _isTextElementStartsDirty = true;
    }

    public void Backspace()
    {
        var start = GetPreviousTextElementStart(_position);
        var count = _position - start;

        _position = start;
        _inputBuffer.Remove(_position, count);
        _isTextElementStartsDirty = true;
    }

    public void Delete()
    {
        var end = GetNextTextElementEnd(_position);

        _inputBuffer.Remove(_position, end - _position);
        _isTextElementStartsDirty = true;
    }

    public void BackspaceWord()
    {
        var count = 0;

        while (_position > 0)
        {
            var start = GetPreviousTextElementStart(_position);

            if (!IsWhiteSpace(start, _position - start))
            {
                break;
            }

            count += _position - start;
            _position = start;
        }

        while (_position > 0)
        {
            var start = GetPreviousTextElementStart(_position);

            if (IsWhiteSpace(start, _position - start))
            {
                break;
            }

            count += _position - start;
            _position = start;
        }

        _inputBuffer.Remove(_position, count);
        _isTextElementStartsDirty = true;
    }

    public void DeleteWord()
    {
        var count = 0;

        while (_position + count < _inputBuffer.Length)
        {
            var start = _position + count;
            var end = GetNextTextElementEnd(start);

            if (IsWhiteSpace(start, end - start))
            {
                break;
            }

            count += end - start;
        }

        while (_position + count < _inputBuffer.Length)
        {
            var start = _position + count;
            var end = GetNextTextElementEnd(start);

            if (!IsWhiteSpace(start, end - start))
            {
                break;
            }

            count += end - start;
        }

        _inputBuffer.Remove(_position, count);
        _isTextElementStartsDirty = true;
    }

    public void Clear()
    {
        _position = 0;
        _inputBuffer.Clear();
        _textElementStarts = [];
        _isTextElementStartsDirty = false;
    }

    public void MoveBackward()
    {
        _position = GetPreviousTextElementStart(_position);
    }

    public void MoveForward() => _position = GetNextTextElementEnd(_position);

    public void MoveToPreviousWord()
    {
        while (_position > 0)
        {
            var start = GetPreviousTextElementStart(_position);

            if (!IsWhiteSpace(start, _position - start))
            {
                break;
            }

            _position = start;
        }

        while (_position > 0)
        {
            var start = GetPreviousTextElementStart(_position);

            if (IsWhiteSpace(start, _position - start))
            {
                break;
            }

            _position = start;
        }
    }

    public void MoveToNextWord()
    {
        while (_position < _inputBuffer.Length)
        {
            var end = GetNextTextElementEnd(_position);

            if (IsWhiteSpace(_position, end - _position))
            {
                break;
            }

            _position = end;
        }

        while (_position < _inputBuffer.Length)
        {
            var end = GetNextTextElementEnd(_position);

            if (!IsWhiteSpace(_position, end - _position))
            {
                break;
            }

            _position = end;
        }
    }

    public void MoveToStart() => _position = 0;

    public void MoveToEnd() => _position = _inputBuffer.Length;

    public string ToBackwardString() => _inputBuffer.ToString(0, _position);

    public string ToForwardString() => _inputBuffer.ToString(_position, _inputBuffer.Length - _position);

    public override string ToString() => _inputBuffer.ToString();

    private int GetPreviousTextElementStart(int position)
    {
        var indices = GetTextElementStarts();
        var index = System.Array.BinarySearch(indices, position);

        if (index >= 0)
        {
            return index > 0 ? indices[index - 1] : 0;
        }

        index = ~index;

        return index > 0 ? indices[index - 1] : 0;
    }

    private int GetNextTextElementEnd(int position)
    {
        var indices = GetTextElementStarts();
        var index = System.Array.BinarySearch(indices, position);

        if (index >= 0)
        {
            return index + 1 < indices.Length ? indices[index + 1] : _inputBuffer.Length;
        }

        index = ~index;

        return index < indices.Length ? indices[index] : _inputBuffer.Length;
    }

    private bool IsWhiteSpace(int start, int count)
    {
        for (var i = start; i < start + count; i++)
        {
            if (!char.IsWhiteSpace(_inputBuffer[i]))
            {
                return false;
            }
        }

        return true;
    }

    private int[] GetTextElementStarts()
    {
        if (_isTextElementStartsDirty)
        {
            _textElementStarts = StringInfo.ParseCombiningCharacters(_inputBuffer.ToString());
            _isTextElementStartsDirty = false;
        }

        return _textElementStarts;
    }
}
