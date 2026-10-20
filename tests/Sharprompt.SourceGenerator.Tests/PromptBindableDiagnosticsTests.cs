using System.Globalization;
using System.Linq;

using Microsoft.CodeAnalysis;

using Xunit;

namespace Sharprompt.SourceGenerator.Tests;

// Invalid [PromptBindable] definitions that used to produce generated code that does not compile,
// throws at runtime, or silently ignores what was written. Each reports a diagnostic at the
// attribute or property involved, and the rest of the model still binds.
public class PromptBindableDiagnosticsTests
{
    private const string Usings = """
        using System.Collections.Generic;
        using System.ComponentModel;
        using System.ComponentModel.DataAnnotations;

        using Sharprompt;

        namespace TestApp;

        public enum MyEnum { A, B, C }

        """;

    [Theory]
    [InlineData("public static int[] Items => new[] { 1 };", "int[]")]
    [InlineData("public static string Items => \"ab\";", "string")]
    [InlineData("public static IEnumerable<object> Items => new object[] { \"a\" };", "System.Collections.Generic.IEnumerable<object>")]
    [InlineData("public static void Items() { }", "void")]
    public void MemberItemsWithIncompatibleType_ReportsDiagnosticAndIgnoresItems(string member, string memberType)
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
        Assert.Equal("SHARP006", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
        Assert.Equal("MemberItems(\"Items\")", result.GetLocationText(diagnostic));
        Assert.Contains($"'{memberType}'", diagnostic.GetMessage(CultureInfo.InvariantCulture));
        Assert.Contains("'IEnumerable<string>'", diagnostic.GetMessage(CultureInfo.InvariantCulture));
        Assert.Contains("model.Value = global::Sharprompt.Prompt.Input<string>(", source);
    }

    [Theory]
    [InlineData("public static List<string> Items => new() { \"a\" };", "string", "Select<string>")]
    [InlineData("public static IReadOnlyList<MyEnum> Items() => new[] { MyEnum.A };", "MyEnum?", "Select<global::TestApp.MyEnum>")]
    [InlineData("public static string[] Items => new[] { \"a\" };", "IList<string>", "MultiSelect<string>")]
    public void MemberItemsWithCompatibleType_GeneratesSelect(string member, string propertyType, string expected)
    {
        var result = GeneratorTestHelper.Run(Usings + $$"""
            [PromptBindable]
            public class Model
            {
                {{member}}

                [MemberItems("Items")]
                public {{propertyType}} Value { get; set; } = default!;
            }
            """);

        var source = result.AssertGeneratedAndCompiles();

        Assert.Empty(result.GeneratorDiagnostics);
        Assert.Contains(expected, source);
        Assert.Contains("options.Items = global::TestApp.Model.Items", source);
    }

    [Theory]
    [InlineData("int", "\"a\", \"b\"", "\"a\" of type 'string'")]
    [InlineData("long", "1, 2", "1 of type 'int'")]
    [InlineData("int", "1, null", "null")]
    [InlineData("MyEnum", "0", "0 of type 'int'")]
    [InlineData("List<int>", "1, \"b\"", "\"b\" of type 'string'")]
    [InlineData("List<int?>", "1L, 2L", "1 of type 'long'")]
    public void InlineItemsWithIncompatibleValue_ReportsDiagnosticAndIgnoresItems(string propertyType, string items, string invalidValue)
    {
        var result = GeneratorTestHelper.Run(Usings + $$"""
            [PromptBindable]
            public class Model
            {
                [InlineItems({{items}})]
                public {{propertyType}} Value { get; set; } = default!;
            }
            """);

        var source = result.AssertGeneratedAndCompiles();

        var diagnostic = Assert.Single(result.GeneratorDiagnostics);
        Assert.Equal("SHARP007", diagnostic.Id);
        Assert.Equal($"InlineItems({items})", result.GetLocationText(diagnostic));
        Assert.StartsWith($"The value {invalidValue} in [InlineItems]", diagnostic.GetMessage(CultureInfo.InvariantCulture));
        Assert.DoesNotContain("options.Items", source);
    }

