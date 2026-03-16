using System;
using System.Collections.Generic;

using Xunit;

namespace Sharprompt.Tests;

public class EnumMetadataRegistryTests
{
    [Fact]
    public void Register_And_TryGet_ReturnsRegisteredValues()
    {
        var values = new List<string> { "A", "B", "C" };
        Func<string, string> textSelector = x => x.ToUpperInvariant();

        EnumMetadataRegistry.Register(values, textSelector);

        var found = EnumMetadataRegistry.TryGet<string>(out var retrievedValues, out var retrievedSelector);

        Assert.True(found);
        Assert.Equal(values, retrievedValues);
        Assert.Same(textSelector, retrievedSelector);
    }

    [Fact]
    public void TryGet_Unregistered_ReturnsFalse()
    {
        var found = EnumMetadataRegistry.TryGet<Uri>(out _, out _);

        Assert.False(found);
    }
}
