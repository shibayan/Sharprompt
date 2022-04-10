using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Sharprompt.Strings;

namespace Sharprompt;

public class ListOptions<T>
{
    public string Message { get; set; }

    public IEnumerable<T> DefaultValues { get; set; }

    public int Minimum { get; set; } = 1;

    public int Maximum { get; set; } = int.MaxValue;

    public IList<Func<object, ValidationResult>> Validators { get; } = new List<Func<object, ValidationResult>>();

    internal void EnsureOptions()
    {
        _ = Message ?? throw new ArgumentNullException(nameof(Message));

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
