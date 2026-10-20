using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

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

        // Aliases collapse to one entry; the representative is the alias whose
        // name sorts first ordinally ("Default" < "None").
        Assert.Equal(new[] { AliasedEnum.None, AliasedEnum.Value }, metadata.Values);
        Assert.Equal("Default", metadata.TextSelector(AliasedEnum.None));
        Assert.Equal("Default", metadata.TextSelector(AliasedEnum.Default));
    }

    // The cases below mirror EnumMetadataGeneratorTests in Sharprompt.SourceGenerator.Tests, which also
    // checks that the generated metadata matches this fallback for each of them.

    [Fact]
    public void CreateFallbackMetadata_Aliases_UseRepresentativeDisplayAttribute()
    {
        var metadata = EnumMetadataRegistry.CreateFallbackMetadata<ParityAliasEnum>();

        // "Default" < "None" and "Alpha" < "Zulu", so only their [Display] attributes apply.
        Assert.Equal(new[] { ParityAliasEnum.Alpha, ParityAliasEnum.Default, ParityAliasEnum.Value }, metadata.Values);
        Assert.Equal(new[] { "Alpha", "Default value", "Value" }, metadata.Values.Select(metadata.TextSelector));
        Assert.Equal("Default value", metadata.TextSelector(ParityAliasEnum.None));
        Assert.Equal("Alpha", metadata.TextSelector(ParityAliasEnum.Zulu));
    }

    [Fact]
    public void CreateFallbackMetadata_OutOfOrderDeclarations_AreSortedByValue()
    {
        var metadata = EnumMetadataRegistry.CreateFallbackMetadata<ParityOutOfOrderEnum>();

        Assert.Equal(new[] { ParityOutOfOrderEnum.Alpha, ParityOutOfOrderEnum.Bravo, ParityOutOfOrderEnum.Charlie }, metadata.Values);
    }

    [Fact]
    public void CreateFallbackMetadata_NegativeValues_AreSortedAsSigned()
    {
        var metadata = EnumMetadataRegistry.CreateFallbackMetadata<ParitySignedEnum>();

        Assert.Equal(new[] { ParitySignedEnum.Min, ParitySignedEnum.MinusOne, ParitySignedEnum.Zero, ParitySignedEnum.One }, metadata.Values);
    }

    [Fact]
    public void CreateFallbackMetadata_UnsignedValues_AreSortedAsUnsigned()
    {
        var metadata = EnumMetadataRegistry.CreateFallbackMetadata<ParityUnsignedEnum>();

        Assert.Equal(new[] { ParityUnsignedEnum.Zero, ParityUnsignedEnum.One, ParityUnsignedEnum.Max }, metadata.Values);
    }

    [Fact]
    public void CreateFallbackMetadata_FlagsCompositeValues_AreSortedByValue()
    {
        var metadata = EnumMetadataRegistry.CreateFallbackMetadata<ParityFlagsEnum>();

        Assert.Equal(new[] { ParityFlagsEnum.None, ParityFlagsEnum.Read, ParityFlagsEnum.Write, ParityFlagsEnum.ReadWrite, ParityFlagsEnum.Execute, ParityFlagsEnum.All }, metadata.Values);
        Assert.Equal(new[] { "None", "Read", "Write", "Read and write", "Execute", "All" }, metadata.Values.Select(metadata.TextSelector));

        // A combination without its own member is not listed and falls back to Enum.ToString.
        Assert.Equal("Read, Execute", metadata.TextSelector(ParityFlagsEnum.Read | ParityFlagsEnum.Execute));
    }

    [Fact]
    public void CreateFallbackMetadata_MixedOrderedAndUnordered_PutsUnorderedLastInValueOrder()
    {
        var metadata = EnumMetadataRegistry.CreateFallbackMetadata<ParityMixedOrderEnum>();

        Assert.Equal(new[] { ParityMixedOrderEnum.Echo, ParityMixedOrderEnum.Delta, ParityMixedOrderEnum.Foxtrot, ParityMixedOrderEnum.Bravo, ParityMixedOrderEnum.Alpha, ParityMixedOrderEnum.Charlie }, metadata.Values);
        Assert.Equal(new[] { "Echo", "The Delta", "Foxtrot", "Bravo", "Alpha", "Charlie" }, metadata.Values.Select(metadata.TextSelector));
    }

    [Fact]
    public void CreateFallbackMetadata_OrderIntMaxValue_ComesBeforeUnordered()
    {
        var metadata = EnumMetadataRegistry.CreateFallbackMetadata<ParityMaxOrderEnum>();

        Assert.Equal(new[] { ParityMaxOrderEnum.Zero, ParityMaxOrderEnum.Max, ParityMaxOrderEnum.MaxToo, ParityMaxOrderEnum.Unordered, ParityMaxOrderEnum.AlsoUnordered }, metadata.Values);
        Assert.Equal(new[] { "Zero", "Max", "Max too", "Unordered", "AlsoUnordered" }, metadata.Values.Select(metadata.TextSelector));
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

    public enum FallbackEnum
    {
        Foo,
        Bar
    }

    public enum ParityAliasEnum
    {
        [Display(Name = "Nothing", Order = 0)]
        None = 0,
        [Display(Name = "Default value")]
        Default = 0,
        Value = 1,
        [Display(Order = 0)]
        Alpha = 2,
        Zulu = 2
    }

    public enum ParityOutOfOrderEnum
    {
        Charlie = 3,
        Alpha = 1,
        Bravo = 2
    }

    public enum ParitySignedEnum : long
    {
        Zero = 0,
        MinusOne = -1,
        One = 1,
        Min = long.MinValue
    }

    public enum ParityUnsignedEnum : ulong
    {
        Max = ulong.MaxValue,
        Zero = 0,
        One = 1
    }

    [Flags]
    public enum ParityFlagsEnum
    {
        None = 0,
        [Display(Name = "Read and write")]
        ReadWrite = Read | Write,
        Read = 1,
        Write = 2,
        Execute = 4,
        All = Read | Write | Execute
    }

    public enum ParityMixedOrderEnum
    {
        Alpha,
        [Display(Order = 2)]
        Bravo,
        Charlie,
        [Display(Name = "The Delta", Order = 1)]
        Delta,
        [Display(Order = -1)]
        Echo,
        [Display(Order = 1)]
        Foxtrot
    }

    public enum ParityMaxOrderEnum
    {
        Unordered = 0,
        [Display(Order = int.MaxValue)]
        Max = 1,
        [Display(Order = 0)]
        Zero = 2,
        AlsoUnordered = 3,
        [Display(Name = "Max too", Order = int.MaxValue)]
        MaxToo = 4
    }
}
