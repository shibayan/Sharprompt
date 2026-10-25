# Contributing

Thanks for contributing to Sharprompt.

## Development Environment

- .NET 10 SDK (for the solution format and test runner), with .NET 8 and .NET 10 runtimes for tests
- A terminal that supports interactive console applications

## Setup

```sh
dotnet restore
dotnet build -c Release
dotnet test -c Release
```

The example project can be run with:

```sh
dotnet run --project samples/Sharprompt.Example
```

## Before Opening a Pull Request

Please make sure the following commands succeed locally:

```sh
dotnet build -c Release
dotnet test -c Release --no-build
dotnet format --verify-no-changes --verbosity detailed --no-restore
```

Public APIs must have XML documentation; missing comments fail the library build. The generated XML is packed with the library. The test suite compiles each C# block in the README as a separate console application with the standard console-project implicit usings, including source generators. Keep each block self-contained with explicit imports for Sharprompt and any other namespaces.

Run the unattended usage examples for both target frameworks:

```sh
dotnet run --project samples/Sharprompt.Automation -c Release -f net8.0 --no-build
dotnet run --project samples/Sharprompt.Automation -c Release -f net10.0 --no-build
```

The testing guide imports its C# directly from this sample. See [docs/README.md](docs/README.md) for building the site, Markdown exports, and `llms.txt`.

Changes that may affect Native AOT, trimming, resources, or the source generators are also verified in CI by publishing a test application against the packed NuGet package. See [tests/Sharprompt.NativeAot](tests/Sharprompt.NativeAot/README.md) to run it locally.

## Contribution Guidelines

- Keep changes focused on a single problem or feature.
- Add or update tests when behavior changes.
- Update the README or examples when public APIs or user-facing behavior change.
- Prefer small, reviewable pull requests.

## Pull Requests

- Describe the motivation for the change.
- Summarize the behavior change clearly.
- Link related issues when applicable.

By submitting a contribution, you agree that your work will be released under the MIT License used by this repository.
