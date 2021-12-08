using System;

namespace Sharprompt
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class InlineDataAttribute : Attribute
    {
        public InlineDataAttribute(params object[] source)
        {
            _source = source;
        }

        private readonly object[] _source;
    }
}
