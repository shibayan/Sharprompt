using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;

using Sharprompt.Forms;
using Sharprompt.Internal;

namespace Sharprompt
{
    public static partial class Prompt
    {
        public static bool AnyKey(CancellationToken? cancellationToken)
        {
            using var form = new AnykeyForm();
            return form.Start(cancellationToken ?? CancellationToken.None);
        }

        public static T Input<T>(InputOptions options, CancellationToken? cancellationToken)
        {
            using var form = new InputForm<T>(options);

            return form.Start(cancellationToken??CancellationToken.None);
        }

        public static T Input<T>(Action<InputOptions> configure, CancellationToken? cancellationToken)
        {
            var options = new InputOptions();

            configure(options);

            return Input<T>(options, cancellationToken ?? CancellationToken.None);
        }

        public static T Input<T>(string message, object defaultValue = null, IList<Func<object, ValidationResult>> validators = null)
        {
            return Input<T>(message, CancellationToken.None, defaultValue, validators);
        }

        public static T Input<T>(string message, CancellationToken cancellationToken, object defaultValue = null, IList<Func<object, ValidationResult>> validators = null)
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

            return Input<T>(options, cancellationToken);
        }


        public static string Password(PasswordOptions options, CancellationToken? cancellationToken)
        {
            using var form = new PasswordForm(options);

            return form.Start(cancellationToken ?? CancellationToken.None);

        }

        public static string Password(Action<PasswordOptions> configure, CancellationToken? cancellationToken)
        {
            var options = new PasswordOptions();

            configure(options);

            return Password(options, cancellationToken??CancellationToken.None);
        }

        public static string Password(string message, IList<Func<object, ValidationResult>> validators = null)
        {
            return Password(message, CancellationToken.None, validators);
        }

        public static string Password(string message, CancellationToken cancellationToken, IList<Func<object, ValidationResult>> validators = null)
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

