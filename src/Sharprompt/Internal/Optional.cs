namespace Sharprompt.Internal;

internal readonly struct Optional<T>
{
    public Optional(T value)
    {
        HasValue = true;
        Value = value;
    }

    public bool HasValue { get; } = false;

    public T Value { get; } = default!;

    public static readonly Optional<T> Empty = new();

    public static implicit operator T(Optional<T> optional) => optional.Value;

    public static Optional<T> Create(T? value) => value is null ? Empty : new Optional<T>(value);

    public static Optional<T> Create(object? value) => value is null ? Empty : new Optional<T>((T)value);
}
