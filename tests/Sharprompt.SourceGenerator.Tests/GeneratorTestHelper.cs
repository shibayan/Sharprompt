using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

using Xunit;

namespace Sharprompt.SourceGenerator.Tests;

internal static class GeneratorTestHelper
{
    private static readonly CSharpParseOptions s_parseOptions = new(LanguageVersion.Latest);

    private static readonly Lazy<MetadataReference[]> s_references = new(() =>
        ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator)
            .Append(typeof(Prompt).Assembly.Location)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(path => (MetadataReference)MetadataReference.CreateFromFile(path))
            .ToArray());

    public static CSharpCompilation CreateCompilation(params string[] sources)
    {
        return CSharpCompilation.Create(
            "GeneratorTest",
            sources.Select(source => CSharpSyntaxTree.ParseText(source, s_parseOptions, path: "Model.cs")),
            s_references.Value,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
    }

    public static GeneratorDriver CreateDriver()
    {
        return CSharpGeneratorDriver.Create(
            [new PromptBindableGenerator().AsSourceGenerator()],
            parseOptions: s_parseOptions,
            driverOptions: new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true));
    }

    public static GeneratorTestResult Run(string source)
    {
        var compilation = CreateCompilation(source);

        // The test input itself must be valid, so that any error comes from the generated code.
        Assert.Empty(compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error));

        CreateDriver().RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var generatorDiagnostics);

        var generatedTree = outputCompilation.SyntaxTrees.Skip(1).SingleOrDefault();

        return new GeneratorTestResult(
            compilation.SyntaxTrees.Single().GetText(),
            generatedTree?.ToString(),
            generatorDiagnostics,
            outputCompilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToImmutableArray());
    }
}

internal sealed record GeneratorTestResult(SourceText Input, string? Source, ImmutableArray<Diagnostic> GeneratorDiagnostics, ImmutableArray<Diagnostic> CompileErrors)
{
    public string AssertGeneratedAndCompiles()
    {
        Assert.Empty(CompileErrors);
        Assert.NotNull(Source);

        return Source;
    }

    // Generator diagnostics carry a file location (path + span) rather than a syntax tree, so resolve it against the input text.
    public string GetLocationText(Diagnostic diagnostic)
    {
        Assert.Equal("Model.cs", diagnostic.Location.GetLineSpan().Path);

        return Input.ToString(diagnostic.Location.SourceSpan);
    }
}
