using System.Collections.Generic;

using Sharprompt.Forms;
using Sharprompt.Internal;

namespace Sharprompt
{
    public static partial class Prompt
    {
        public static T AutoForms<T>() where T : new()
        {
            var model = new T();

            StartForms(model);

            return model;
        }

        public static T AutoForms<T>(T model)
        {
            StartForms(model);

            return model;
        }

        private static void StartForms<T>(T model)
        {
            var propertyMetadatas = PropertyMetadataFactory.Create(model);

            foreach (var propertyMetadata in propertyMetadatas)
            {
                var formType = propertyMetadata.DetermineFormType();

                var result = formType switch
                {
                    FormType.Confirm => MakeConfirm(propertyMetadata),
                    FormType.Input => MakeInput(propertyMetadata),
                    FormType.List => MakeList(propertyMetadata),
                    FormType.MultiSelect => MakeMultiSelect(propertyMetadata),
                    FormType.Password => MakePassword(propertyMetadata),
                    FormType.Select => MakeSelect(propertyMetadata),
                    _ => null
                };

                propertyMetadata.PropertyInfo.SetValue(model, result);
            }
        }

        private static bool MakeConfirm(PropertyMetadata propertyMetadata)
        {
            return Confirm(options =>
            {
                options.Message = propertyMetadata.Message;
                options.DefaultValue = (bool?)propertyMetadata.DefaultValue;
            });
        }

        private static object MakeInput(PropertyMetadata propertyMetadata)
        {
            var method = typeof(Prompt).GetMethod(nameof(MakeInputCore)).MakeGenericMethod(propertyMetadata.PropertyType);

            return method.Invoke(null, new object[] { propertyMetadata });
        }

        private static T MakeInputCore<T>(PropertyMetadata propertyMetadata)
        {
            return Input<T>(options =>
            {
                options.Message = propertyMetadata.Message;
                options.DefaultValue = propertyMetadata.DefaultValue;

                options.Validators.Merge(propertyMetadata.Validators);
            });
        }

        private static object MakeList(PropertyMetadata propertyMetadata)
        {
            var method = typeof(Prompt).GetMethod(nameof(MakeListCore)).MakeGenericMethod(propertyMetadata.PropertyType);

            return method.Invoke(null, new object[] { propertyMetadata });
        }

        private static IEnumerable<T> MakeListCore<T>(PropertyMetadata propertyMetadata)
        {
            return List<T>(options =>
            {
                options.Message = propertyMetadata.Message;

                options.Validators.Merge(propertyMetadata.Validators);
            });
        }

        private static object MakeMultiSelect(PropertyMetadata propertyMetadata)
        {
            var method = typeof(Prompt).GetMethod(nameof(MakeMultiSelectCore)).MakeGenericMethod(propertyMetadata.PropertyType);

            return method.Invoke(null, new object[] { propertyMetadata });
        }

        private static IEnumerable<T> MakeMultiSelectCore<T>(PropertyMetadata propertyMetadata)
        {
            return MultiSelect<T>(options =>
            {
                options.Message = propertyMetadata.Message;
            });
        }

        private static string MakePassword(PropertyMetadata propertyMetadata)
        {
            return Password(options =>
            {
                options.Message = propertyMetadata.Message;

                options.Validators.Merge(propertyMetadata.Validators);
            });
        }

        private static object MakeSelect(PropertyMetadata propertyMetadata)
        {
            var method = typeof(Prompt).GetMethod(nameof(MakeSelectCore)).MakeGenericMethod(propertyMetadata.PropertyType);

            return method.Invoke(null, new object[] { propertyMetadata });
        }

        private static T MakeSelectCore<T>(PropertyMetadata propertyMetadata)
        {
            return Select<T>(options =>
            {
                options.Message = propertyMetadata.Message;
                options.DefaultValue = propertyMetadata.DefaultValue;
            });
        }
    }
}
