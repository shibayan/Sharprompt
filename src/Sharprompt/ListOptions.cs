using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Sharprompt.Strings;

namespace Sharprompt;

/// <summary>
/// Settings for entering a list of values using invariant-culture TypeConverter conversion. Submit an empty line to finish.
/// </summary>
/// <typeparam name="T">The input or item type.</typeparam>
public class ListOptions<T> : PromptOptions where T : notnull
{
    /// <summary>
    /// The items already in the list when the prompt starts. Defaults to an empty sequence.
    /// </summary>
    public IEnumerable<T> DefaultValues { get; set; } = [];

    /// <summary>
    /// The minimum number of items required to finish. Defaults to 1; set to 0 to allow an empty list.
    /// </summary>
    public int Minimum { get; set; } = 1;

    /// <summary>
    /// The maximum number of items. Defaults to int.MaxValue and must be at least Minimum.
    /// </summary>
    public int Maximum { get; set; } = int.MaxValue;

    /// <summary>
    /// Validators applied to each newly entered, converted item. Return ValidationResult.Success (null) to accept it, or a ValidationResult to retry.
    /// </summary>
    public IList<Func<object?, ValidationResult?>> Validators { get; } = [];

    /// <summary>
    /// Checks the prompt message and verifies nonnegative, ordered list-size limits.
    /// </summary>
    /// <exception cref="ArgumentNullException">A required setting is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The minimum is negative or the maximum is less than the minimum.</exception>
    internal override void EnsureOptions()
    {
        base.EnsureOptions();

        if (Minimum < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(Minimum), string.Format(Resource.Validation_Minimum_OutOfRange, Minimum));
        }

        if (Maximum < Minimum)
        {
            throw new ArgumentOutOfRangeException(nameof(Maximum), string.Format(Resource.Validation_Maximum_OutOfRange, Maximum, Minimum));
        }
    }
}
