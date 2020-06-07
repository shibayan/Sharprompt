# Sharprompt

![Build](https://github.com/shibayan/Sharprompt/workflows/Build/badge.svg)
[![License](https://img.shields.io/github/license/shibayan/Sharprompt.svg)](https://github.com/shibayan/Sharprompt/blob/master/LICENSE)

Interactive command line interface toolkit for .NET Core

![sharprompt](https://user-images.githubusercontent.com/1356444/62227794-87506e00-b3f7-11e9-84ae-06c9a900448b.gif)

## NuGet Package

Package Name | Target Framework | NuGet
---|---|---
Sharprompt | .NET Standard 2.0 | [![NuGet](https://img.shields.io/nuget/v/Sharprompt.svg)](https://www.nuget.org/packages/Sharprompt)

## Install

```
Install-Package Sharprompt
```

```
dotnet add package Sharprompt
```

## Usage

```csharp
var name = Prompt.Input<string>("What's your name?");
Console.WriteLine($"Hello, {name}!");

var secret = Prompt.Password("Type new password", new[] { Validators.Required(), Validators.MinLength(8) });
Console.WriteLine("Password OK");

var answer = Prompt.Confirm("Are you ready?", defaultValue: true);
Console.WriteLine($"Your answer is {answer}");
```

## APIs

### Input

```csharp
var name = Prompt.Input<string>("What's your name?");
Console.WriteLine($"Hello, {name}!");
```

![input](https://user-images.githubusercontent.com/1356444/62228275-50c72300-b3f8-11e9-8d51-63892e8eeaaa.gif)

### Confirm

```csharp
var answer = Prompt.Confirm("Are you ready?");
Console.WriteLine($"Your answer is {answer}");
```

![confirm](https://user-images.githubusercontent.com/1356444/62229064-e0210600-b3f9-11e9-8c52-b9c9257811c0.gif)

### Password

```csharp
var secret = Prompt.Password("Type new password");
Console.WriteLine("Password OK");
```

![password](https://user-images.githubusercontent.com/1356444/62228952-9fc18800-b3f9-11e9-98ea-3aa52ee84e93.gif)

### Select

```csharp
var city = Prompt.Select("Select your city", new[] { "Seattle", "London", "Tokyo" });
Console.WriteLine($"Hello, {city}!");
```

![select](https://user-images.githubusercontent.com/1356444/62228719-2de93e80-b3f9-11e9-8be5-f19e6ef58aeb.gif)

### MultiSelect
```csharp
var cities = Prompt.MultiSelect("Which cities would you like to visit?", new[] { "Seattle", "London", "Tokyo", "New York", "Singapore", "Shanghai" }, pageSize: 3);
Console.WriteLine($"You picked {string.Join(", ", options)}");
```

## Custom Prompter
```csharp
Prompt.ColorSchema.Answer = ConsoleColor.DarkRed;
Prompt.ColorSchema.Select = ConsoleColor.DarkCyan;
Prompt.ColorSchema.DisabledOption = ConsoleColor.DarkGray;

var name = Prompt.Input<string>("What's your name?");
Console.WriteLine($"Hello, {name}!");
```

## Platforms

- Windows
  - Cmd / PowerShell / Windows Terminal
- Ubuntu
  - Bash
- macOS
  - Bash

## License

This project is licensed under the [MIT License](https://github.com/shibayan/Sharprompt/blob/master/LICENSE)
