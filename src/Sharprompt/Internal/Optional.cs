using System;
using System.Globalization;

using Sharprompt.Strings;

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

    public static Optional<T> Create(object? value, string? paramName = null)
    {
        if (value is null)
        {
            return Empty;
        }

        if (value is T typedValue)
        {
            return new Optional<T>(typedValue);
        }

        var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

        try
        {
            // Accept compatible values such as an int literal for a long prompt,
            // since DefaultValue is typed as object and the exact type is easy to miss.
            if (targetType.IsEnum)
            {
                if (value is not Enum && Type.GetTypeCode(value.GetType()) is >= TypeCode.SByte and <= TypeCode.UInt64)
                {
                    return new Optional<T>((T)Enum.ToObject(targetType, value));
                }
            }
            else if (value is IConvertible && typeof(IConvertible).IsAssignableFrom(targetType))
            {
                return new Optional<T>((T)Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture));
            }
        }
        catch (Exception ex) when (ex is InvalidCastException or FormatException or OverflowException)
        {
            throw new ArgumentException(string.Format(Resource.Validation_Type_Incompatible, value.GetType(), typeof(T)), paramName, ex);
        }

        throw new ArgumentException(string.Format(Resource.Validation_Type_Incompatible, value.GetType(), typeof(T)), paramName);
    }
}
