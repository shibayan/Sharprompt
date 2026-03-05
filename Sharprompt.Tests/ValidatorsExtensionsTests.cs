using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Sharprompt.Internal;

using Xunit;

namespace Sharprompt.Tests;

public class ValidatorsExtensionsTests
{
    [Fact]
    public void Merge_AddsValidatorsToList()
    {
        var list = new List<Func<object?, ValidationResult?>>();
        Func<object?, ValidationResult?> v1 = _ => ValidationResult.Success;
        Func<object?, ValidationResult?> v2 = _ => ValidationResult.Success;

        list.Merge([v1, v2]);

        Assert.Equal(2, list.Count);
        Assert.Same(v1, list[0]);
        Assert.Same(v2, list[1]);
    }

    [Fact]
    public void Merge_WithNull_DoesNotThrow()
    {
        var list = new List<Func<object?, ValidationResult?>>();

        list.Merge(null);

        Assert.Empty(list);
    }

    [Fact]
    public void Merge_WithEmpty_DoesNotAdd()
    {
        var list = new List<Func<object?, ValidationResult?>>();

        list.Merge([]);

        Assert.Empty(list);
    }
}
