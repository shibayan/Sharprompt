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
        // Use a type unique to this test: the registry is a global static, so
        // registering a common type such as string leaks into other tests.
        var values = new List<CustomItem> { new("A"), new("B"), new("C") };
        Func<CustomItem, string> textSelector = x => x.Value.ToUpperInvariant();

        EnumMetadataRegistry.Register(values, textSelector);

        var found = EnumMetadataRegistry.TryGet<CustomItem>(out var retrievedValues, out var retrievedSelector);

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
    public void CreateFallbackMetadata_AliasedValues_KeepsSingleDeterministicRepresentative()
    {
        var metadata = EnumMetadataRegistry.CreateFallbackMetadata<AliasedEnum>();

        // Aliases collapse to one entry, keeping the first declared representative ("None").
        Assert.Equal(new[] { AliasedEnum.None, AliasedEnum.Value }, metadata.Values);
        Assert.Equal("None", metadata.TextSelector(AliasedEnum.None));
        Assert.Equal("None", metadata.TextSelector(AliasedEnum.Default));
    }

    [Fact]
    public void Generated_And_Fallback_Metadata_Are_Identical_For_AnnotatedEnum()
    {
        AssertMetadataParity<AnnotatedEnum>();
    }

    [Fact]
    public void Generated_And_Fallback_Metadata_Are_Identical_For_AliasedEnum()
    {
        AssertMetadataParity<AliasedEnum>();
    }

    [Fact]
    public void Generated_And_Fallback_Metadata_Are_Identical_For_AliasedWithDisplayEnum()
    {
        AssertMetadataParity<AliasedWithDisplayEnum>();

        var fallback = EnumMetadataRegistry.CreateFallbackMetadata<AliasedWithDisplayEnum>();
        Assert.Equal(new[] { AliasedWithDisplayEnum.Zero, AliasedWithDisplayEnum.One }, fallback.Values);
        Assert.Equal("Zero (First)", fallback.TextSelector(AliasedWithDisplayEnum.Zero));
        Assert.Equal("Zero (First)", fallback.TextSelector(AliasedWithDisplayEnum.Nothing));
        Assert.Equal("One", fallback.TextSelector(AliasedWithDisplayEnum.One));
    }

    [Fact]
    public void Generated_And_Fallback_Metadata_Are_Identical_For_ExplicitOrderAndUnannotated()
    {
        AssertMetadataParity<AnnotatedOrderEnum>();

        var fallback = EnumMetadataRegistry.CreateFallbackMetadata<AnnotatedOrderEnum>();
        Assert.Equal(
            new[]
            {
                AnnotatedOrderEnum.FirstOrder,
                AnnotatedOrderEnum.SecondOrder,
                AnnotatedOrderEnum.SecondOrderB,
                AnnotatedOrderEnum.UnannotatedA,
                AnnotatedOrderEnum.UnannotatedB
            },
            fallback.Values);
    }

    [Fact]
    public void Generated_And_Fallback_Metadata_Are_Identical_For_OutOfOrderEnum()
    {
        AssertMetadataParity<OutOfOrderEnum>();

        var fallback = EnumMetadataRegistry.CreateFallbackMetadata<OutOfOrderEnum>();
        Assert.Equal(new[] { OutOfOrderEnum.Charlie, OutOfOrderEnum.Alpha, OutOfOrderEnum.Bravo }, fallback.Values);
    }

    [Fact]
    public void SelectOptions_UnregisteredEnum_HasItems()
    {
        var options = new SelectOptions<ConsoleColor>
        {
            Message = "message"
        };

        options.EnsureOptions();

        Assert.Equal(Enum.GetValues<ConsoleColor>(), options.Items);
    }

    [Fact]
    public void MultiSelectOptions_UnregisteredEnum_HasItems()
    {
        var options = new MultiSelectOptions<ConsoleColor>
        {
            Message = "message"
        };

        options.EnsureOptions();

        Assert.Equal(Enum.GetValues<ConsoleColor>(), options.Items);
    }

    private static void AssertMetadataParity<TEnum>() where TEnum : struct, Enum
    {
        var found = EnumMetadataRegistry.TryGet<TEnum>(out var generatedValues, out var generatedSelector);
        Assert.True(found, $"Source generator should have registered metadata for {typeof(TEnum).Name}.");

        var fallback = EnumMetadataRegistry.CreateFallbackMetadata<TEnum>();

        Assert.Equal(generatedValues, fallback.Values);

        foreach (var value in Enum.GetValues<TEnum>())
        {
            Assert.Equal(generatedSelector(value), fallback.TextSelector(value));
        }
    }

    public sealed record CustomItem(string Value);

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

    public enum AliasedEnum
    {
        None = 0,
        Default = 0,
        Value = 1
    }

    public enum AliasedWithDisplayEnum
    {
        [Display(Name = "Zero (First)")]
        Zero = 0,
        [Display(Name = "Zero (Second)")]
        Nothing = 0,
        [Display(Name = "One")]
        One = 1
    }

    public enum AnnotatedOrderEnum
    {
        [Display(Name = "Order 2", Order = 2)]
        SecondOrder = 10,

        UnannotatedA = 20,

        [Display(Name = "Order 1", Order = 1)]
        FirstOrder = 30,

        UnannotatedB = 40,

        [Display(Name = "Order 2 (Second)", Order = 2)]
        SecondOrderB = 50
    }

    public enum OutOfOrderEnum
    {
        Charlie = 3,
        Alpha = 1,
        Bravo = 2
    }
}
