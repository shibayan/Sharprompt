using System.Globalization;
using System.Text;

namespace Sharprompt.Internal;

/// <summary>
/// Stores editable UTF-16 text and moves or deletes by Unicode text elements and whitespace-delimited words.
/// </summary>
internal class TextInputBuffer
{
    private readonly StringBuilder _inputBuffer = new();
    private int[] _textElementStarts = [];

    private int _position;
    private bool _isTextElementStartsDirty = true;

    /// <summary>
    /// The input length in UTF-16 code units rather than terminal columns or Unicode text elements.
    /// </summary>
    public int Length => _inputBuffer.Length;

    /// <summary>
    /// Whether the edit cursor is at the beginning of the input.
    /// </summary>
    public bool IsStart => _position == 0;

    /// <summary>
    /// Whether the edit cursor is at the end of the input.
    /// </summary>
    public bool IsEnd => _position == _inputBuffer.Length;

    /// <summary>
    /// Inserts a UTF-16 code unit at the cursor and advances the cursor past it.
    /// </summary>
    /// <param name="value">The UTF-16 code unit to insert at the cursor.</param>
    public void Insert(char value)
    {
        _inputBuffer.Insert(_position++, value);
        _isTextElementStartsDirty = true;
    }

    /// <summary>
    /// Removes the Unicode text element before the cursor and moves to its start.
    /// </summary>
    /// <remarks>Callers check IsStart before deleting.</remarks>
    public void Backspace()
    {
        var start = GetPreviousTextElementStart(_position);
        var count = _position - start;

        _position = start;
        _inputBuffer.Remove(_position, count);
        _isTextElementStartsDirty = true;
    }

    /// <summary>
    /// Removes the Unicode text element at the cursor without moving the cursor.
    /// </summary>
    /// <remarks>Callers check IsEnd before deleting.</remarks>
    public void Delete()
    {
        var end = GetNextTextElementEnd(_position);

        _inputBuffer.Remove(_position, end - _position);
        _isTextElementStartsDirty = true;
    }

    /// <summary>
    /// Removes preceding whitespace and the preceding word, moving the cursor to the deletion start.
    /// </summary>
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

    /// <summary>
    /// Removes the following word and its trailing whitespace without moving the cursor.
    /// </summary>
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

    /// <summary>
    /// Removes all input, resets the cursor, and clears cached text-element boundaries.
    /// </summary>
    public void Clear()
    {
        _position = 0;
        _inputBuffer.Clear();
        _textElementStarts = [];
        _isTextElementStartsDirty = false;
    }

    /// <summary>
    /// Moves to the start of the previous Unicode text element.
    /// </summary>
    public void MoveBackward()
    {
        _position = GetPreviousTextElementStart(_position);
    }

    /// <summary>
    /// Moves to the end of the next Unicode text element.
    /// </summary>
    public void MoveForward() => _position = GetNextTextElementEnd(_position);

    /// <summary>
    /// Skips preceding whitespace and moves to the beginning of the preceding word.
    /// </summary>
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

    /// <summary>
    /// Skips the following word and its trailing whitespace to reach the next word.
    /// </summary>
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

    /// <summary>
    /// Moves the edit cursor to the start without changing the input.
    /// </summary>
    public void MoveToStart() => _position = 0;

    /// <summary>
    /// Moves the edit cursor to the end without changing the input.
    /// </summary>
    public void MoveToEnd() => _position = _inputBuffer.Length;

    /// <summary>
    /// Returns the input before the edit cursor for rendering.
    /// </summary>
    /// <returns>The text before the edit cursor.</returns>
    public string ToBackwardString() => _inputBuffer.ToString(0, _position);

    /// <summary>
    /// Returns the input after the edit cursor for rendering.
    /// </summary>
    /// <returns>The text after the edit cursor.</returns>
    public string ToForwardString() => _inputBuffer.ToString(_position, _inputBuffer.Length - _position);

    /// <summary>
    /// Returns the complete input without changing the edit cursor.
    /// </summary>
    /// <returns>The complete input string.</returns>
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
