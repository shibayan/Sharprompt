using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Sharprompt.Internal;

namespace Sharprompt
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class MemberDataSourceAttribute : Attribute, IDataSourceProvider
    {
        public MemberDataSourceAttribute(Type memberType, string memberName)
        {
            _memberType = memberType;
            _memberName = memberName;
        }

        private readonly Type _memberType;
        private readonly string _memberName;

        public IEnumerable<T> GetItems<T>()
        {
            var memberInfo = _memberType.GetMember(_memberName, BindingFlags.Public | BindingFlags.Static)
                                        .FirstOrDefault();

            if (memberInfo is PropertyInfo propertyInfo)
            {
                if (propertyInfo.PropertyType != typeof(IEnumerable<T>))
                {
                    throw new ArgumentException("");
                }

                return (IEnumerable<T>)propertyInfo.GetValue(null);
            }

            if (memberInfo is MethodInfo methodInfo)
            {
                if (methodInfo.GetParameters().Length != 0)
                {
                    throw new ArgumentException("");
                }

                if (methodInfo.ReturnType != typeof(IEnumerable<T>))
                {
                    throw new ArgumentException("");
                }

                return (IEnumerable<T>)methodInfo.Invoke(null, null);
            }

            throw new ArgumentException($"{_memberType.Name} {_memberName}");
        }
    }
}
