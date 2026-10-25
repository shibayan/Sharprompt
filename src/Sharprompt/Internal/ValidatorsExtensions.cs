using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sharprompt.Internal;

/// <summary>
/// Adds optional validator sequences to a prompt validator collection without changing their order.
/// </summary>
internal static class ValidatorsExtensions
{
    /// <summary>
    /// Appends validators in order, treating a null source sequence as having no validators.
    /// </summary>
    /// <param name="source">The validator collection to append to.</param>
    /// <param name="validators">The validators to apply or append in their existing order.</param>
    public static void Merge(this IList<Func<object?, ValidationResult?>> source, IEnumerable<Func<object?, ValidationResult?>>? validators)
    {
        foreach (var validator in validators ?? [])
        {
            source.Add(validator);
        }
    }
}
