using System.Globalization;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

using Xunit;

namespace Sharprompt.SourceGenerator.Tests;

public class PromptBindableGeneratorTests
{
    private const string Usings = """
        using System.Collections.Generic;
        using System.ComponentModel.DataAnnotations;

        using Sharprompt;

        namespace TestApp;

        public enum MyEnum { A, B, C }

        """;

    [Fact]
    public void Class_GeneratesBinderWithCancellationToken()
    {
        var result = GeneratorTestHelper.Run(Usings + """
            [PromptBindable]
            public class Model
            {
                public string Name { get; set; } = "";
            }
            """);

        var source = result.AssertGeneratedAndCompiles();

        Assert.Empty(result.GeneratorDiagnostics);
        Assert.Contains("global::Sharprompt.ModelBinderRegistry.Register<global::TestApp.Model>((model, cancellationToken) =>", source);
        Assert.Contains("model.Name = global::Sharprompt.Prompt.Input<string>(options =>", source);
        Assert.Contains("}, cancellationToken);", source);
    }

    [Theory]
    [InlineData("record")]
    [InlineData("record class")]
    [InlineData("sealed record")]
    public void Record_GeneratesBinder(string keyword)
    {
        var result = GeneratorTestHelper.Run(Usings + $$"""
            [PromptBindable]
            public {{keyword}} Model
            {
                public string Name { get; set; } = "";
            }
            """);

        var source = result.AssertGeneratedAndCompiles();

        Assert.Empty(result.GeneratorDiagnostics);
        Assert.Contains("global::Sharprompt.ModelBinderRegistry.Register<global::TestApp.Model>(", source);
        Assert.Contains("model.Name = global::Sharprompt.Prompt.Input<string>(", source);
        Assert.DoesNotContain("EqualityContract", source);
    }

    [Theory]
    [InlineData("IEnumerable<string>", "model.Values = global::Sharprompt.Prompt.List<string>(")]
    [InlineData("string[]", "model.Values = global::System.Linq.Enumerable.ToArray(global::Sharprompt.Prompt.List<string>(")]
    [InlineData("List<int>", "model.Values = global::System.Linq.Enumerable.ToList(global::Sharprompt.Prompt.List<int>(")]
    [InlineData("IList<int>", "model.Values = global::System.Linq.Enumerable.ToList(global::Sharprompt.Prompt.List<int>(")]
    [InlineData("ICollection<int>", "model.Values = global::System.Linq.Enumerable.ToList(global::Sharprompt.Prompt.List<int>(")]
    [InlineData("IReadOnlyList<int>", "model.Values = global::System.Linq.Enumerable.ToList(global::Sharprompt.Prompt.List<int>(")]
    [InlineData("IReadOnlyCollection<int>", "model.Values = global::System.Linq.Enumerable.ToList(global::Sharprompt.Prompt.List<int>(")]
    [InlineData("HashSet<string>", "model.Values = new global::System.Collections.Generic.HashSet<string>(global::Sharprompt.Prompt.List<string>(")]
    [InlineData("ISet<string>", "model.Values = new global::System.Collections.Generic.HashSet<string>(global::Sharprompt.Prompt.List<string>(")]
    [InlineData("List<string>?", "model.Values = global::System.Linq.Enumerable.ToList(global::Sharprompt.Prompt.List<string>(")]
    [InlineData("IEnumerable<MyEnum>", "model.Values = global::Sharprompt.Prompt.MultiSelect<global::TestApp.MyEnum>(")]
    [InlineData("MyEnum[]", "model.Values = global::System.Linq.Enumerable.ToArray(global::Sharprompt.Prompt.MultiSelect<global::TestApp.MyEnum>(")]
    [InlineData("List<MyEnum>", "model.Values = global::System.Linq.Enumerable.ToList(global::Sharprompt.Prompt.MultiSelect<global::TestApp.MyEnum>(")]
    public void CollectionProperty_IsConvertedToPropertyType(string propertyType, string expected)
    {
        var result = GeneratorTestHelper.Run(Usings + $$"""
            [PromptBindable]
            public class Model
            {
                public {{propertyType}} Values { get; set; } = null!;
            }
            """);

        var source = result.AssertGeneratedAndCompiles();

        Assert.Empty(result.GeneratorDiagnostics);
        Assert.Contains(expected, source);
    }

