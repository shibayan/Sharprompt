using System;

namespace Sharprompt;

/// <summary>
/// Provides items from a public static property or parameterless method returning IEnumerable of the property item type.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class MemberItemsAttribute : Attribute
{
    /// <summary>
    /// Specifies a public static item provider.
    /// </summary>
    /// <param name="memberName">The provider property or parameterless method name. Cannot be null.</param>
    /// <remarks>The provider is resolved on the bound model type.</remarks>
    public MemberItemsAttribute(string memberName)
    {
        ArgumentNullException.ThrowIfNull(memberName);
    }

    /// <summary>
    /// Specifies a public static item provider.
    /// </summary>
    /// <param name="memberName">The provider property or parameterless method name. Cannot be null.</param>
    /// <param name="memberType">The type declaring the provider. Cannot be null.</param>
    public MemberItemsAttribute(string memberName, Type memberType)
        : this(memberName)
    {
        ArgumentNullException.ThrowIfNull(memberType);
    }
}
