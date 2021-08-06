# Sharprompt

[![Build](https://github.com/shibayan/Sharprompt/workflows/Build/badge.svg)](https://github.com/shibayan/Sharprompt/actions/workflows/build.yml)
[![Downloads](https://img.shields.io/nuget/dt/Sharprompt)](https://www.nuget.org/packages/Sharprompt/)
[![NuGet](https://img.shields.io/nuget/v/Sharprompt)](https://www.nuget.org/packages/Sharprompt/)
[![License](https://img.shields.io/github/license/shibayan/Sharprompt)](https://github.com/shibayan/Sharprompt/blob/master/LICENSE)

Interactive command line interface toolkit for C#

![sharprompt](https://user-images.githubusercontent.com/1356444/62227794-87506e00-b3f7-11e9-84ae-06c9a900448b.gif)

## Install

```
Install-Package Sharprompt
```

```
dotnet add package Sharprompt
```

## Features

- Multi-platform support
- Supports the popular Prompts (Input / Password / Select / etc)
- Validation of input value
- Automatic generation of data source using Enum value
- Customize the color scheme
- Unicode support (East asian width and Emoji)

## Usage

```csharp
// Simple Input prompt
var name = Prompt.Input<string>("What's your name?");
Console.WriteLine($"Hello, {name}!");

// Password prompt
var secret = Prompt.Password("Type new password", new[] { Validators.Required(), Validators.MinLength(8) });
Console.WriteLine("Password OK");

// Confirmation prompt
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

**Enum value support**

```csharp
var value = Prompt.Select<MyEnum>("Select enum value");
Console.WriteLine($"You selected {value}");
```

### MultiSelect

```csharp
var cities = Prompt.MultiSelect("Which cities would you like to visit?", new[] { "Seattle", "London", "Tokyo", "New York", "Singapore", "Shanghai" }, pageSize: 3);
Console.WriteLine($"You picked {string.Join(", ", options)}");
```

![multiselect](https://user-images.githubusercontent.com/1356444/127033929-3278e39c-e260-4aed-9c3c-3cfd7d3f3549.gif)

### List

```csharp
var value = Prompt.List<string>("Please add item(s)");
Console.WriteLine($"You picked {string.Join(", ", value)}");
```

![list](https://user-images.githubusercontent.com/1356444/127033968-cf70bd1b-bcd1-4c4f-bdbe-74aae52cdb86.gif)

## Configuration

### Custom Prompter

```csharp
Prompt.ColorSchema.Answer = ConsoleColor.DarkRed;
Prompt.ColorSchema.Select = ConsoleColor.DarkCyan;

var name = Prompt.Input<string>("What's your name?");
Console.WriteLine($"Hello, {name}!");
```

### Unicode Support

![unicode](https://user-images.githubusercontent.com/1356444/89803983-86a3f900-db6e-11ea-8fc8-5b6f9ef5644f.gif)

```csharp
// Prefer UTF-8 as the output encoding
Console.OutputEncoding = Encoding.UTF8;

var name = Prompt.Input<string>("What's your name?");
Console.WriteLine($"Hello, {name}!");
```

## Platforms

- Windows
  - Command Prompt / PowerShell / Windows Terminal
- Ubuntu
  - Bash
- macOS
  - Bash

## License

This project is licensed under the [MIT License](https://github.com/shibayan/Sharprompt/blob/master/LICENSE)
