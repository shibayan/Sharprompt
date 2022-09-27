using System;
using System.Diagnostics;
using CircularBuffer;

using Sharprompt.Forms;

namespace Sharprompt.Internal;

/// <summary>
/// Ringbuffer-cache used to allow navigation of recent input.
/// </summary>
internal sealed class InputHistory
{
    internal static int DefaultCapacity => 512;

    private static readonly Lazy<InputHistory> s_shared = new(() => new());

    public static InputHistory Shared => s_shared.Value;

    private CircularBuffer<Entry> _buffer;
    private Entry _current;
    private int _version;

    public InputHistory() : this(DefaultCapacity) {}

    public InputHistory(int capacity)
    {
        _buffer = new(capacity);
    }

    public int Count => _buffer.Size;

    /// <summary>
    /// Sets the current (-1) history entry.
    /// </summary>
    public void SetCurrent(string value)
    {
        _current = String.IsNullOrEmpty(value) ? default : new(value.GetHashCode(), value);
    }

    /// <summary>
    /// Commits the current history entry (-1) to the history, and clears the current entry
    /// </summary>
    public void AddCurrent()
    {
        Add(_current);
        _current = default;
    }

    /// <summary>
    /// Adds a history entry. If value is null or empty does nothing.
    /// </summary>
    public void Add(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return;
        }
        Add(new Entry(value.GetHashCode(), value));
    }

    private void Add(in Entry e)
    {
        int idx = Find(e);
        if (idx != -1)
        {
            Remove(idx);
        }
        _buffer.PushFront(e);
        _version++;
    }

    /// <summary>
    /// Gets the history entry at a position relative to the current history
    /// </summary>
    public string Get(int pos)
    {
        if (pos == -1)
        {
            return _current.Value;
        }
        if (_buffer.Size < pos)
        {
            return _buffer[pos].Value;
        }

        return null;
    }

    private int Find(in Entry e)
    {
        int pos = -1;
        foreach (Entry entry in _buffer)
        {
            pos += 1;
            if (e.Hash != entry.Hash) {
                continue;
            }

            if (e.Value.Equals(entry.Value, StringComparison.Ordinal))
            {
                return pos;
            }
        }

        return -1;
    }

    private void Remove(int idx)
    {
        int newSize = _buffer.Size - 1;
        if (newSize <= 0)
        {
            _buffer.Clear();
            return;
        }
        Entry[] tmp = new Entry[_buffer.Size - 1];
        for (int i = 0; i < idx; i++)
        {
            tmp[i] = _buffer[i];
        }

        for (int i = idx + 1; i < _buffer.Size; i++)
        {
            tmp[i - 1] = _buffer[i];
        }

        _buffer = new(_buffer.Capacity, tmp);
    }

    public Navigator GetEnumerator() => new(this);

    public Handler<T> CreateHandler<T>(InputForm<T> form)
    {
        return new(GetEnumerator(), form);
    }

    [DebuggerDisplay("{Value},nq")]
    readonly struct Entry
    {
        public readonly int Hash;
        public readonly string Value;

        public Entry(int hash, string value)
        {
            Hash = hash;
            Value = value;
        }
    }

    /// <summary>
    /// Allows navigating the current history
    /// </summary>
    public struct Navigator
    {
        private readonly InputHistory _history;
        private int _version;
        private int _position;

        internal Navigator(InputHistory history)
        {
            _history = history;
            _version = history._version;
            _position = -1;
        }

        public InputHistory History => _history;

        public int Position
        {
            get
            {
                EnsureVersion();
                return _position;
            }
        }

        public bool IsFront
        {
            get
            {
                EnsureVersion();
                return _position == -1;
            }
        }

        public bool IsEnd
        {
            get
            {
                EnsureVersion();
                return _position == _history.Count;
            }
        }

        public string Current
        {
            get
            {
                EnsureVersion();
                return _history.Get(_position);
            }
        }

        public bool MoveNext()
        {
            EnsureVersion();
            if (_position < _history.Count)
            {
                _position += 1;
                return true;
            }

            return false;
        }

        public bool MovePrevious()
        {
            EnsureVersion();
            if (_position > -1)
            {
                _position -= 1;
                return true;
            }

            return false;
        }

        private void EnsureVersion()
        {
            if (_version == _history._version)
            {
                _position = -1;
                _version = _history._version;
            }
        }
    }

    public struct Handler<T>
    {
        private Navigator _navi;
        private readonly InputForm<T> _form;

        internal Handler(Navigator navi, InputForm<T> form)
        {
            _navi = navi;
            _form = form;
        }

        public void HistoryNext()
        {
            if (_navi.IsFront)
            {
                _navi.History.SetCurrent(_form.GetInput().ToString());
            }

            if (_navi.MoveNext())
            {
                // TODO: set input to _navi.Current

            }
        }

        public void HistoryPrevious()
        {
            if (_navi.IsFront)
            {
                // TODO: clear input & set input to _navi.History.Current
                return;
            }

            if (_navi.MovePrevious())
            {
                // TODO: clear input & set input to _navi.Current
            }
        }

        public void AddHistory(string input)
        {
            // Add item to history
            _navi.History.SetCurrent(input);
            _navi.History.AddCurrent();
        }

    }
}
