using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

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
            var propertyMetadatas = PropertyMetadataFactory.Create<T>();

            foreach (var propertyMetadata in propertyMetadatas)
            {
                var propertyInfo = propertyMetadata.PropertyInfo;

                var defaultValue = propertyInfo.GetValue(model);
                var validators = propertyMetadata.GetValidator(model);

                if (propertyMetadata.DataType == DataType.Password)
                {
                    var options = new PasswordOptions
                    {
                        Message = propertyMetadata.Message
                    };

                    options.Validators.Merge(validators);

                    propertyInfo.SetValue(model, Password(options));
                }
                else if (propertyMetadata.PropertyType == typeof(bool))
                {
                    var options = new ConfirmOptions
                    {
                        Message = propertyMetadata.Message,
                        DefaultValue = (bool?)defaultValue
                    };

                    propertyInfo.SetValue(model, Confirm(options));
                }
                else if (propertyMetadata.PropertyType.IsEnum)
                {
                    var method = _selectMethod.MakeGenericMethod(propertyMetadata.PropertyType);

                    propertyInfo.SetValue(model, InvokeMethod(method, propertyMetadata.Message, null, defaultValue));
                }
                else if (propertyMetadata.IsCollection && propertyMetadata.PropertyType.GetGenericArguments()[0].IsEnum)
                {
                    var method = _multiSelectMethod.MakeGenericMethod(propertyMetadata.PropertyType.GetGenericArguments()[0]);

                    propertyInfo.SetValue(model, InvokeMethod(method, propertyMetadata.Message, null, 1, int.MaxValue, defaultValue));
                }
                else
                {
                    var method = _inputMethod.MakeGenericMethod(propertyMetadata.PropertyType);

                    propertyInfo.SetValue(model, InvokeMethod(method, propertyMetadata.Message, defaultValue, validators));
                }

                propertyInfo.SetValue(model, null);
            }
        }

        private static object InvokeMethod(MethodInfo methodInfo, params object[] parameters)
        {
            return methodInfo.Invoke(null, parameters);
        }

        private static class PropertyMetadataFactory
        {
            public static IReadOnlyList<PropertyMetadata> Create<T>()
            {
                return typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                .Select(x => new PropertyMetadata(x))
                                .OrderBy(x => x.Order)
                                .ToArray();
            }
        }

        private class PropertyMetadata
        {
            public PropertyMetadata(PropertyInfo propertyInfo)
            {
                var displayAttribute = propertyInfo.GetCustomAttribute<DisplayAttribute>();
                var dataTypeAttribute = propertyInfo.GetCustomAttribute<DataTypeAttribute>();

                PropertyInfo = propertyInfo;
                PropertyType = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;
                DataType = dataTypeAttribute?.DataType;
                IsCollection = propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>);
                Message = displayAttribute?.GetPrompt();
                Order = displayAttribute?.GetOrder();
                Validations = propertyInfo.GetCustomAttributes<ValidationAttribute>(true);
            }

            public PropertyInfo PropertyInfo { get; }
            public Type PropertyType { get; }
            public DataType? DataType { get; }
            public bool IsCollection { get; }
            public string Message { get; }
            public int? Order { get; }

            private IEnumerable<ValidationAttribute> Validations { get; }

            public IReadOnlyList<Func<object, ValidationResult>> GetValidator(object model) =>
                Validations.Select(x => new ValidationAttributeAdapter(x).GetValidator(PropertyInfo.Name, model)).ToArray();
        }

        private static readonly MethodInfo _inputMethod = typeof(Prompt).GetMethods().First(x => x.Name == nameof(Input) && x.GetParameters().Length == 3);
        private static readonly MethodInfo _selectMethod = typeof(Prompt).GetMethods().First(x => x.Name == nameof(Select) && x.GetParameters().Length == 3);
        private static readonly MethodInfo _multiSelectMethod = typeof(Prompt).GetMethods().First(x => x.Name == nameof(MultiSelect) && x.GetParameters().Length == 5);
    }
}