    [Theory]
    [InlineData("long", "1L, 2L")]
    [InlineData("object", "1, \"a\"")]
    [InlineData("MyEnum?", "MyEnum.A, MyEnum.B")]
    [InlineData("HashSet<string>", "\"a\", \"b\"")]
    [InlineData("List<int?>", "1, 2")]
    [InlineData("int?[]", "1, 2")]
    public void InlineItemsWithCompatibleValues_GeneratesItems(string propertyType, string items)
    {
        var result = GeneratorTestHelper.Run(Usings + $$"""
            [PromptBindable]
            public class Model
            {
                [InlineItems({{items}})]
                public {{propertyType}} Value { get; set; } = default!;
            }
            """);

        var source = result.AssertGeneratedAndCompiles();

        Assert.Empty(result.GeneratorDiagnostics);
        Assert.Contains("options.Items = global::System.Linq.Enumerable.Cast<", source);
    }

    [Fact]
    public void InlineItemsAndMemberItems_ReportsDiagnosticAndUsesInlineItems()
    {
        var result = GeneratorTestHelper.Run(Usings + """
            [PromptBindable]
            public class Model
            {
                public static string[] Items => new[] { "a" };

                [InlineItems("x")]
                [MemberItems(nameof(Items))]
                public string Value { get; set; } = "";
            }
            """);

        var source = result.AssertGeneratedAndCompiles();

        var diagnostic = Assert.Single(result.GeneratorDiagnostics);
        Assert.Equal("SHARP008", diagnostic.Id);
        Assert.Equal("MemberItems(nameof(Items))", result.GetLocationText(diagnostic));
        Assert.Contains("[InlineItems] is also applied", diagnostic.GetMessage(CultureInfo.InvariantCulture));
        Assert.Contains("new object[] { \"x\" }", source);
        Assert.DoesNotContain("Model.Items", source);
    }

    [Theory]
    [InlineData("[InlineItems(true)]", "bool", "InlineItems(true)", "Confirm")]
    [InlineData("[MemberItems(nameof(Flags))]", "bool?", "MemberItems(nameof(Flags))", "Confirm")]
    [InlineData("[DataType(DataType.Password), InlineItems(\"a\")]", "string", "InlineItems(\"a\")", "Password")]
    public void ItemsOnConfirmOrPassword_ReportsDiagnostic(string attributes, string propertyType, string location, string prompt)
    {
        var result = GeneratorTestHelper.Run(Usings + $$"""
            [PromptBindable]
            public class Model
            {
                public static bool[] Flags => new[] { true };

                {{attributes}}
                public {{propertyType}} Value { get; set; } = default!;
            }
            """);

        var source = result.AssertGeneratedAndCompiles();

        var diagnostic = Assert.Single(result.GeneratorDiagnostics);
        Assert.Equal("SHARP008", diagnostic.Id);
        Assert.Equal(location, result.GetLocationText(diagnostic));
        Assert.Contains($"bound with a {prompt} prompt", diagnostic.GetMessage(CultureInfo.InvariantCulture));
        Assert.Contains($"global::Sharprompt.Prompt.{prompt}(", source);
    }

    [Theory]
    [InlineData("int")]
    [InlineData("string[]")]
    [InlineData("string?")]
    public void PasswordOnNonStringProperty_ReportsDiagnosticAndIsSkipped(string propertyType)
    {
        var result = GeneratorTestHelper.Run(Usings + $$"""
            [PromptBindable]
            public class Model
            {
                public string Name { get; set; } = "";

                [DataType(DataType.Password)]
                public {{propertyType}} Secret { get; set; } = default!;
            }
            """);

        var source = result.AssertGeneratedAndCompiles();

        if (propertyType == "string?")
        {
            // A nullable reference annotation does not change the type, so it is still a string.
            Assert.Empty(result.GeneratorDiagnostics);
            Assert.Contains("model.Secret = global::Sharprompt.Prompt.Password(", source);
            return;
        }

        var diagnostic = Assert.Single(result.GeneratorDiagnostics);
        Assert.Equal("SHARP009", diagnostic.Id);
        Assert.Equal("Secret", result.GetLocationText(diagnostic));
        Assert.DoesNotContain("model.Secret", source);
        Assert.Contains("model.Name =", source);
    }

    [Theory]
    [InlineData("public string Value { set { } }")]
    [InlineData("public string Value { private get; set; } = \"\";")]
    [InlineData("public string Value { protected get; set; } = \"\";")]
    public void PropertyWithoutAccessibleGetter_ReportsDiagnosticAndIsSkipped(string declaration)
    {
        var result = GeneratorTestHelper.Run(Usings + $$"""
            [PromptBindable]
            public class Model
            {
                public string Name { get; set; } = "";

                {{declaration}}
            }
            """);

        var source = result.AssertGeneratedAndCompiles();

        var diagnostic = Assert.Single(result.GeneratorDiagnostics);
        Assert.Equal("SHARP010", diagnostic.Id);
        Assert.Equal("Value", result.GetLocationText(diagnostic));
        Assert.DoesNotContain("model.Value", source);
        Assert.Contains("model.Name =", source);
    }

