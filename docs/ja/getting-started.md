# はじめに

## インストール

NuGet から Sharprompt をインストールします:

::: code-group

```powershell [Package Manager]
Install-Package Sharprompt
```

```sh [.NET CLI]
dotnet add package Sharprompt
```

:::

## クイックスタート

```csharp
using Sharprompt;

// シンプルな入力
var name = Prompt.Input<string>("What's your name?");
Console.WriteLine($"Hello, {name}!");

// パスワード入力
var secret = Prompt.Password("Type new password",
    validators: new[] { Validators.Required(), Validators.MinLength(8) });
Console.WriteLine("Password OK");

// 確認
var answer = Prompt.Confirm("Are you ready?", defaultValue: true);
Console.WriteLine($"Your answer is {answer}");
```

## サンプルの実行

リポジトリには全プロンプトタイプのサンプルプロジェクトが含まれています:

```sh
dotnet run --project samples/Sharprompt.Example
```

## サポートされているプラットフォーム

| プラットフォーム | ターミナル |
|----------|----------|
| Windows | コマンドプロンプト、PowerShell、Windows Terminal |
| Linux | Windows Terminal (WSL 2)、各種ターミナルエミュレータ |
| macOS | Terminal.app、iTerm2 など |
