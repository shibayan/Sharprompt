# Getting Started

## Installation

Install Sharprompt via NuGet:

::: code-group

```powershell [Package Manager]
Install-Package Sharprompt
```

```sh [.NET CLI]
dotnet add package Sharprompt
```

:::

## Quick Start

```csharp
using Sharprompt;

// Simple input
var name = Prompt.Input<string>("What's your name?");
Console.WriteLine($"Hello, {name}!");

// Password input
var secret = Prompt.Password("Type new password",
    validators: new[] { Validators.Required(), Validators.MinLength(8) });
Console.WriteLine("Password OK");

// Confirmation
var answer = Prompt.Confirm("Are you ready?", defaultValue: true);
Console.WriteLine($"Your answer is {answer}");
```

## Running the Examples

The repository includes a sample project with all prompt types:

```sh
dotnet run --project samples/Sharprompt.Example
```

## Supported Platforms

| Platform | Terminal |
|----------|----------|
| Windows | Command Prompt, PowerShell, Windows Terminal |
| Linux | Windows Terminal (WSL 2), various terminal emulators |
| macOS | Terminal.app, iTerm2, etc. |
