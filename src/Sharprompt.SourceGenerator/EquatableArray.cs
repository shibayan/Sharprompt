using System;
using System.Collections;
using System.Collections.Generic;

namespace Sharprompt.SourceGenerator;

/// <summary>
/// An immutable array wrapper with structural equality, so that values flowing through
/// the incremental pipeline can be compared and cached.
/// </summary>
internal readonly struct EquatableArray<T> : IEquatable<EquatableArray<T>>, IEnumerable<T> where T : IEquatable<T>
{
    private readonly T[]? _array;

    public EquatableArray(T[] array)
    {
        _array = array;
    }

    public int Length => _array?.Length ?? 0;

    public bool Equals(EquatableArray<T> other)
    {
        var left = _array ?? [];
        var right = other._array ?? [];

        if (left.Length != right.Length)
        {
            return false;
        }

        for (var i = 0; i < left.Length; i++)
        {
            if (!EqualityComparer<T>.Default.Equals(left[i], right[i]))
            {
                return false;
            }
        }

        return true;
    }

    public override bool Equals(object? obj) => obj is EquatableArray<T> other && Equals(other);

    public override int GetHashCode()
    {
        var hash = 17;

        foreach (var item in _array ?? [])
        {
            hash = (hash * 31) + (item?.GetHashCode() ?? 0);
        }

        return hash;
    }

    public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)(_array ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
