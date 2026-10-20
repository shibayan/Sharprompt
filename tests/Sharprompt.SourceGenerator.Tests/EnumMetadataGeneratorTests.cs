using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

using Xunit;

namespace Sharprompt.SourceGenerator.Tests;

// Each case runs the generated code and compares it with EnumMetadataRegistry.CreateFallbackMetadata on
// the same enum, so the generator and the reflection fallback must agree on values and display names.
// The same cases are covered for the fallback alone in Sharprompt.Tests (EnumMetadataRegistryTests).
public class EnumMetadataGeneratorTests
{
    private const string Usings = """
        using System;
        using System.ComponentModel.DataAnnotations;

        namespace TestApp;

        """;

    [Fact]
    public void Aliases_KeepOrdinalFirstNameAsRepresentative()
    {
        var metadata = AssertMatchesFallback("""
            public enum Sample
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
            """);

        // "Default" < "None" and "Alpha" < "Zulu", so only their [Display] attributes apply.
        metadata.AssertValues("Alpha", "Default", "Value");
        Assert.Equal(["Alpha", "Default value", "Value"], metadata.Texts);
        Assert.Equal("Default value", metadata.TextOf("None"));
        Assert.Equal("Alpha", metadata.TextOf("Zulu"));
    }

    [Fact]
    public void OutOfOrderDeclarations_AreSortedByValue()
    {
        var metadata = AssertMatchesFallback("""
            public enum Sample
            {
                Charlie = 3,
                Alpha = 1,
                Bravo = 2
            }
            """);

        metadata.AssertValues("Alpha", "Bravo", "Charlie");
        Assert.Equal(["Alpha", "Bravo", "Charlie"], metadata.Texts);
    }

    [Fact]
    public void NegativeValues_AreSortedAsSigned()
    {
        var metadata = AssertMatchesFallback("""
            public enum Sample : long
            {
                Zero = 0,
                MinusOne = -1,
                One = 1,
                Min = long.MinValue
            }
            """);

        metadata.AssertValues("Min", "MinusOne", "Zero", "One");
    }

    [Fact]
    public void UnsignedValues_AreSortedAsUnsigned()
    {
        var metadata = AssertMatchesFallback("""
            public enum Sample : ulong
            {
                Max = ulong.MaxValue,
                Zero = 0,
                One = 1
            }
            """);

        metadata.AssertValues("Zero", "One", "Max");
    }

    [Fact]
    public void FlagsCompositeValues_AreSortedByValue()
    {
        var metadata = AssertMatchesFallback("""
            [Flags]
            public enum Sample
            {
                None = 0,
                [Display(Name = "Read and write")]
                ReadWrite = Read | Write,
                Read = 1,
                Write = 2,
                Execute = 4,
                All = Read | Write | Execute
            }
            """, extraValues: [5]);

        metadata.AssertValues("None", "Read", "Write", "ReadWrite", "Execute", "All");
        Assert.Equal(["None", "Read", "Write", "Read and write", "Execute", "All"], metadata.Texts);

        // A combination without its own member is not listed and falls back to Enum.ToString.
        Assert.Equal("Read, Execute", metadata.TextOf(5));
    }

    [Fact]
    public void MixedOrderedAndUnordered_PutsUnorderedLastInValueOrder()
    {
        var metadata = AssertMatchesFallback("""
            public enum Sample
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
            """);

        metadata.AssertValues("Echo", "Delta", "Foxtrot", "Bravo", "Alpha", "Charlie");
        Assert.Equal(["Echo", "The Delta", "Foxtrot", "Bravo", "Alpha", "Charlie"], metadata.Texts);
    }

    [Fact]
    public void OrderIntMaxValue_ComesBeforeUnordered()
    {
        var metadata = AssertMatchesFallback("""
            public enum Sample
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
            """);

        metadata.AssertValues("Zero", "Max", "MaxToo", "Unordered", "AlsoUnordered");
        Assert.Equal(["Zero", "Max", "Max too", "Unordered", "AlsoUnordered"], metadata.Texts);
    }

    [Fact]
    public void EmptyEnum_GeneratesNothing()
    {
        var result = GeneratorTestHelper.Run(Usings + "public enum Sample { }", new EnumMetadataGenerator());

        Assert.Empty(result.GeneratorDiagnostics);
        Assert.Null(result.Source);
    }

