using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
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
    public void DisplayNamesWithSpecialCharacters_AreEscapedInGeneratedCode()
    {
        // The display names are C# escape sequences in the input, so the generator must re-escape the
        // resulting characters; a raw line break inside a regular string literal fails with CS1010.
        var metadata = AssertMatchesFallback("""
            public enum Sample
            {
                [Display(Name = "First\nSecond")]
                LineFeed,
                [Display(Name = "First\rSecond")]
                CarriageReturn,
                [Display(Name = "Quote \" and backslash \\")]
                QuoteAndBackslash
            }
            """);

        Assert.Equal(["First\nSecond", "First\rSecond", "Quote \" and backslash \\"], metadata.Texts);
    }

    [Fact]
    public void KeywordMemberNames_AreEscapedInGeneratedCode()
    {
        // "default" < "zulu" ordinally, so the verbatim @default becomes the alias representative and
        // must be emitted as @default; contextual keywords such as value need no escaping.
        var metadata = AssertMatchesFallback("""
            public enum Sample
            {
                zulu = 0,
                @default = 0,
                @class = 1,
                value = 2
            }
            """);

        metadata.AssertValues("default", "class", "value");
        Assert.Equal(["default", "class", "value"], metadata.Texts);
        Assert.Equal("default", metadata.TextOf("zulu"));
    }

    [Fact]
    public void ObsoleteMembers_AreNotReferencedByGeneratedCode()
    {
        // "Ancient" < "Current" ordinally, so the error-level obsolete alias is the representative; the
        // generated code must not reference it (CS0619 cannot be suppressed). Warning-level obsolete
        // members must not produce CS0612/CS0618 either, which would break warnings-as-errors builds.
        const string declaration = """
            public enum Sample
            {
                Current = 0,
                [Obsolete("Use Current", true)]
                Ancient = 0,
                [Obsolete]
                Legacy = 1,
                [Obsolete("Old", false), Display(Name = "Older")]
                Old = 2
            }
            """;

        var result = GeneratorTestHelper.Run(Usings + declaration, new EnumMetadataGenerator());

        // The input never references the obsolete members, so any obsolete diagnostic comes from generated code.
        Assert.NotNull(result.Source);
        Assert.DoesNotContain(result.OutputCompilation.GetDiagnostics(), diagnostic => diagnostic.Id is "CS0612" or "CS0618" or "CS0619");

        var metadata = AssertMatchesFallback(declaration);

        Assert.Equal(["Ancient", "Legacy", "Older"], metadata.Texts);
        Assert.Equal("Ancient", metadata.TextOf("Current"));
    }

    [Theory]
    [InlineData("sbyte", "Min = sbyte.MinValue, Max = sbyte.MaxValue")]
    [InlineData("int", "Min = int.MinValue, Max = int.MaxValue")]
    [InlineData("uint", "Min = uint.MinValue, Max = uint.MaxValue")]
    [InlineData("long", "Min = long.MinValue, Max = long.MaxValue")]
    [InlineData("ulong", "Min = ulong.MinValue, Max = ulong.MaxValue")]
    public void ExtremeUnderlyingValues_AreEmittedAsValidLiterals(string underlyingType, string members)
    {
        var metadata = AssertMatchesFallback("public enum Sample : " + underlyingType + " { " + members + " }");

        metadata.AssertValues("Min", "Max");
    }

    [Fact]
    public void ResourceDisplayNames_AreReadFromResourceTypeOnEveryCall()
    {
        const string declaration = """
            public static class Strings
            {
                public static string Low => System.Globalization.CultureInfo.CurrentUICulture.Name == "ja-JP" ? "低" : "Low priority";

                public static string? Missing => null;

                public static string @class => "Class";
            }

            public enum Sample
            {
                [Display(Name = nameof(Strings.Low), ResourceType = typeof(Strings))]
                Low,
                [Display(Name = nameof(Strings.Missing), ResourceType = typeof(Strings))]
                Other,
                [Display(Name = "class", ResourceType = typeof(Strings))]
                Keyword,
                [Display(Name = "Plain")]
                Plain
            }
            """;

        var previousCulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentUICulture = new CultureInfo("en-US");

            var metadata = AssertMatchesFallback(declaration);

            // A null resource value falls back to the member name, as DisplayAttribute.GetName returns null.
            Assert.Equal(["Low priority", "Other", "Class", "Plain"], metadata.Texts);

            // The same metadata follows a later change of the UI culture: names are not cached.
            CultureInfo.CurrentUICulture = new CultureInfo("ja-JP");

            Assert.Equal("低", metadata.TextOf("Low"));
            AssertMatchesFallback(declaration);
        }
        finally
        {
            CultureInfo.CurrentUICulture = previousCulture;
        }
    }

    // DisplayAttribute.GetName throws for these, so the generator leaves the enum to the fallback
    // rather than emitting code that would behave differently.
    [Theory]
    [InlineData("internal static class Strings { public static string Low => \"Low\"; }")]
    [InlineData("public static class Strings { internal static string Low => \"Low\"; }")]
    [InlineData("public static class Strings { public static string Low { private get => \"Low\"; set { } } }")]
    [InlineData("public static class Strings { public static int Low => 1; }")]
    [InlineData("public class Strings { public string Low => \"Low\"; }")]
    [InlineData("public static class Strings { }")]
    [InlineData("public static class Strings { [Obsolete(\"Old\", true)] public static string Low => \"Low\"; }")]
    [InlineData("public static class Strings { [Obsolete(\"Old\")] public static string Low => \"Low\"; }")]
    [InlineData("public static class Strings { private static string s_low = \"Low\"; public static ref string Low => ref s_low; }")]
    [InlineData("public static class Strings { private static string s_low = \"Low\"; public static ref readonly string Low => ref s_low; }")]
    [InlineData("public interface Strings { static abstract string Low { get; } }")]
    [InlineData("public interface Strings { static virtual string Low => \"Low\"; }")]
    public void UnusableResourceProperty_LeavesEnumToFallback(string resourceType)
    {
        var result = GeneratorTestHelper.Run(Usings + resourceType + """

            public enum Sample
            {
                [Display(Name = "Low", ResourceType = typeof(Strings))]
                Low
            }
            """, new EnumMetadataGenerator());

        Assert.Empty(result.GeneratorDiagnostics);
        Assert.Empty(result.CompileErrors);
        Assert.Null(result.Source);
    }

    // A resource type that is not a named type has no resource property, so GetName throws for it,
    // while a null ResourceType means no resource at all.
    [Theory]
    [InlineData("typeof(string[])", false)]
    [InlineData("typeof(int*)", false)]
    [InlineData("null", true)]
    public void NonNamedResourceType_LeavesEnumToFallback(string resourceType, bool generated)
    {
        var result = GeneratorTestHelper.Run(Usings + $$"""
            public enum Sample
            {
                [Display(Name = "Low", ResourceType = {{resourceType}})]
                Low
            }
            """, new EnumMetadataGenerator());

        Assert.Empty(result.GeneratorDiagnostics);
        Assert.Empty(result.CompileErrors);
        Assert.Equal(generated, result.Source is not null);
    }

    [Fact]
    public void EmptyEnum_GeneratesNothing()
    {
        var result = GeneratorTestHelper.Run(Usings + "public enum Sample { }", new EnumMetadataGenerator());

        Assert.Empty(result.GeneratorDiagnostics);
        Assert.Null(result.Source);
    }

    // The generated top-level class cannot name these enums, so they must be skipped (and left to the
    // runtime fallback) instead of breaking the consumer's build with CS0122, CS0246 or CS0400.
    [Theory]
    [InlineData("public class Outer { private enum Sample { A } }")]
    [InlineData("public class Outer { protected enum Sample { A } }")]
    [InlineData("public class Outer { private protected enum Sample { A } }")]
    [InlineData("public class Outer { private class Middle { public enum Sample { A } } }")]
    [InlineData("public class Outer<T> { public enum Sample { A } }")]
    [InlineData("public class Outer<T> { public class Middle { public enum Sample { A } } }")]
    [InlineData("file enum Sample { A }")]
    [InlineData("file class Outer { public enum Sample { A } }")]
    public void InaccessibleEnum_GeneratesNothing(string declaration)
    {
        var result = GeneratorTestHelper.Run(Usings + declaration, new EnumMetadataGenerator());

        Assert.Empty(result.GeneratorDiagnostics);
        Assert.Empty(result.CompileErrors);
        Assert.Null(result.Source);
    }

    [Fact]
    public void AccessibleNestedEnums_AreGenerated()
    {
        var (hintNames, errors) = RunAll(Usings + """
            public class Outer
            {
                public enum PublicSample { A }

                internal enum InternalSample { A }

                protected internal enum ProtectedInternalSample { A }

                internal class Middle
                {
                    public enum Sample { A }
                }
            }
            """);

        Assert.Empty(errors);
        Assert.Equal(4, hintNames.Length);

        foreach (var name in new[] { "TestApp.Outer+PublicSample.", "TestApp.Outer+InternalSample.", "TestApp.Outer+ProtectedInternalSample.", "TestApp.Outer+Middle+Sample." })
        {
            Assert.Contains(hintNames, hintName => hintName.StartsWith(name, StringComparison.Ordinal));
        }
    }

    [Fact]
    public void EnumsWithSimilarNames_GetDistinctHintNames()
    {
        // These all mapped to one hint name before ('.' replaced with '_'), or differ only in case, which
        // hint names ignore; AddSource then threw and the generator reported CS8785 instead of any output.
        var (hintNames, errors) = RunAll("""
            namespace A
            {
                public enum B_C { X }

                public class B { public enum C { X } }

                public enum Sample { X }

                public enum sample { X }
            }

            namespace A_B
            {
                public enum C { X }
            }
            """);

        Assert.Empty(errors);
        Assert.Equal(5, hintNames.Distinct(StringComparer.OrdinalIgnoreCase).Count());
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

    private static (string[] HintNames, Diagnostic[] Errors) RunAll(string source)
    {
        var compilation = GeneratorTestHelper.CreateCompilation(source);

        Assert.Empty(compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error));

        var driver = GeneratorTestHelper.CreateDriver(new EnumMetadataGenerator()).RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var generatorDiagnostics);

        Assert.Empty(generatorDiagnostics);

        return (
            driver.GetRunResult().Results.Single().GeneratedSources.Select(generated => generated.HintName).ToArray(),
            outputCompilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray());
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
