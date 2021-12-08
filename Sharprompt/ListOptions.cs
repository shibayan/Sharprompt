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

        public ListOptions<T> SetMessage(string message)
        {
            Message = message;

            return this;
        }

        public ListOptions<T> SetDefaultValues(IEnumerable<T> defaultValues)
        {
            DefaultValues = defaultValues;

            return this;
        }

        public ListOptions<T> SetMinimum(int minimum)
        {
            Minimum = minimum;

            return this;
        }

        public ListOptions<T> SetMaximum(int maximum)
        {
            Maximum = maximum;

            return this;
        }

        public ListOptions<T> AddValidators(params Func<object, ValidationResult>[] validators)
        {
            foreach (var validator in validators)
            {
                Validators.Add(validator);
            }

            return this;
        }
    }
}
