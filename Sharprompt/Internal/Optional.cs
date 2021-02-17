namespace Sharprompt.Internal
{
    internal readonly struct Optional<T>
    {
        public Optional(T value)
        {
            HasValue = true;
            Value = value;
        }

        public bool HasValue { get; }

        public T Value { get; }

        public static implicit operator T(Optional<T> optional) => optional.Value;

        public static readonly Optional<T> Empty = new Optional<T>();

        public static Optional<T> Create(T value)
        {
            return value == null ? Empty : new Optional<T>(value);
        }

        public static Optional<T> Create(object value)
        {
            return value == null ? Empty : new Optional<T>((T)value);
        }
    }
}