    [Theory]
    [InlineData("public string Secret { set { } }")]
    [InlineData("public string Secret { private get; set; } = \"\";")]
    public void PasswordPropertyWithoutAccessibleGetter_IsBound(string declaration)
    {
        // A Password prompt has no default value, so the binder never reads the property.
        var result = GeneratorTestHelper.Run(Usings + $$"""
            [PromptBindable]
            public class Model
            {
                [DataType(DataType.Password)]
                {{declaration}}
            }
            """);

        var source = result.AssertGeneratedAndCompiles();

        Assert.Empty(result.GeneratorDiagnostics);
        Assert.Contains("model.Secret = global::Sharprompt.Prompt.Password(", source);
    }

    [Fact]
    public void InlineItemsWithNullArray_ReportsDiagnosticAndIgnoresItems()
    {
        // [InlineItems(null)] passes a null params array, not an array containing null.
        var result = GeneratorTestHelper.Run(Usings + """
            [PromptBindable]
            public class Model
            {
                public string Name { get; set; } = "";

                [InlineItems(null!)]
                public string Value { get; set; } = "";
            }
            """);

        var source = result.AssertGeneratedAndCompiles();

        var diagnostic = Assert.Single(result.GeneratorDiagnostics);
        Assert.Equal("SHARP007", diagnostic.Id);
        Assert.StartsWith("The value null in [InlineItems]", diagnostic.GetMessage(CultureInfo.InvariantCulture));
        Assert.Contains("model.Name =", source);
        Assert.Contains("model.Value = global::Sharprompt.Prompt.Input<string>(", source);
    }

    [Theory]
    [InlineData("typeof(List<>)")]
    [InlineData("typeof(Dictionary<,>)")]
    [InlineData("new MyEnum[] { }")]
    public void ValidationAttributeWithAccessibleArguments_IsUsed(string argument)
    {
        var result = GeneratorTestHelper.Run(Usings + $$"""
            public sealed class CheckAttribute(object value) : ValidationAttribute
            {
                public object Value { get; } = value;
            }

            [PromptBindable]
            public class Model
            {
                [Check({{argument}})]
                public string Value { get; set; } = "";
            }
            """);

        var source = result.AssertGeneratedAndCompiles();

        Assert.Empty(result.GeneratorDiagnostics);
        Assert.Contains("new global::TestApp.CheckAttribute(", source);
    }

    [Fact]
    public void ValidationAttributeWithEmptyArrayOfInaccessibleType_ReportsDiagnostic()
    {
        var result = GeneratorTestHelper.Run(Usings + """
            public sealed class CheckAttribute(object value) : ValidationAttribute
            {
                public object Value { get; } = value;
            }

            [PromptBindable]
            public class Model
            {
                [Check(new Hidden[] { })]
                public string Value { get; set; } = "";

                private enum Hidden { A }
            }
            """);

        var source = result.AssertGeneratedAndCompiles();

        var diagnostic = Assert.Single(result.GeneratorDiagnostics);
        Assert.Equal("SHARP013", diagnostic.Id);
        Assert.DoesNotContain("Hidden", source);
    }

    [Theory]
    [InlineData("System.Span<char>")]
    [InlineData("System.ReadOnlySpan<int>")]
    public void RefStructProperty_ReportsDiagnosticAndIsSkipped(string propertyType)
    {
        var result = GeneratorTestHelper.Run(Usings + $$"""
            [PromptBindable]
            public class Model
            {
                public string Name { get; set; } = "";

                public {{propertyType}} Value { get => default; set { } }
            }
            """);

        var source = result.AssertGeneratedAndCompiles();

        var diagnostic = Assert.Single(result.GeneratorDiagnostics);
        Assert.Equal("SHARP011", diagnostic.Id);
        Assert.Equal("Value", result.GetLocationText(diagnostic));
        Assert.DoesNotContain("model.Value", source);
    }

