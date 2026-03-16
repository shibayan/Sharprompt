# Contributing

Thanks for contributing to Sharprompt.

## Development Environment

- .NET 8 SDK
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
