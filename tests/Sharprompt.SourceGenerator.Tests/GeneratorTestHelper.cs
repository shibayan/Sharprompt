using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

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

    public static GeneratorDriver CreateDriver(IIncrementalGenerator? generator = null)
    {
        return CSharpGeneratorDriver.Create(
            [(generator ?? new PromptBindableGenerator()).AsSourceGenerator()],
            parseOptions: s_parseOptions,
            driverOptions: new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true));
    }

    public static GeneratorTestResult Run(string source, IIncrementalGenerator? generator = null)
    {
        var compilation = CreateCompilation(source);

        // The test input itself must be valid, so that any error comes from the generated code.
        Assert.Empty(compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error));

        CreateDriver(generator).RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var generatorDiagnostics);

        var generatedTree = outputCompilation.SyntaxTrees.Skip(1).SingleOrDefault();

        return new GeneratorTestResult(
            compilation.SyntaxTrees.Single().GetText(),
            generatedTree?.ToString(),
            generatorDiagnostics,
            outputCompilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToImmutableArray(),
            outputCompilation);
    }
}

internal sealed record GeneratorTestResult(SourceText Input, string? Source, ImmutableArray<Diagnostic> GeneratorDiagnostics, ImmutableArray<Diagnostic> CompileErrors, Compilation OutputCompilation)
{
    // Loads the generated code into its own context, so that each test gets distinct types even
    // though every test compilation shares the same assembly name.
    public Assembly LoadAssembly()
    {
        AssertGeneratedAndCompiles();

        using var stream = new MemoryStream();

        var emitResult = OutputCompilation.Emit(stream);
        Assert.True(emitResult.Success, string.Join(Environment.NewLine, emitResult.Diagnostics));

        stream.Position = 0;

        return new AssemblyLoadContext(name: null).LoadFromStream(stream);
    }

    public string AssertGeneratedAndCompiles()
    {
        Assert.Empty(CompileErrors);
        Assert.NotNull(Source);

        return Source;
    }

    // Generator diagnostics must be bound to the input syntax tree (not just a file path), otherwise
    // #pragma warning disable and other source suppressions cannot apply to them.
    public string GetLocationText(Diagnostic diagnostic)
    {
        Assert.True(diagnostic.Location.IsInSource);
        Assert.Equal("Model.cs", diagnostic.Location.SourceTree!.FilePath);

        return Input.ToString(diagnostic.Location.SourceSpan);
    }
}
