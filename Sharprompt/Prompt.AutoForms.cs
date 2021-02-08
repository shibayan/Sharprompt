using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Sharprompt
{
    public static partial class Prompt
    {
        public static T AutoForms<T>() where T : new()
        {
            var model = new T();

            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var propertyInfo in properties)
            {
                var displayAttribute = propertyInfo.GetCustomAttribute<DisplayAttribute>();
                var dataTypeAttribute = propertyInfo.GetCustomAttribute<DataTypeAttribute>();

                if (dataTypeAttribute?.DataType == DataType.Password)
                {
                    propertyInfo.SetValue(model, Prompt.Password(displayAttribute.Description));
                }
                else if (propertyInfo.PropertyType.IsEnum)
                {
                    var method = typeof(Prompt).GetMethods()
                                               .First(x => x.Name == nameof(Select))
                                               .MakeGenericMethod(propertyInfo.PropertyType);

                    propertyInfo.SetValue(model, method.Invoke(null, new object[] { displayAttribute.Description, null, null, null }));
                }
                else if (propertyInfo.PropertyType == typeof(bool))
                {
                    propertyInfo.SetValue(model, Prompt.Confirm(displayAttribute.Description));
                }
                else
                {
                    var method = typeof(Prompt).GetMethod(nameof(Input))
                                               .MakeGenericMethod(propertyInfo.PropertyType);

                    propertyInfo.SetValue(model, method.Invoke(null, new object[] { displayAttribute.Description, null, null }));
                }
            }

            return model;
        }
    }
}
