using System;
using System.ComponentModel.DataAnnotations;

namespace Sharprompt.Internal
{
    internal class ValidationAttributeAdapter
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