    [Theory]
    [InlineData("object", "object")]
    [InlineData("Address", "TestApp.Address")]
    [InlineData("List<Address>", "TestApp.Address")]
    [InlineData("System.Numerics.BigInteger", "System.Numerics.BigInteger")]
    public void TypeWithoutConverter_ReportsDiagnosticAndStillBinds(string propertyType, string reportedType)
    {
        var result = GeneratorTestHelper.Run(Usings + $$"""
            public class Address;

            [PromptBindable]
            public class Model
            {
                public {{propertyType}} Value { get; set; } = default!;
            }
            """);

        var source = result.AssertGeneratedAndCompiles();

        var diagnostic = Assert.Single(result.GeneratorDiagnostics);
        Assert.Equal("SHARP012", diagnostic.Id);
        Assert.Equal("Value", result.GetLocationText(diagnostic));
        Assert.Contains($"type '{reportedType}'", diagnostic.GetMessage(CultureInfo.InvariantCulture));
        Assert.Contains("model.Value =", source);
    }

    [Theory]
    [InlineData("string")]
    [InlineData("int?")]
    [InlineData("List<int?>")]
    [InlineData("decimal")]
    [InlineData("System.DateOnly")]
    [InlineData("System.Guid")]
    [InlineData("System.Uri")]
    [InlineData("System.Version")]
    [InlineData("System.Int128")]
    [InlineData("MyEnum[]")]
    [InlineData("Temperature")]
    [InlineData("Celsius")]
    [InlineData("Temperature?")]
    [InlineData("System.Version?")]
    [InlineData("Degree?")]
    [InlineData("List<System.Uri?>")]
    public void TypeWithConverter_DoesNotReportDiagnostic(string propertyType)
    {
        var result = GeneratorTestHelper.Run(Usings + $$"""
            [TypeConverter(typeof(TypeConverter))]
            public struct Temperature;

            [TypeConverter("TestApp.SomeConverter, TestApp")]
            public class Degree;

            public class Celsius : Degree;

            [PromptBindable]
            public class Model
            {
                public {{propertyType}} Value { get; set; } = default!;
            }
            """);

        result.AssertGeneratedAndCompiles();

        Assert.Empty(result.GeneratorDiagnostics);
    }

    [Theory]
    [InlineData("[Check]", "TestApp.Model.CheckAttribute")]
    [InlineData("[Range(typeof(Hidden), \"1\", \"2\")]", "System.ComponentModel.DataAnnotations.RangeAttribute")]
    public void InaccessibleValidationAttribute_ReportsDiagnosticAndSkipsValidator(string attribute, string reportedAttribute)
    {
        var result = GeneratorTestHelper.Run(Usings + $$"""
            [PromptBindable]
            public class Model
            {
                [Required]
                {{attribute}}
                public string Value { get; set; } = "";

                private sealed class CheckAttribute : ValidationAttribute;

                private sealed class Hidden;
            }
            """);

        var source = result.AssertGeneratedAndCompiles();

        var diagnostic = Assert.Single(result.GeneratorDiagnostics);
        Assert.Equal("SHARP013", diagnostic.Id);
        Assert.Equal(attribute.Trim('[', ']'), result.GetLocationText(diagnostic));
        Assert.Contains($"'{reportedAttribute}'", diagnostic.GetMessage(CultureInfo.InvariantCulture));
        Assert.Contains("RequiredAttribute()", source);
        Assert.DoesNotContain("Hidden", source);
        Assert.DoesNotContain("CheckAttribute", source);
    }

    [Theory]
    [InlineData("public class Model { }")]
    [InlineData("public class Model { [BindIgnore] public string Name { get; set; } = \"\"; }")]
    [InlineData("public class Model { public string Name { get; } = \"\"; }")]
    [InlineData("public static class Model { public static string Name { get; set; } = \"\"; }")]
    public void TypeWithoutBindableProperties_ReportsDiagnosticAndGeneratesNothing(string declaration)
    {
        var result = GeneratorTestHelper.Run(Usings + "[PromptBindable]\n" + declaration);

        Assert.Empty(result.CompileErrors);
        Assert.Null(result.Source);

        var diagnostic = Assert.Single(result.GeneratorDiagnostics);
        Assert.Equal("SHARP014", diagnostic.Id);
        Assert.Equal("Model", result.GetLocationText(diagnostic));
        Assert.Contains("Prompt.Bind throws", diagnostic.GetMessage(CultureInfo.InvariantCulture));
    }

    [Fact]
    public void Diagnostics_HaveUniqueIdsAndWarningSeverity()
    {
        var descriptors = typeof(DiagnosticDescriptors)
            .GetFields()
            .Select(field => (DiagnosticDescriptor)field.GetValue(null)!)
            .ToArray();

        Assert.Equal(descriptors.Length, descriptors.Select(descriptor => descriptor.Id).Distinct().Count());
        Assert.All(descriptors, descriptor => Assert.Equal(DiagnosticSeverity.Warning, descriptor.DefaultSeverity));
    }
}
