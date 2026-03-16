using System;

namespace Sharprompt;

[AttributeUsage(AttributeTargets.Property)]
public sealed class MemberItemsAttribute : Attribute
{
    public MemberItemsAttribute(string memberName)
    {
        ArgumentNullException.ThrowIfNull(memberName);
    }

    public MemberItemsAttribute(string memberName, Type memberType)
        : this(memberName)
    {
        ArgumentNullException.ThrowIfNull(memberType);
    }
}
