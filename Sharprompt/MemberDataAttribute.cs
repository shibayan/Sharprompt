using System;

namespace Sharprompt
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class MemberDataAttribute : Attribute
    {
        public MemberDataAttribute(Type type, string memberName)
        {
            _type = type;
            _memberName = memberName;
        }

        private readonly Type _type;
        private readonly string _memberName;
    }
}
