using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Sharprompt.Internal;

internal static class ValidatorsExtensions
{
    public static void Merge(this IList<Func<object?, ValidationResult?>> source, IEnumerable<Func<object?, ValidationResult?>>? validators)
    {
        foreach (var validator in validators ?? Enumerable.Empty<Func<object?, ValidationResult?>>())
        {
            source.Add(validator);
        }
    }
}