    [Theory]
    [InlineData("List<string>", "MultiSelect<string>")]
    [InlineData("string[]", "MultiSelect<string>")]
    [InlineData("HashSet<string>", "MultiSelect<string>")]
    public void CollectionPropertyWithItems_IsConvertedToPropertyType(string propertyType, string expected)
    {
        var result = GeneratorTestHelper.Run(Usings + $$"""
            [PromptBindable]
            public class Model
            {
                [InlineItems("a", "b", "c")]
                public {{propertyType}} Values { get; set; } = null!;
            }
            """);

        var source = result.AssertGeneratedAndCompiles();

        Assert.Empty(result.GeneratorDiagnostics);
        Assert.Contains(expected, source);
        Assert.Contains("options.DefaultValues = ((global::System.Collections.Generic.IEnumerable<string>?)model.Values) ?? global::System.Array.Empty<string>();", source);
    }

    [Theory]
    [InlineData("System.Collections.Immutable.ImmutableArray<int>")]
    [InlineData("System.Collections.ObjectModel.Collection<int>")]
    [InlineData("Dictionary<string, int>")]
    [InlineData("int[,]")]
    public void UnsupportedCollectionProperty_ReportsDiagnosticAndIsSkipped(string propertyType)
    {
        var result = GeneratorTestHelper.Run(Usings + $$"""
            [PromptBindable]
            public class Model
            {
                public string Name { get; set; } = "";

                public {{propertyType}} Values { get; set; } = default!;
            }
            """);

        var source = result.AssertGeneratedAndCompiles();

        var diagnostic = Assert.Single(result.GeneratorDiagnostics);
        Assert.Equal("SHARP003", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
        Assert.Equal("Values", result.GetLocationText(diagnostic));
        Assert.Contains("model.Name =", source);
        Assert.DoesNotContain("model.Values", source);
    }

    [Fact]
    public void InitOnlyProperty_ReportsDiagnosticAndIsSkipped()
    {
        var result = GeneratorTestHelper.Run(Usings + """
            [PromptBindable]
            public class Model
            {
                public string Name { get; init; } = "";

                public string Other { get; set; } = "";

                [BindIgnore]
                public string Ignored { get; init; } = "";

                public string ReadOnly { get; } = "";
            }
            """);

        var source = result.AssertGeneratedAndCompiles();

        var diagnostic = Assert.Single(result.GeneratorDiagnostics);
        Assert.Equal("SHARP002", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
        Assert.Equal("Name", result.GetLocationText(diagnostic));
        Assert.DoesNotContain("model.Name", source);
        Assert.DoesNotContain("model.Ignored", source);
        Assert.DoesNotContain("model.ReadOnly", source);
        Assert.Contains("model.Other =", source);
    }

    [Fact]
    public void PositionalRecord_ReportsDiagnosticForEachParameter()
    {
        var result = GeneratorTestHelper.Run(Usings + """
            [PromptBindable]
            public record Model(string Name, int Age);
            """);

        Assert.Empty(result.CompileErrors);
        Assert.Null(result.Source);
        Assert.Equal(new[] { "SHARP002", "SHARP002" }, result.GeneratorDiagnostics.Select(d => d.Id));
    }

    [Fact]
    public void Order_PutsPropertiesWithoutOrderLastInDeclarationOrder()
    {
        var result = GeneratorTestHelper.Run(Usings + """
            [PromptBindable]
            public class Model
            {
                public string First { get; set; } = "";

                [Display(Order = 2)]
                public string Second { get; set; } = "";

                public string Third { get; set; } = "";

                [Display(Order = 1)]
                public string Fourth { get; set; } = "";

                [Display(Name = "No order")]
                public string Fifth { get; set; } = "";
            }
            """);

        var source = result.AssertGeneratedAndCompiles();

        var order = new[] { "Fourth", "Second", "First", "Third", "Fifth" }
            .Select(name => source.IndexOf("model." + name + " =", System.StringComparison.Ordinal))
            .ToArray();

        Assert.DoesNotContain(-1, order);
        Assert.Equal(order.OrderBy(x => x), order);
    }

    [Theory]
    [InlineData("de-DE")]
    [InlineData("fr-FR")]
    [InlineData("sv-SE")]
    [InlineData("ar-SA")]
    public void NumericAttributeArguments_AreFormattedIndependentOfCulture(string cultureName)
    {
        var originalCulture = CultureInfo.CurrentCulture;
        var originalUICulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = new CultureInfo(cultureName);

            var result = GeneratorTestHelper.Run(Usings + """
                public sealed class FloatRangeAttribute : ValidationAttribute
                {
                    public FloatRangeAttribute(float minimum, float maximum, long step) { }
                }

                [PromptBindable]
                public class Model
                {
                    [Range(-0.5, 0.30000000000000004)]
                    public double Ratio { get; set; }

                    [Range(-1, 1000000)]
                    public int Count { get; set; }

                    [FloatRange(-1.25f, 2.5f, -3L)]
                    public float Scale { get; set; }
                }
                """);

            var source = result.AssertGeneratedAndCompiles();

            Assert.Empty(result.GeneratorDiagnostics);
            Assert.Contains("new global::System.ComponentModel.DataAnnotations.RangeAttribute(-0.5d, 0.30000000000000004d)", source);
            Assert.Contains("new global::System.ComponentModel.DataAnnotations.RangeAttribute(-1, 1000000)", source);
            Assert.Contains("new global::TestApp.FloatRangeAttribute(-1.25f, 2.5f, -3L)", source);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUICulture;
        }
    }

    [Fact]
    public void NonFiniteAttributeArguments_AreEmittedAsNamedConstants()
    {
        var result = GeneratorTestHelper.Run(Usings + """
            public sealed class FloatRangeAttribute : ValidationAttribute
            {
                public FloatRangeAttribute(float minimum, float maximum) { }
            }

            [PromptBindable]
            public class Model
            {
                [Range(double.NegativeInfinity, double.PositiveInfinity)]
                public double Ratio { get; set; }

                [Range(double.NaN, 1.0)]
                public double Other { get; set; }

                [FloatRange(float.NaN, float.PositiveInfinity)]
                public float Scale { get; set; }
            }
            """);

        var source = result.AssertGeneratedAndCompiles();

        Assert.Contains("RangeAttribute(double.NegativeInfinity, double.PositiveInfinity)", source);
        Assert.Contains("RangeAttribute(double.NaN, 1d)", source);
        Assert.Contains("FloatRangeAttribute(float.NaN, float.PositiveInfinity)", source);
    }

    [Theory]
    [InlineData("")]
    [InlineData("private static IEnumerable<string> Items => new[] { \"a\" };")]
    [InlineData("public IEnumerable<string> Items => new[] { \"a\" };")]
    [InlineData("public static IEnumerable<string> Items(int count) => new[] { \"a\" };")]
    [InlineData("public static readonly string[] Items = { \"a\" };")]
    public void MemberItemsWithoutUsableMember_ReportsDiagnostic(string member)
    {
        var result = GeneratorTestHelper.Run(Usings + $$"""
            [PromptBindable]
            public class Model
            {
                {{member}}

                [MemberItems("Items")]
                public string Value { get; set; } = "";
            }
            """);

        var source = result.AssertGeneratedAndCompiles();

        var diagnostic = Assert.Single(result.GeneratorDiagnostics);
        Assert.Equal("SHARP001", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
        Assert.Equal("MemberItems(\"Items\")", result.GetLocationText(diagnostic));
        Assert.Contains("'Items'", diagnostic.GetMessage(CultureInfo.InvariantCulture));
        Assert.Contains("'TestApp.Model'", diagnostic.GetMessage(CultureInfo.InvariantCulture));
        Assert.Contains("model.Value = global::Sharprompt.Prompt.Input<string>(", source);
    }

    [Fact]
    public void MemberItemsOnOtherTypeWithoutUsableMember_ReportsDiagnostic()
    {
        var result = GeneratorTestHelper.Run(Usings + """
            public static class Provider
            {
                internal static IEnumerable<string> Items => new[] { "a" };
            }

            [PromptBindable]
            public class Model
            {
                [MemberItems("Items", typeof(Provider))]
                public string Value { get; set; } = "";
            }
            """);

        result.AssertGeneratedAndCompiles();

        var diagnostic = Assert.Single(result.GeneratorDiagnostics);
        Assert.Equal("SHARP001", diagnostic.Id);
        Assert.Contains("'TestApp.Provider'", diagnostic.GetMessage(CultureInfo.InvariantCulture));
    }

    [Theory]
    [InlineData("public static IEnumerable<string> Items => new[] { \"a\" };", "options.Items = global::TestApp.Model.Items;")]
    [InlineData("public static IEnumerable<string> Items() => new[] { \"a\" };", "options.Items = global::TestApp.Model.Items();")]
    public void MemberItemsWithUsableMember_GeneratesSelect(string member, string expected)
    {
        var result = GeneratorTestHelper.Run(Usings + $$"""
            [PromptBindable]
            public class Model
            {
                {{member}}

                [MemberItems("Items")]
                public string Value { get; set; } = "";
            }
            """);

        var source = result.AssertGeneratedAndCompiles();

        Assert.Empty(result.GeneratorDiagnostics);
        Assert.Contains("model.Value = global::Sharprompt.Prompt.Select<string>(", source);
        Assert.Contains(expected, source);
    }

    [Fact]
    public void UnrelatedEdit_KeepsPipelineOutputCached()
    {
        const string source = Usings + """
            [PromptBindable]
            public class Model
            {
                public string Name { get; set; } = "";

                public string Id { get; init; } = "";

                [MemberItems("Missing")]
                public string Value { get; set; } = "";
            }
            """;

        var compilation = GeneratorTestHelper.CreateCompilation(source);
        var driver = GeneratorTestHelper.CreateDriver().RunGenerators(compilation);

        var firstRun = driver.GetRunResult().Results.Single();
        Assert.Equal(2, firstRun.Diagnostics.Length);
        Assert.Single(firstRun.GeneratedSources);

        // Appending trivia re-runs the transform for the edited tree, but the result must compare equal.
        var oldTree = compilation.SyntaxTrees.Single();
        var newTree = oldTree.WithChangedText(oldTree.GetText().WithChanges(new TextChange(new TextSpan(oldTree.Length, 0), "\n// edited\n")));

        driver = driver.RunGenerators(compilation.ReplaceSyntaxTree(oldTree, newTree));

        var secondRun = driver.GetRunResult().Results.Single();

        Assert.Equal(firstRun.Diagnostics.Select(d => d.ToString()), secondRun.Diagnostics.Select(d => d.ToString()));
        Assert.All(
            secondRun.TrackedSteps[PromptBindableGenerator.TransformTrackingName].SelectMany(step => step.Outputs),
            output => Assert.Equal(IncrementalStepRunReason.Unchanged, output.Reason));
        Assert.All(
            secondRun.TrackedOutputSteps.SelectMany(pair => pair.Value).SelectMany(step => step.Outputs),
            output => Assert.Equal(IncrementalStepRunReason.Cached, output.Reason));
    }
}
