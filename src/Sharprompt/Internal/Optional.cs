using System;
using System.Globalization;

using Sharprompt.Strings;

namespace Sharprompt.Internal;

/// <summary>
/// Distinguishes an omitted default value from a supplied value, including the default value of a value type.
/// </summary>
/// <typeparam name="T">The stored, converted, or selected value type.</typeparam>
internal readonly struct Optional<T>
{
    /// <summary>
    /// Stores a supplied value and marks the optional value as present.
    /// </summary>
    /// <param name="value">The value to store; this constructor marks even a null value as present.</param>
    public Optional(T value)
    {
        HasValue = true;
        Value = value;
    }

    /// <summary>
    /// Whether a value was supplied, independently of whether it equals the default value of its type.
    /// </summary>
    public bool HasValue { get; } = false;

    /// <summary>
    /// The supplied value, or the type default when no value is present.
    /// </summary>
    /// <remarks>Check HasValue when the presence of a default matters.</remarks>
    public T Value { get; } = default!;

    /// <summary>
    /// An optional value with no supplied default.
    /// </summary>
    public static readonly Optional<T> Empty = new();

    /// <summary>
    /// Returns the stored value or the type default without requiring a presence check.
    /// </summary>
    /// <param name="optional">The optional value to unwrap.</param>
    /// <returns>The stored value, or the type default when no value was supplied.</returns>
    public static implicit operator T(Optional<T> optional) => optional.Value;

    /// <summary>
    /// Creates a present optional value for non-null input, or an empty optional for null.
    /// </summary>
    /// <param name="value">The supplied value; null is treated as absent by the factory methods.</param>
    /// <returns>A present optional value for compatible non-null input, or an empty optional for null.</returns>
    public static Optional<T> Create(T? value) => value is null ? Empty : new Optional<T>(value);

    /// <summary>
    /// Creates an optional default from an object, allowing compatible invariant-culture primitive and integral enum conversions.
    /// </summary>
    /// <param name="value">The supplied value; null is treated as absent by the factory methods.</param>
    /// <param name="paramName">The argument name attached to an incompatible-value exception.</param>
    /// <returns>A present optional value for compatible non-null input, or an empty optional for null.</returns>
    /// <exception cref="ArgumentException">The supplied value cannot be represented by the target type.</exception>
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
                if (value is sbyte or byte or short or ushort or int or uint or long or ulong)
                {
                    // Enum.ToObject silently truncates out-of-range values, so convert to the
                    // underlying type first to surface an OverflowException instead.
                    var underlyingValue = Convert.ChangeType(value, Enum.GetUnderlyingType(targetType), CultureInfo.InvariantCulture);

                    return new Optional<T>((T)Enum.ToObject(targetType, underlyingValue));
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
