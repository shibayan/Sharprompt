using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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

    [Fact]
    public void TryGet_UnregisteredEnum_FallsBackToReflection()
    {
        var found = EnumMetadataRegistry.TryGet<DayOfWeek>(out var values, out var textSelector);

        Assert.True(found);
        Assert.Equal(Enum.GetValues<DayOfWeek>(), values);
        Assert.Equal("Monday", textSelector(DayOfWeek.Monday));
    }

    [Fact]
    public void TryGet_RegisteredEnum_TakesPrecedenceOverFallback()
    {
        var values = new List<RegisteredEnum> { RegisteredEnum.Second };
        Func<RegisteredEnum, string> textSelector = _ => "custom";

        EnumMetadataRegistry.Register(values, textSelector);

        var found = EnumMetadataRegistry.TryGet<RegisteredEnum>(out var retrievedValues, out var retrievedSelector);

        Assert.True(found);
        Assert.Equal(values, retrievedValues);
        Assert.Same(textSelector, retrievedSelector);
    }

    [Fact]
    public void CreateFallbackMetadata_UsesDisplayAttribute()
    {
        var metadata = EnumMetadataRegistry.CreateFallbackMetadata<AnnotatedEnum>();

        Assert.Equal(new[] { AnnotatedEnum.Third, AnnotatedEnum.First, AnnotatedEnum.Second }, metadata.Values);
        Assert.Equal("The First", metadata.TextSelector(AnnotatedEnum.First));
        Assert.Equal("Second", metadata.TextSelector(AnnotatedEnum.Second));
        Assert.Equal("The Third", metadata.TextSelector(AnnotatedEnum.Third));
    }

    [Fact]
    public void SelectOptions_UnregisteredEnum_HasItems()
    {
        var options = new SelectOptions<FallbackEnum>
        {
            Message = "message"
        };

        options.EnsureOptions();

        Assert.Equal(new[] { FallbackEnum.Foo, FallbackEnum.Bar }, options.Items);
    }

    [Fact]
    public void MultiSelectOptions_UnregisteredEnum_HasItems()
    {
        var options = new MultiSelectOptions<FallbackEnum>
        {
            Message = "message"
        };

        options.EnsureOptions();

        Assert.Equal(new[] { FallbackEnum.Foo, FallbackEnum.Bar }, options.Items);
    }

    public enum RegisteredEnum
    {
        First,
        Second
    }

    public enum AnnotatedEnum
    {
        [Display(Name = "The First", Order = 2)]
        First,
        Second,
        [Display(Name = "The Third", Order = 1)]
        Third
    }

    public enum FallbackEnum
    {
        Foo,
        Bar
    }
}
