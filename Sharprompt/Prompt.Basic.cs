using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

using Sharprompt.Forms;
using Sharprompt.Internal;

namespace Sharprompt
{
    public static partial class Prompt
    {
        public static T Input<T>(InputOptions options)
        {
            using var form = new InputForm<T>(options);

            return form.Start();
        }

        public static T Input<T>(Action<InputOptions> configure)
        {
            var options = new InputOptions();

            configure(options);

            return Input<T>(options);
        }

        public static T Input<T>(string message, object defaultValue = null, IList<Func<object, ValidationResult>> validators = null)
        {
            var options = new InputOptions
            {
                Message = message,
                DefaultValue = defaultValue
            };

            if (validators != null)
            {
                foreach (var validator in validators)
                {
                    options.Validators.Add(validator);
                }
            }

            return Input<T>(options);
        }

        public static string Password(PasswordOptions options)
        {
            using var form = new PasswordForm(options);

            return form.Start();
        }

        public static string Password(Action<PasswordOptions> configure)
        {
            var options = new PasswordOptions();

            configure(options);

            return Password(options);
        }

        public static string Password(string message, IList<Func<object, ValidationResult>> validators = null)
        {
            var options = new PasswordOptions
            {
                Message = message
            };

            if (validators != null)
            {
                foreach (var validator in validators)
                {
                    options.Validators.Add(validator);
                }
            }

            return Password(options);
        }

        public static bool Confirm(ConfirmOptions options)
        {
            using var form = new ConfirmForm(options);

            return form.Start();
        }

        public static bool Confirm(Action<ConfirmOptions> configure)
        {
            var options = new ConfirmOptions();

            configure(options);

            return Confirm(options);
        }

        public static bool Confirm(string message, bool? defaultValue = null)
        {
            var options = new ConfirmOptions
            {
                Message = message,
                DefaultValue = defaultValue
            };

            return Confirm(options);
        }

        public static T Select<T>(SelectOptions<T> options)
        {
            using var form = new SelectForm<T>(options);

            return form.Start();
        }

        public static T Select<T>(Action<SelectOptions<T>> configure)
        {
            var options = new SelectOptions<T>();

            configure(options);

            return Select(options);
        }

        public static T Select<T>(string message, int? pageSize = null, T? defaultValue = null) where T : struct, Enum
        {
            var items = EnumValue<T>.GetValues();

            var options = new SelectOptions<EnumValue<T>>
            {
                Message = message,
                Items = items,
                DefaultValue = (EnumValue<T>)defaultValue,
                PageSize = pageSize,
                TextSelector = x => x.DisplayName
            };

            return Select(options).Value;
        }

        public static T Select<T>(string message, IEnumerable<T> items, int? pageSize = null, object defaultValue = null, Func<T, string> textSelector = null)
        {
            var options = new SelectOptions<T>
            {
                Message = message,
                Items = items,
                DefaultValue = defaultValue,
                PageSize = pageSize,
                TextSelector = textSelector ?? (x => x.ToString())
            };

            return Select(options);
        }

        public static IEnumerable<T> MultiSelect<T>(MultiSelectOptions<T> options)
        {
            using var form = new MultiSelectForm<T>(options);

            return form.Start();
        }

        public static IEnumerable<T> MultiSelect<T>(Action<MultiSelectOptions<T>> configure)
        {
            var options = new MultiSelectOptions<T>();

            configure(options);

            return MultiSelect(options);
        }

        public static IEnumerable<T> MultiSelect<T>(string message, int? pageSize = null, int minimum = 1, int maximum = int.MaxValue, IEnumerable<T> defaultValues = null) where T : struct, Enum
        {
            var items = EnumValue<T>.GetValues();

            var options = new MultiSelectOptions<EnumValue<T>>
            {
                Message = message,
                Items = items,
                DefaultValues = defaultValues?.Select(x => (EnumValue<T>)x),
                PageSize = pageSize,
                Minimum = minimum,
                Maximum = maximum,
                TextSelector = x => x.DisplayName
            };

            return MultiSelect(options).Select(x => x.Value);
        }

        public static IEnumerable<T> MultiSelect<T>(string message, IEnumerable<T> items, int? pageSize = null, int minimum = 1, int maximum = int.MaxValue, IEnumerable<T> defaultValues = null, Func<T, string> textSelector = null)
        {
            var options = new MultiSelectOptions<T>
            {
                Message = message,
                Items = items,
                DefaultValues = defaultValues,
                PageSize = pageSize,
                Minimum = minimum,
                Maximum = maximum,
                TextSelector = x => x.ToString()
            };

            return MultiSelect(options);
        }

        public static IEnumerable<T> List<T>(ListOptions<T> options)
        {
            using var form = new ListForm<T>(options);

            return form.Start();
        }

        public static IEnumerable<T> List<T>(Action<ListOptions<T>> configure)
        {
            var options = new ListOptions<T>();

            configure(options);

            return List(options);
        }

        public static IEnumerable<T> List<T>(string message, int minimum = 1, int maximum = int.MaxValue, IList<Func<object, ValidationResult>> validators = null)
        {
            var options = new ListOptions<T>
            {
                Message = message,
                Minimum = minimum,
                Maximum = maximum
            };

            if (validators != null)
            {
                foreach (var validator in validators)
                {
                    options.Validators.Add(validator);
                }
            }

            return List(options);
        }
    }
}
