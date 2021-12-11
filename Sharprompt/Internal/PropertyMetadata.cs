using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

using Sharprompt.Forms;

using AnnotationsDataType = System.ComponentModel.DataAnnotations.DataType;

namespace Sharprompt.Internal
{
    internal class PropertyMetadata
    {
        public PropertyMetadata(object model, PropertyInfo propertyInfo)
        {
            var displayAttribute = propertyInfo.GetCustomAttribute<DisplayAttribute>();
            var dataTypeAttribute = propertyInfo.GetCustomAttribute<DataTypeAttribute>();

            PropertyInfo = propertyInfo;
            Type = propertyInfo.PropertyType;
            IsCollection = propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>);
            DataType = dataTypeAttribute?.DataType;
            Message = displayAttribute?.GetName() ?? displayAttribute?.GetDescription();
            Placeholder = displayAttribute?.GetPrompt();
            Order = displayAttribute?.GetOrder();
            DefaultValue = propertyInfo.GetValue(model);
            Validators = propertyInfo.GetCustomAttributes<ValidationAttribute>(true)
                                     .Select(x => new ValidationAttributeAdapter(x).GetValidator(propertyInfo.Name, model))
                                     .ToArray();
            DataSourceProvider = (IDataSourceProvider)propertyInfo.GetCustomAttribute<InlineDataSourceAttribute>() ?? propertyInfo.GetCustomAttribute<MemberDataSourceAttribute>();
        }

        public PropertyInfo PropertyInfo { get; }
        public Type Type { get; }
        public bool IsCollection { get; }
        public AnnotationsDataType? DataType { get; }
        public string Message { get; }
        public string Placeholder { get; set; }
        public int? Order { get; }
        public object DefaultValue { get; }
        public IReadOnlyList<Func<object, ValidationResult>> Validators { get; }
        public IDataSourceProvider DataSourceProvider { get; set; }

        public FormType DetermineFormType()
        {
            if (DataType == AnnotationsDataType.Password)
            {
                return FormType.Password;
            }

            if (Type == typeof(bool))
            {
                return FormType.Confirm;
            }

            if (!IsCollection && (Type.IsEnum || DataSourceProvider is not null))
            {
                return FormType.Select;
            }

            if (IsCollection && (Type.GetGenericArguments()[0].IsEnum || DataSourceProvider is not null))
            {
                return FormType.MultiSelect;
            }

            if (IsCollection && DataSourceProvider is null)
            {
                return FormType.List;
            }

            return FormType.Input;
        }

        private class ValidationAttributeAdapter
        {
            public ValidationAttributeAdapter(ValidationAttribute validationAttribute)
            {
                _validationAttribute = validationAttribute;
            }

            private readonly ValidationAttribute _validationAttribute;

            public Func<object, ValidationResult> GetValidator(string propertyName, object model)
            {
                var validationContext = new ValidationContext(model)
                {
                    DisplayName = propertyName,
                    MemberName = propertyName
                };

                return input => _validationAttribute.GetValidationResult(input, validationContext);
            }
        }
    }
}
