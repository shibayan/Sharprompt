using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Xunit;

namespace Sharprompt.SourceGenerator.Tests;

public class DocumentationExamplesTests
{
    [Theory]
    [InlineData("README.md")]
    [InlineData("usage-guide.en.md")]
    [InlineData("usage-guide.ja.md")]
    public void ExamplesCompileWithSourceGenerators(string fileName)
    {
        var markdown = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Examples", fileName));
        var examples = Regex.Matches(markdown, @"(?m)^```csharp\r?\n([\s\S]*?)^```\s*$");

        Assert.NotEmpty(examples);

        foreach (Match example in examples)
        {
            // Match the implicit usings of dotnet new console. Library and other namespace
            // imports must be present in each example. Never run these interactive programs.
            var source = """
                global using System;
                global using System.Collections.Generic;
                global using System.IO;
                global using System.Linq;
                global using System.Net.Http;
                global using System.Threading;
                global using System.Threading.Tasks;

                """;
            var compilation = GeneratorTestHelper.CreateCompilation(source, example.Groups[1].Value)
                .WithOptions(new CSharpCompilationOptions(OutputKind.ConsoleApplication, nullableContextOptions: NullableContextOptions.Enable));
            var driver = CSharpGeneratorDriver.Create(
                [new PromptBindableGenerator().AsSourceGenerator(), new EnumMetadataGenerator().AsSourceGenerator()],
                parseOptions: GeneratorTestHelper.ParseOptions);

            driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var generatorDiagnostics);

            using var assembly = new MemoryStream();
            var result = output.Emit(assembly);
            var problems = result.Diagnostics.Concat(generatorDiagnostics)
                .Where(d => d.Severity is DiagnosticSeverity.Error or DiagnosticSeverity.Warning);

            Assert.True(result.Success && !problems.Any(), $"{fileName} example at offset {example.Index}:\n{example.Groups[1].Value}\n{string.Join(Environment.NewLine, problems)}");
        }
    }
}
