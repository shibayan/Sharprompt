using System;
using System.Collections.Generic;
using System.Linq;

using Sharprompt.Internal;

namespace Sharprompt
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class InlineDataSourceAttribute : Attribute, IDataSourceProvider
    {
        public InlineDataSourceAttribute(params object[] data)
        {
            _data = data;
        }

        private readonly object[] _data;

        public IEnumerable<T> GetItems<T>() => _data.Cast<T>();
    }
}
