using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sharprompt
{
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
                throw new ArgumentOutOfRangeException(nameof(Minimum), $"The minimum ({Minimum}) is not valid");
            }

            if (Maximum < Minimum)
            {
                throw new ArgumentOutOfRangeException(nameof(Maximum), $"The maximum ({Maximum}) is not valid when minimum is set to ({Minimum})");
            }
        }
    }
}