            return Password(options, cancellationToken);
        }


        public static bool Confirm(ConfirmOptions options, CancellationToken? cancellationToken)
        {
            using var form = new ConfirmForm(options);

            return form.Start(cancellationToken ?? CancellationToken.None);
        }

        public static bool Confirm(Action<ConfirmOptions> configure, CancellationToken? cancellationToken)
        {
            var options = new ConfirmOptions();

            configure(options);

            return Confirm(options, cancellationToken??CancellationToken.None);
        }

        public static bool Confirm(string message, bool? defaultValue = null)
        {
            return Confirm(message, CancellationToken.None, defaultValue);
        }

        public static bool Confirm(string message, CancellationToken cancellationToken, bool? defaultValue = null)
        {
            var options = new ConfirmOptions
            {
                Message = message,
                DefaultValue = defaultValue
            };

            return Confirm(options, cancellationToken);
        }

        public static T Select<T>(SelectOptions<T> options, CancellationToken? cancellationToken)
        {
            using var form = new SelectForm<T>(options);

            return form.Start(cancellationToken??CancellationToken.None);
        }

        public static T Select<T>(Action<SelectOptions<T>> configure, CancellationToken? cancellationToken)
        {
            var options = new SelectOptions<T>();

            configure(options);

            return Select(options, cancellationToken??CancellationToken.None);
        }

        public static T Select<T>(string message, int? pageSize = null, T? defaultValue = null) where T : struct, Enum
        {
            return Select(message, CancellationToken.None, pageSize, defaultValue);
        }

        public static T Select<T>(string message, CancellationToken cancellationToken, int? pageSize = null, T? defaultValue = null) where T : struct, Enum
        {
            var items = EnumValue<T>.GetValues();

            var options = new SelectOptions<EnumValue<T>>
            {
                Message = message,
                Items = items,
                DefaultValue = (EnumValue<T>)defaultValue,
                PageSize = pageSize,
                TextSelector = x => x?.DisplayName
            };
            var aux = Select(options, cancellationToken);
            if (cancellationToken.IsCancellationRequested)
            {
                return default;
            }
            if (aux != null)
            {
                return aux.Value;
            }
            return default;
        }

        public static T Select<T>(string message, IEnumerable<T> items, int? pageSize = null, object defaultValue = null, Func<T, string> textSelector = null)
        {
            return Select(message, items, CancellationToken.None, pageSize, defaultValue);
        }

        public static T Select<T>(string message, IEnumerable<T> items, CancellationToken cancellationToken, int? pageSize = null, object defaultValue = null, Func<T, string> textSelector = null)
        {
            var options = new SelectOptions<T>
            {
                Message = message,
                Items = items,
                DefaultValue = defaultValue,
                PageSize = pageSize,
                TextSelector = textSelector ?? (x => x?.ToString())
            };

            return Select(options, cancellationToken);
        }

        public static IEnumerable<T> MultiSelect<T>(MultiSelectOptions<T> options, CancellationToken? cancellationToken)
        {
            using var form = new MultiSelectForm<T>(options);

            return form.Start(cancellationToken??CancellationToken.None);

        }

        public static IEnumerable<T> MultiSelect<T>(Action<MultiSelectOptions<T>> configure, CancellationToken? cancellationToken)
        {
            var options = new MultiSelectOptions<T>();

            configure(options);

            return MultiSelect(options, cancellationToken??CancellationToken.None);
        }

        public static IEnumerable<T> MultiSelect<T>(string message, int? pageSize = null, int minimum = 1, int maximum = -1, IEnumerable<T> defaultValues = null) where T : struct, Enum
        {
            return MultiSelect(message, CancellationToken.None, pageSize, minimum, maximum, defaultValues);
        }

        public static IEnumerable<T> MultiSelect<T>(string message, CancellationToken cancellationToken, int? pageSize = null, int minimum = 1, int maximum = -1, IEnumerable<T> defaultValues = null) where T : struct, Enum
        {
            var items = EnumValue<T>.GetValues();

            var options = new MultiSelectOptions<EnumValue<T>>
            {
                Message = message,
                Items = items,
                DefaultValues = defaultValues?.Select(x => (EnumValue<T>)x),
                PageSize = pageSize,
                Minimum = minimum < 1 ? 1 : minimum,
                Maximum = maximum < 0 ? int.MaxValue : maximum,
                TextSelector = x => x.DisplayName
            };

            var aux = MultiSelect(options, cancellationToken);
            if (cancellationToken.IsCancellationRequested)
            {
                return null;
            }
            return aux.Select(x => x.Value);
        }

        public static IEnumerable<T> MultiSelect<T>(string message, IEnumerable<T> items, int? pageSize = null, int minimum = 1, int maximum = -1, IEnumerable<T> defaultValues = null, Func<T, string> textSelector = null)
        {
            return MultiSelect(message, items, CancellationToken.None, pageSize, minimum, maximum, defaultValues);
        }

        public static IEnumerable<T> MultiSelect<T>(string message, IEnumerable<T> items, CancellationToken cancellationToken, int? pageSize = null, int minimum = 1, int maximum = -1, IEnumerable<T> defaultValues = null, Func<T, string> textSelector = null)
        {
            var options = new MultiSelectOptions<T>
            {
                Message = message,
                Items = items,
                DefaultValues = defaultValues,
                PageSize = pageSize,
                Minimum = minimum<0?0:minimum,
                Maximum = maximum<0?int.MaxValue:maximum,
                TextSelector = textSelector?? (x => x?.ToString())
            };

            return MultiSelect(options, cancellationToken);
        }

        public static IEnumerable<T> List<T>(ListOptions<T> options, CancellationToken? cancellationToken)
        {
            using var form = new ListForm<T>(options);
            return form.Start(cancellationToken??CancellationToken.None);
        }

        public static IEnumerable<T> List<T>(Action<ListOptions<T>> configure, CancellationToken? cancellationToken)
        {
            var options = new ListOptions<T>();

            configure(options);

            return List(options, cancellationToken??CancellationToken.None);
        }

        public static IEnumerable<T> List<T>(string message, int minimum = 1, int maximum = -1, IList<Func<object, ValidationResult>> validators = null)
        {
            return List<T>(message, CancellationToken.None, minimum, maximum, validators);
        }

        public static IEnumerable<T> List<T>(string message, CancellationToken cancellationToken, int minimum = 1, int maximum = -1, IList<Func<object, ValidationResult>> validators = null)
        {
            var options = new ListOptions<T>
            {
                Message = message,
                Minimum = minimum<0?0:minimum,
                Maximum = maximum<0?int.MaxValue:maximum
            };

            if (validators != null)
            {
                foreach (var validator in validators)
                {
                    options.Validators.Add(validator);
                }
            }

            return List(options, cancellationToken);
        }

        public static PathSelected FileBrowser(PathOptions options, CancellationToken? cancellationToken)
        {
            using var form = new FileBrowserForm(options);

            return form.Start(cancellationToken??CancellationToken.None);
        }

        public static PathSelected FileBrowser(Action<PathOptions> configure, CancellationToken? cancellationToken)
        {
            var options = new PathOptions();

            configure(options);

            return FileBrowser(options, cancellationToken??CancellationToken.None);
        }

        public static PathSelected FileBrowser(FileBrowserChoose fileBrowserChoose, string message, string defaultValue = null, string prefixExtension = null, bool allowNotSelected = false, string rootFolder = null, string searchPattern = null, int? pageSize = null, bool supressHidden = true, bool promptCurrentPath = true, bool promptSearchPattern = true, bool showMarkup = true)
        {
            return FileBrowser(fileBrowserChoose, message, CancellationToken.None, defaultValue, prefixExtension, allowNotSelected, rootFolder, searchPattern, pageSize, supressHidden, promptCurrentPath, promptSearchPattern, showMarkup);
        }

        public static PathSelected FileBrowser(FileBrowserChoose fileBrowserChoose, string message, CancellationToken cancellationToken, string defaultValue = null, string prefixExtension = null, bool allowNotSelected = false, string rootFolder = null, string searchPattern = null, int? pageSize = null, bool supressHidden = true, bool promptCurrentPath = true, bool promptSearchPattern = true, bool showMarkup = true)
        {
            var options = new PathOptions
            {
                AllowNotSelected = allowNotSelected,
                BrowserChoose = fileBrowserChoose,
                DefaultValue = defaultValue,
                PageSize = pageSize,
                PrefixExtension = prefixExtension,
                ShowNavigationCurrentPath = promptCurrentPath,
                PromptSearchPattern = promptSearchPattern,
                RootFolder = rootFolder,
                SearchPattern = searchPattern,
                ShowMarkup = showMarkup,
                SupressHidden = supressHidden,
                Message = message
            };
            return FileBrowser(options, cancellationToken);
        }


    }
}
