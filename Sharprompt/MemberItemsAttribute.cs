using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Sharprompt.Internal;
using Sharprompt.Strings;

namespace Sharprompt;

[AttributeUsage(AttributeTargets.Property)]
public sealed class MemberItemsAttribute : Attribute, IItemsProvider
{
    public MemberItemsAttribute(string memberName)
    {
        ArgumentNullException.ThrowIfNull(memberName);

        _memberName = memberName;
    }

    public MemberItemsAttribute(string memberName, Type memberType)
        : this(memberName)
    {
        ArgumentNullException.ThrowIfNull(memberType);

        _memberType = memberType;
    }

    private readonly string _memberName;
    private readonly Type? _memberType;

    public IEnumerable<T> GetItems<T>(PropertyInfo targetPropertyInfo) where T : notnull
    {
        var targetType = _memberType ?? targetPropertyInfo.DeclaringType;

        if (targetType is null)
        {
            throw new ArgumentException(string.Format(Resource.Validation_Type_MemberNotFound, _memberType, _memberName));
        }

        var memberInfo = targetType.GetMember(_memberName, BindingFlags.Public | BindingFlags.Static)
                                   .FirstOrDefault();

        if (memberInfo is PropertyInfo propertyInfo)
        {
            if (!typeof(IEnumerable<T>).IsAssignableFrom(propertyInfo.PropertyType))
            {
                throw new ArgumentException(string.Format(Resource.Validation_Type_Incompatible, propertyInfo.PropertyType, typeof(IEnumerable<T>)));
            }

            return (IEnumerable<T>)propertyInfo.GetValue(null)!;
        }

        if (memberInfo is MethodInfo methodInfo)
        {
            if (methodInfo.GetParameters().Length != 0)
            {
                throw new ArgumentException(Resource.Validation_Type_NotParameterlessMethod);
            }

            if (!typeof(IEnumerable<T>).IsAssignableFrom(methodInfo.ReturnType))
            {
                throw new ArgumentException(string.Format(Resource.Validation_Type_Incompatible, methodInfo.ReturnType, typeof(IEnumerable<T>)));
            }

            return (IEnumerable<T>)methodInfo.Invoke(null, null)!;
        }

        throw new ArgumentException(string.Format(Resource.Validation_Type_MemberNotFound, _memberType, _memberName));
    }
}