    [Fact]
    public void UnrelatedEdit_KeepsPipelineOutputCached()
    {
        const string source = Usings + """
            public enum Sample
            {
                [Display(Name = "The First")]
                First,
                Second
            }
            """;

        var compilation = GeneratorTestHelper.CreateCompilation(source);
        var driver = GeneratorTestHelper.CreateDriver(new EnumMetadataGenerator()).RunGenerators(compilation);

        Assert.Single(driver.GetRunResult().Results.Single().GeneratedSources);

        // Appending trivia re-runs the transform for the edited tree, but the result must compare equal.
        var oldTree = compilation.SyntaxTrees.Single();
        var newTree = oldTree.WithChangedText(oldTree.GetText().WithChanges(new TextChange(new TextSpan(oldTree.Length, 0), "\n// edited\n")));

        driver = driver.RunGenerators(compilation.ReplaceSyntaxTree(oldTree, newTree));

        var secondRun = driver.GetRunResult().Results.Single();

        Assert.Single(secondRun.GeneratedSources);
        Assert.All(
            secondRun.TrackedSteps[EnumMetadataGenerator.TransformTrackingName].SelectMany(step => step.Outputs),
            output => Assert.Contains(output.Reason, new[] { IncrementalStepRunReason.Cached, IncrementalStepRunReason.Unchanged }));
    }

    private static EnumMetadataSnapshot AssertMatchesFallback(string declaration, long[]? extraValues = null)
    {
        var result = GeneratorTestHelper.Run(Usings + declaration, new EnumMetadataGenerator());

        Assert.Empty(result.GeneratorDiagnostics);

        var enumType = result.LoadAssembly().GetType("TestApp.Sample", throwOnError: true)!;

        // Running the module initializer registers the generated metadata, as it does at application startup.
        // Check that it did, because TryGet would otherwise silently return the fallback itself.
        RuntimeHelpers.RunModuleConstructor(enumType.Module.ModuleHandle);

        var registry = (ConcurrentDictionary<Type, object>)typeof(EnumMetadataRegistry).GetField("s_metadata", BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!;
        Assert.True(registry.ContainsKey(enumType));

        var arguments = new object?[] { null, null };
        Assert.True((bool)typeof(EnumMetadataRegistry).GetMethod("TryGet", BindingFlags.NonPublic | BindingFlags.Static)!.MakeGenericMethod(enumType).Invoke(null, arguments)!);

        var generated = new EnumMetadataSnapshot(enumType, arguments[0]!, (Delegate)arguments[1]!);

        var fallbackMetadata = typeof(EnumMetadataRegistry).GetMethod("CreateFallbackMetadata", BindingFlags.NonPublic | BindingFlags.Static)!.MakeGenericMethod(enumType).Invoke(null, null)!;
        var fallback = new EnumMetadataSnapshot(
            enumType,
            fallbackMetadata.GetType().GetProperty("Values")!.GetValue(fallbackMetadata)!,
            (Delegate)fallbackMetadata.GetType().GetProperty("TextSelector")!.GetValue(fallbackMetadata)!);

        Assert.Equal(fallback.Values, generated.Values);
        Assert.Equal(fallback.Texts, generated.Texts);

        // Every declared member, including the aliases that are not listed, must map to the same text.
        foreach (var value in Enum.GetValues(enumType).Cast<Enum>().Concat((extraValues ?? []).Select(x => (Enum)Enum.ToObject(enumType, x))))
        {
            Assert.Equal(fallback.TextOf(value), generated.TextOf(value));
        }

        return generated;
    }

    private sealed class EnumMetadataSnapshot
    {
        private readonly Type _enumType;
        private readonly Delegate _textSelector;

        public EnumMetadataSnapshot(Type enumType, object values, Delegate textSelector)
        {
            _enumType = enumType;
            _textSelector = textSelector;

            Values = ((IEnumerable)values).Cast<Enum>().ToArray();
        }

        public IReadOnlyList<Enum> Values { get; }

        public IReadOnlyList<string> Texts => Values.Select(TextOf).ToArray();

        public string TextOf(Enum value) => (string)_textSelector.DynamicInvoke(value)!;

        public string TextOf(string memberName) => TextOf((Enum)Enum.Parse(_enumType, memberName));

        public string TextOf(long value) => TextOf((Enum)Enum.ToObject(_enumType, value));

        public void AssertValues(params string[] memberNames)
        {
            Assert.Equal(memberNames.Select(name => (Enum)Enum.Parse(_enumType, name)), Values);
        }
    }
}
