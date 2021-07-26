using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading;

using Sharprompt.Internal;

namespace Sharprompt
{
    public static partial class Prompt
    {
        public static T AutoForms<T>(CancellationToken? cancellationToken) where T : new()
        {
            var model = new T();

            StartForms(model, cancellationToken??CancellationToken.None);

            return model;
        }

        public static T AutoForms<T>(T model, CancellationToken? cancellationToken)
        {
            StartForms(model, cancellationToken??CancellationToken.None);
            return model;
        }

        private static void StartForms<T>(T model, CancellationToken cancellationToken)
        {
            var propertyMetadatas = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                             .Select(x => new PropertyMetadata(x))
                                             .OrderBy(x => x.Order)
                                             .ToArray();

            foreach (var propertyMetadata in propertyMetadatas)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                var propertyInfo = propertyMetadata.PropertyInfo;
                var validators = propertyMetadata.Validations.Select(x => new ValidationAttributeAdapter(x).GetValidator(propertyInfo.Name, model)).ToArray();

                var defaultValue = propertyInfo.GetValue(model);

                if (propertyMetadata.DataType == DataType.Password)
                {
                    propertyInfo.SetValue(model, Password(propertyMetadata.Prompt,cancellationToken, validators));
                }
                else if (propertyMetadata.PropertyType == typeof(bool))
                {
                    propertyInfo.SetValue(model, Confirm(propertyMetadata.Prompt,cancellationToken, (bool?)defaultValue));
                }
                else if (propertyMetadata.PropertyType.IsEnum)
                {
                    var method = _selectMethod.MakeGenericMethod(propertyMetadata.PropertyType);

                    propertyInfo.SetValue(model, InvokeMethod(method, propertyMetadata.Prompt,cancellationToken, null, defaultValue));
                }
                else if (propertyMetadata.IsCollection && propertyMetadata.PropertyType.GetGenericArguments()[0].IsEnum)
                {
                    var method = _multiSelectMethod.MakeGenericMethod(propertyMetadata.PropertyType.GetGenericArguments()[0]);

                    propertyInfo.SetValue(model, InvokeMethod(method, propertyMetadata.Prompt,cancellationToken, null, 1, -1, defaultValue));
                }
                else
                {
                    var method = _inputMethod.MakeGenericMethod(propertyMetadata.PropertyType);
                    propertyInfo.SetValue(model, InvokeMethod(method, propertyMetadata.Prompt, cancellationToken, defaultValue, validators));
                }
            }
        }

        private static object InvokeMethod(MethodInfo methodInfo, params object[] parameters)
        {
            return methodInfo.Invoke(null, parameters);

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
                Prompt = displayAttribute?.GetPrompt();
                Order = displayAttribute?.GetOrder();
                Validations = propertyInfo.GetCustomAttributes<ValidationAttribute>(true);
            }

            public PropertyInfo PropertyInfo { get; }
            public Type PropertyType { get; }
            public DataType? DataType { get; }
            public bool IsCollection { get; }
            public string Prompt { get; }
            public int? Order { get; }
            public IEnumerable<ValidationAttribute> Validations { get; }
        }

        private static readonly MethodInfo _inputMethod = typeof(Prompt).GetMethods().First(x => x.Name == nameof(Input) && x.GetParameters().Length == 4);
        private static readonly MethodInfo _selectMethod = typeof(Prompt).GetMethods().First(x => x.Name == nameof(Select) && x.GetParameters().Length == 4);
        private static readonly MethodInfo _multiSelectMethod = typeof(Prompt).GetMethods().First(x => x.Name == nameof(MultiSelect) && x.GetParameters().Length == 6);
    }
}
