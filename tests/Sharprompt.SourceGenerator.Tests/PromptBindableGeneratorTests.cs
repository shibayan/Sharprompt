using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;

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
        Assert.Equal(new[] { "SHARP002", "SHARP002", "SHARP014" }, result.GeneratorDiagnostics.Select(d => d.Id));
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

    [Fact]
    public void Order_PutsPropertiesWithoutOrderAfterOrderMaxValue()
    {
        var result = GeneratorTestHelper.Run(Usings + """
            [PromptBindable]
            public class Model
            {
                public string Unordered { get; set; } = "";

                [Display(Order = int.MaxValue)]
                public string Max { get; set; } = "";

                [Display(Order = int.MinValue)]
                public string Min { get; set; } = "";
            }
            """);

        var source = result.AssertGeneratedAndCompiles();

        var order = new[] { "Min", "Max", "Unordered" }
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
    [InlineData("public static IEnumerable<string> Items { set { } }")]
    [InlineData("public static IEnumerable<string> Items { private get => new[] { \"a\" }; set { } }")]
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

    private const string ProviderTypes = """
        public static class GenericProvider<T>
        {
            public static IEnumerable<string> Items => new[] { "a" };
        }

        """;

    [Theory]
    [InlineData("Provider")]
    [InlineData("GenericProvider<Secret>")]
    [InlineData("GenericProvider<Secret[]>")]
    [InlineData("GenericProvider<List<Secret>>")]
    [InlineData("GenericProvider<>")]
    public void MemberItemsOnInaccessibleType_ReportsDiagnostic(string providerType)
    {
        var result = GeneratorTestHelper.Run(Usings + ProviderTypes + $$"""
            [PromptBindable]
            public class Model
            {
                private static class Provider
                {
                    public static IEnumerable<string> Items => new[] { "a" };
                }

                private class Secret
                {
                }

                [MemberItems("Items", typeof({{providerType}}))]
                public string Value { get; set; } = "";
            }
            """);

        var source = result.AssertGeneratedAndCompiles();

        var diagnostic = Assert.Single(result.GeneratorDiagnostics);
        Assert.Equal("SHARP001", diagnostic.Id);
        Assert.Contains("model.Value = global::Sharprompt.Prompt.Input<string>(", source);
    }

    [Theory]
    [InlineData("GenericProvider<int>", "global::TestApp.GenericProvider<int>")]
    [InlineData("GenericProvider<MyEnum[]>", "global::TestApp.GenericProvider<global::TestApp.MyEnum[]>")]
    [InlineData("GenericProvider<List<Model>>", "global::TestApp.GenericProvider<global::System.Collections.Generic.List<global::TestApp.Model>>")]
    public void MemberItemsOnConstructedGenericType_GeneratesSelect(string providerType, string expected)
    {
        var result = GeneratorTestHelper.Run(Usings + ProviderTypes + $$"""
            [PromptBindable]
            public class Model
            {
                [MemberItems("Items", typeof({{providerType}}))]
                public string Value { get; set; } = "";
            }
            """);

        var source = result.AssertGeneratedAndCompiles();

        Assert.Empty(result.GeneratorDiagnostics);
        Assert.Contains("options.Items = " + expected + ".Items;", source);
    }

    [Fact]
    public void Indexer_IsSkipped()
    {
        var result = GeneratorTestHelper.Run(Usings + """
            [PromptBindable]
            public class Model
            {
                public string Name { get; set; } = "";

                public string this[int index]
                {
                    get => "";
                    set { }
                }

                public string this[string key, int index]
                {
                    get => "";
                    set { }
                }
            }
            """);

        var source = result.AssertGeneratedAndCompiles();

        Assert.Empty(result.GeneratorDiagnostics);
        Assert.Contains("model.Name = global::Sharprompt.Prompt.Input<string>(", source);
        Assert.DoesNotContain("this[", source);
    }

    [Fact]
    public void IndexerOnly_ReportsNoBindablePropertiesAndGeneratesNothing()
    {
        var result = GeneratorTestHelper.Run(Usings + """
            [PromptBindable]
            public class Model
            {
                public string this[int index]
                {
                    get => "";
                    set { }
                }
            }
            """);

        Assert.Empty(result.CompileErrors);
        Assert.Equal("SHARP014", Assert.Single(result.GeneratorDiagnostics).Id);
        Assert.Null(result.Source);
    }

    [Fact]
    public void KeywordNamedMembers_AreEscaped()
    {
        var result = GeneratorTestHelper.Run(Usings + """
            public sealed class KeywordAttribute : ValidationAttribute
            {
                public int @for { get; set; }
            }

            [PromptBindable]
            public class Model
            {
                public static IEnumerable<string> @default => new[] { "a" };

                public static IEnumerable<string> @static() => new[] { "a" };

                [Required]
                [Keyword(@for = 1)]
                public string @class { get; set; } = "";

                public bool @event { get; set; }

                public List<int> @int { get; set; } = new();

                [MemberItems("default")]
                public string @base { get; set; } = "";

                [MemberItems("static")]
                public IEnumerable<string> @this { get; set; } = null!;

                public string var { get; set; } = "";
            }
            """);

        var source = result.AssertGeneratedAndCompiles();

        Assert.Empty(result.GeneratorDiagnostics);
        Assert.Contains("model.@class = global::Sharprompt.Prompt.Input<string>(", source);
        Assert.Contains("options.DefaultValue = model.@class;", source);
        Assert.Contains("options.Message = \"class\";", source);
        Assert.Contains("var __c = CreateValidationContext(model, \"class\", \"class\");", source);
        Assert.Contains("new global::TestApp.KeywordAttribute() { @for = 1 }", source);
        Assert.Contains("model.@event = global::Sharprompt.Prompt.Confirm(", source);
        Assert.Contains("model.@int = global::System.Linq.Enumerable.ToList(", source);
        Assert.Contains("((global::System.Collections.Generic.IEnumerable<int>?)model.@int)", source);
        Assert.Contains("options.Items = global::TestApp.Model.@default;", source);
        Assert.Contains("options.Items = global::TestApp.Model.@static();", source);
        Assert.Contains("model.@this = global::Sharprompt.Prompt.MultiSelect<string>(", source);

        // Contextual keywords are valid identifiers and are left as is.
        Assert.Contains("model.var = global::Sharprompt.Prompt.Input<string>(", source);
    }

    [Theory]
    [InlineData("public class Model<T>", "Model")]
    [InlineData("public class Model<TKey, TValue>", "Model")]
    [InlineData("public record Model<T>", "Model")]
    public void GenericType_ReportsDiagnosticAndGeneratesNothing(string declaration, string expectedLocation)
    {
        var result = GeneratorTestHelper.Run(Usings + $$"""
            [PromptBindable]
            {{declaration}}
            {
                public string Name { get; set; } = "";
            }
            """);

        Assert.Empty(result.CompileErrors);
        Assert.Null(result.Source);

        var diagnostic = Assert.Single(result.GeneratorDiagnostics);
        Assert.Equal("SHARP004", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
        Assert.Equal(expectedLocation, result.GetLocationText(diagnostic));
    }

    [Fact]
    public void TypeNestedInGenericType_ReportsDiagnosticAndGeneratesNothing()
    {
        var result = GeneratorTestHelper.Run(Usings + """
            public class Outer<T>
            {
                [PromptBindable]
                public class Model
                {
                    public string Name { get; set; } = "";
                }
            }
            """);

        Assert.Empty(result.CompileErrors);
        Assert.Null(result.Source);

        var diagnostic = Assert.Single(result.GeneratorDiagnostics);
        Assert.Equal("SHARP004", diagnostic.Id);
        Assert.Equal("Model", result.GetLocationText(diagnostic));
        Assert.Contains("'TestApp.Outer<T>.Model'", diagnostic.GetMessage(CultureInfo.InvariantCulture));
    }

    [Theory]
    [InlineData("public class Outer", "private")]
    [InlineData("public class Outer", "protected")]
    [InlineData("public class Outer", "private protected")]
    [InlineData("public class Container { private class Outer", "public")]
    [InlineData("public class Container { protected class Outer", "internal")]
    public void InaccessibleNestedType_ReportsDiagnosticAndGeneratesNothing(string outer, string modifier)
    {
        var close = outer.StartsWith("public class Container", System.StringComparison.Ordinal) ? "}}" : "}";

        var result = GeneratorTestHelper.Run(Usings + $$"""
            {{outer}}
            {
                [PromptBindable]
                {{modifier}} class Model
                {
                    public string Name { get; set; } = "";
                }
            {{close}}
            """);

        Assert.Empty(result.CompileErrors);
        Assert.Null(result.Source);

        var diagnostic = Assert.Single(result.GeneratorDiagnostics);
        Assert.Equal("SHARP005", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
        Assert.Equal("Model", result.GetLocationText(diagnostic));
    }

    [Fact]
    public void FileLocalType_ReportsDiagnosticAndGeneratesNothing()
    {
        var result = GeneratorTestHelper.Run(Usings + """
            [PromptBindable]
            file class Model
            {
                public string Name { get; set; } = "";
            }
            """);

        Assert.Empty(result.CompileErrors);
        Assert.Null(result.Source);

        var diagnostic = Assert.Single(result.GeneratorDiagnostics);
        Assert.Equal("SHARP005", diagnostic.Id);
        Assert.Equal("Model", result.GetLocationText(diagnostic));
    }

    [Theory]
    [InlineData("public")]
    [InlineData("internal")]
    [InlineData("protected internal")]
    public void AccessibleNestedType_GeneratesBinder(string modifier)
    {
        var result = GeneratorTestHelper.Run(Usings + $$"""
            internal class Outer
            {
                [PromptBindable]
                {{modifier}} class Model
                {
                    public string Name { get; set; } = "";
                }
            }
            """);

        var source = result.AssertGeneratedAndCompiles();

        Assert.Empty(result.GeneratorDiagnostics);
        Assert.Contains("global::Sharprompt.ModelBinderRegistry.Register<global::TestApp.Outer.Model>(", source);
    }

    [Fact]
    public void TrimAnalyzer_ReportsReflectionBasedValidationContext()
    {
        // Guards the IL2026 checks in AssertGeneratedAndCompiles: the analyzer must actually be running,
        // and it must analyze generated code, which it skips by default.
        var compilation = GeneratorTestHelper.CreateCompilation("""
            // <auto-generated/>
            public static class Sample
            {
                public static object Create(object model) => new System.ComponentModel.DataAnnotations.ValidationContext(model);
            }
            """);

        var diagnostics = GeneratorTestHelper.GetTrimAnalyzerDiagnostics(compilation);

        Assert.Contains(diagnostics, diagnostic => diagnostic.Id == "IL2026");
    }

    [Fact]
    public void LengthValidators_OnStringsAndCollections_AreTrimSafe()
    {
        // MinLength, MaxLength and Length constructors are [RequiresUnreferencedCode] because they read Count
        // through reflection for values that are neither strings nor ICollection. These values never are, so
        // the generated binder must not raise IL2026 for them (AssertGeneratedAndCompiles checks).
        var result = GeneratorTestHelper.Run(Usings + """
            [PromptBindable]
            public class Model
            {
                [MinLength(3)]
                [MaxLength(10)]
                public string Name { get; set; } = "";

                [MaxLength]
                [DataType(DataType.Password)]
                public string Secret { get; set; } = "";

                [Length(1, 5)]
                public string? Nickname { get; set; }

                [MinLength(1)]
                public List<string> Tags { get; set; } = new();

                [MaxLength(2)]
                public string[][] Groups { get; set; } = [];

                // The element type is ICollection itself, which is not among its own AllInterfaces.
                [MinLength(1)]
                public List<System.Collections.ICollection> Collections { get; set; } = new();
            }
            """);

        var source = result.AssertGeneratedAndCompiles();

        Assert.Contains("new global::System.ComponentModel.DataAnnotations.MinLengthAttribute(3)", source);
        Assert.Contains("new global::System.ComponentModel.DataAnnotations.LengthAttribute(1, 5)", source);
    }

    [Theory]
    [InlineData("[MinLength(1)] public Countable Value { get; set; } = new();", "MinLengthAttribute")]
    [InlineData("[Compare(nameof(Other))] public string Value { get; set; } = \"\"; public string Other { get; set; } = \"\";", "CompareAttribute")]
    [InlineData("[Range(typeof(decimal), \"1\", \"2\")] public decimal Value { get; set; }", "RangeAttribute")]
    public void ValidatorsThatReallyUseReflection_KeepTheirWarning(string declaration, string attribute)
    {
        // These do use reflection for the values they get, so the warning must reach the consumer.
        var result = GeneratorTestHelper.Run(Usings + $$"""
            [System.ComponentModel.TypeConverter(typeof(System.ComponentModel.TypeConverter))]
            public class Countable
            {
                public int Count => 0;
            }

            [PromptBindable]
            public class Model
            {
                {{declaration}}
            }
            """);

        Assert.Empty(result.CompileErrors);

        var generatedTree = result.OutputCompilation.SyntaxTrees.Last();
        var diagnostics = GeneratorTestHelper.GetTrimAnalyzerDiagnostics(result.OutputCompilation).Where(d => d.Location.SourceTree == generatedTree);

        Assert.Contains(diagnostics, diagnostic => diagnostic.Id == "IL2026" && diagnostic.GetMessage(CultureInfo.InvariantCulture).Contains(attribute));
    }

    [Fact]
    public void Validators_UseTrimSafeValidationContextWithDisplayName()
    {
        var result = GeneratorTestHelper.Run(Usings + """
            [PromptBindable]
            public class Model
            {
                [Required]
                [Display(Name = "User name", Description = "Enter your name")]
                public string UserName { get; set; } = "";

                [Required]
                [Display(Description = "Only a description")]
                public string Other { get; set; } = "";

                [Required]
                public List<string> Tags { get; set; } = new();
            }
            """);

        var source = result.AssertGeneratedAndCompiles();

        Assert.Empty(result.GeneratorDiagnostics);
        Assert.Contains("var __c = CreateValidationContext(model, \"User name\", \"UserName\");", source);
        Assert.Contains("var __c = CreateValidationContext(model, \"Other\", \"Other\");", source);
        Assert.Contains("var __c = CreateValidationContext(model, \"Tags\", \"Tags\");", source);

#if NET10_0_OR_GREATER
        // The trim-safe constructor is used when the target framework has it.
        Assert.Contains("return new global::System.ComponentModel.DataAnnotations.ValidationContext(model, displayName, null, null) { MemberName = memberName };", source);
        Assert.DoesNotContain("UnconditionalSuppressMessage", source);
#else
        // Otherwise the display name is set up front, so the reflection-based lookup never runs.
        Assert.Contains("return new global::System.ComponentModel.DataAnnotations.ValidationContext(model) { DisplayName = displayName, MemberName = memberName };", source);
        Assert.Contains("UnconditionalSuppressMessage(\"Trimming\", \"IL2026:RequiresUnreferencedCode\"", source);
#endif

        // Validate through the generated factory, to check the message uses the display name.
        var assembly = result.LoadAssembly();
        var binderType = assembly.GetTypes().Single(type => type.Name.Contains("__SharpromptBinder_", System.StringComparison.Ordinal));
        var createValidationContext = binderType.GetMethod("CreateValidationContext", BindingFlags.NonPublic | BindingFlags.Static)!;
        var model = System.Activator.CreateInstance(assembly.GetType("TestApp.Model", throwOnError: true)!)!;
        var context = (ValidationContext)createValidationContext.Invoke(null, [model, "User name", "UserName"])!;

        Assert.Equal("User name", context.DisplayName);
        Assert.Equal("UserName", context.MemberName);
        // The default message is localized with CurrentUICulture, so build the expectation the same way.
        var required = new RequiredAttribute();
        Assert.Equal(required.FormatErrorMessage("User name"), required.GetValidationResult(null, context)!.ErrorMessage);
    }

    [Fact]
    public void Validators_FallBackToPropertyNameForEmptyDisplayName()
    {
        var result = GeneratorTestHelper.Run(Usings + """
            [PromptBindable]
            public class Model
            {
                [Required]
                [Display(Name = "")]
                public string Value { get; set; } = "";
            }
            """);

        var source = result.AssertGeneratedAndCompiles();

        Assert.Contains("var __c = CreateValidationContext(model, \"Value\", \"Value\");", source);
    }

    [Fact]
    public void NoValidators_DoesNotEmitValidationContextFactory()
    {
        var result = GeneratorTestHelper.Run(Usings + """
            [PromptBindable]
            public class Model
            {
                public string Value { get; set; } = "";
            }
            """);

        var source = result.AssertGeneratedAndCompiles();

        Assert.DoesNotContain("CreateValidationContext", source);
    }

    [Theory]
    [InlineData("byte", "(byte)1, (byte)255", "(byte)1, (byte)255", "1,255")]
    [InlineData("sbyte", "(sbyte)-1, (sbyte)2", "(sbyte)-1, (sbyte)2", "-1,2")]
    [InlineData("short", "(short)-1, (short)2", "(short)-1, (short)2", "-1,2")]
    [InlineData("ushort", "(ushort)1, (ushort)65535", "(ushort)1, (ushort)65535", "1,65535")]
    [InlineData("uint", "1U, 4294967295U", "1U, 4294967295U", "1,4294967295")]
    [InlineData("int", "-1, 2", "-1, 2", "-1,2")]
    [InlineData("long", "-1L, 2L", "-1L, 2L", "-1,2")]
    [InlineData("ulong", "1UL, 2UL", "1UL, 2UL", "1,2")]
    public void InlineItemsWithIntegralValues_KeepTheirType(string type, string items, string expected, string expectedValues)
    {
        var result = GeneratorTestHelper.Run(Usings + $$"""
            [PromptBindable]
            public class Model
            {
                [InlineItems({{items}})]
                public {{type}} Value { get; set; }

                [InlineItems({{items}})]
                public List<{{type}}> Values { get; set; } = new();
            }
            """);

        var source = result.AssertGeneratedAndCompiles();

        Assert.Empty(result.GeneratorDiagnostics);
        Assert.Contains("(new object[] { " + expected + " })", source);

        // Enumerable.Cast unboxes each item, which throws InvalidCastException if the boxed type differs.
        var itemsExpression = source.Split('\n')
            .Select(line => line.Trim())
            .First(line => line.StartsWith("options.Items = ", System.StringComparison.Ordinal))
            ["options.Items = ".Length..^1];

        var values = result.Evaluate("return global::System.Linq.Enumerable.ToArray(" + itemsExpression + ");");

        var array = Assert.IsAssignableFrom<System.Array>(values);
        Assert.Equal(expectedValues, string.Join(",", array.Cast<object>().Select(value => System.Convert.ToString(value, CultureInfo.InvariantCulture))));
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

        // Only the diagnostics output depends on the compilation (to bind locations to the new tree);
        // the generated source must stay cached.
        Assert.All(
            secondRun.TrackedSteps[PromptBindableGenerator.SourceTrackingName].SelectMany(step => step.Outputs),
            output => Assert.Equal(IncrementalStepRunReason.Cached, output.Reason));
        Assert.All(secondRun.Diagnostics, diagnostic => Assert.Same(newTree, diagnostic.Location.SourceTree));
    }

    [Fact]
    public void Diagnostics_AreBoundToSyntaxTree_SoPragmaSuppressionApplies()
    {
        const string source = Usings + """
            [PromptBindable]
            public class Model
            {
            #pragma warning disable SHARP002
                public string Id { get; init; } = "";
            #pragma warning restore SHARP002

                public string Name { get; init; } = "";

                public string Other { get; set; } = "";
            }
            """;

        var compilation = GeneratorTestHelper.CreateCompilation(source);
        var tree = compilation.SyntaxTrees.Single();

        GeneratorTestHelper.CreateDriver().RunGeneratorsAndUpdateCompilation(compilation, out _, out var diagnostics);

        // GeneratorDriver returns diagnostics before suppression; the compiler then filters them by
        // looking up the #pragma state in Location.SourceTree. A file-path location has no SourceTree,
        // so it could never be suppressed. Binding to the input tree is what makes suppression work.
        Assert.Equal(2, diagnostics.Length);
        Assert.All(diagnostics, diagnostic => Assert.Same(tree, diagnostic.Location.SourceTree));
        Assert.Equal(
            new[] { "Id", "Name" },
            diagnostics.Select(diagnostic => tree.GetText().ToString(diagnostic.Location.SourceSpan)).Order());
    }
}
