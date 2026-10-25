using System;

namespace Sharprompt.Internal;

/// <summary>
/// Identifies a key and modifier combination for prompt key-handler lookup.
/// </summary>
/// <param name="key">The console key to match.</param>
/// <param name="modifiers">The modifiers required with the key.</param>
internal readonly struct ConsoleKeyBinding(ConsoleKey key, ConsoleModifiers modifiers = default) : IEquatable<ConsoleKeyBinding>
{
    /// <summary>
    /// The console key used to match a handler.
    /// </summary>
    public ConsoleKey Key { get; } = key;

    /// <summary>
    /// The modifiers that must accompany the console key.
    /// </summary>
    public ConsoleModifiers Modifiers { get; } = modifiers;

    /// <summary>
    /// Compares the console key and modifiers for equality.
    /// </summary>
    /// <param name="other">The key binding to compare.</param>
    /// <returns>True if both bindings contain the same key and modifiers; otherwise false.</returns>
    public bool Equals(ConsoleKeyBinding other) => Key == other.Key && Modifiers == other.Modifiers;

    /// <summary>
    /// Compares the console key and modifiers for equality.
    /// </summary>
    /// <param name="obj">The object to compare with this key binding.</param>
    /// <returns>True if both bindings contain the same key and modifiers; otherwise false.</returns>
    public override bool Equals(object? obj) => obj is ConsoleKeyBinding other && Equals(other);

    /// <summary>
    /// Combines the console key and modifiers into a dictionary hash code.
    /// </summary>
    /// <returns>A hash code derived from the key and modifiers.</returns>
    public override int GetHashCode() => HashCode.Combine(Key, Modifiers);

    /// <summary>
    /// Tests whether two bindings have the same key and modifiers.
    /// </summary>
    /// <param name="left">The left key binding.</param>
    /// <param name="right">The right key binding.</param>
    /// <returns>True if both bindings are equal; otherwise false.</returns>
    public static bool operator ==(ConsoleKeyBinding left, ConsoleKeyBinding right) => left.Equals(right);

    /// <summary>
    /// Tests whether two bindings differ in key or modifiers.
    /// </summary>
    /// <param name="left">The left key binding.</param>
    /// <param name="right">The right key binding.</param>
    /// <returns>True if the bindings differ; otherwise false.</returns>
    public static bool operator !=(ConsoleKeyBinding left, ConsoleKeyBinding right) => !left.Equals(right);
}
