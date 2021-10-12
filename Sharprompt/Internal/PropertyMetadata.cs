using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

using Sharprompt.Forms;

namespace Sharprompt.Internal
{
    internal class PropertyMetadata
    {
        public PropertyMetadata(object model, PropertyInfo propertyInfo)
        {
            var displayAttribute = propertyInfo.GetCustomAttribute<DisplayAttribute>();
            var dataTypeAttribute = propertyInfo.GetCustomAttribute<DataTypeAttribute>();

            PropertyInfo = propertyInfo;
            PropertyType = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;
            DataType = dataTypeAttribute?.DataType;
            IsCollection = propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>);
            Message = displayAttribute?.GetPrompt();
            Order = displayAttribute?.GetOrder();
            DefaultValue = propertyInfo.GetValue(model);
            Validators = propertyInfo.GetCustomAttributes<ValidationAttribute>(true)
                                     .Select(x => new ValidationAttributeAdapter(x).GetValidator(PropertyInfo.Name, model))
                                     .ToArray();
        }

        public PropertyInfo PropertyInfo { get; }
        public Type PropertyType { get; }
        public DataType? DataType { get; }
        public bool IsCollection { get; }
        public string Message { get; }
        public int? Order { get; }
        public object DefaultValue { get; }
        public IReadOnlyList<Func<object, ValidationResult>> Validators { get; }

        public FormType DetermineFormType()
        {
            if (DataType == System.ComponentModel.DataAnnotations.DataType.Password)
            {
                return FormType.Password;
            }

            if (PropertyType == typeof(bool))
            {
                return FormType.Confirm;
            }

            if (PropertyType.IsEnum)
            {
                return FormType.Select;
            }

            if (IsCollection && PropertyType.GetGenericArguments()[0].IsEnum)
            {
                return FormType.MultiSelect;
            }

            if (IsCollection)
            {
                return FormType.List;
            }

            return FormType.Input;
        }
    }
}
