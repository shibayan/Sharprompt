# Confirm

`Confirm` プロンプトはユーザーに Yes/No の質問をします。

## 基本的な使い方

```csharp
var answer = Prompt.Confirm("Are you ready?");
Console.WriteLine($"Your answer is {answer}");
```

## デフォルト値付き

```csharp
var answer = Prompt.Confirm("Are you ready?", defaultValue: true);
Console.WriteLine($"Your answer is {answer}");
```

## パラメータ

| パラメータ | 型 | 説明 |
|-----------|------|-------------|
| `message` | `string` | ユーザーに表示するメッセージ |
| `defaultValue` | `bool?` | ユーザーが入力せずに Enter を押した場合のデフォルト値 |

## Options クラス

```csharp
var answer = Prompt.Confirm(new ConfirmOptions
{
    Message = "Are you ready?",
    DefaultValue = true
});
```

## Fluent API

```csharp
using Sharprompt.Fluent;

var answer = Prompt.Confirm(o => o.WithMessage("Are you ready?")
                                  .WithDefaultValue(true));
```
