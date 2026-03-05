using System;

namespace Sharprompt.Internal;

internal readonly struct ConsoleKeyBinding(ConsoleKey key, ConsoleModifiers modifiers = default) : IEquatable<ConsoleKeyBinding>
{
    public ConsoleKey Key { get; } = key;

    public ConsoleModifiers Modifiers { get; } = modifiers;

    public bool Equals(ConsoleKeyBinding other) => Key == other.Key && Modifiers == other.Modifiers;

    public override bool Equals(object? obj) => obj is ConsoleKeyBinding other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Key, Modifiers);

    public static bool operator ==(ConsoleKeyBinding left, ConsoleKeyBinding right) => left.Equals(right);

    public static bool operator !=(ConsoleKeyBinding left, ConsoleKeyBinding right) => !left.Equals(right);
}
