# Input

`Input` プロンプトはジェネリック型パラメータを受け取り、適切な型変換を行います。

## 基本的な使い方

```csharp
var name = Prompt.Input<string>("What's your name?");
Console.WriteLine($"Hello, {name}!");
```

他の型でも使用できます:

```csharp
var number = Prompt.Input<int>("Enter any number");
Console.WriteLine($"Input = {number}");
```

## パラメータ

| パラメータ | 型 | 説明 |
|-----------|------|-------------|
| `message` | `string` | ユーザーに表示するメッセージ |
| `defaultValue` | `object?` | ユーザーが入力せずに Enter を押した場合のデフォルト値 |
| `placeholder` | `string?` | 入力エリアに表示されるプレースホルダーテキスト |
| `validators` | `IList<Func<object?, ValidationResult?>>?` | 入力に適用するバリデータのリスト |

## Options クラス

`InputOptions<T>` を使用してより詳細に制御できます:

```csharp
var name = Prompt.Input(new InputOptions<string>
{
    Message = "What's your name?",
    DefaultValue = "John Smith",
    Placeholder = "At least 3 characters",
    Validators = { Validators.Required(), Validators.MinLength(3) }
});
```

## Fluent API

```csharp
using Sharprompt.Fluent;

var name = Prompt.Input<string>(o => o.WithMessage("What's your name?")
                                      .WithDefaultValue("John Smith")
                                      .WithPlaceholder("At least 3 characters")
                                      .AddValidators(Validators.Required(), Validators.MinLength(3)));
```

## バリデーション付き

```csharp
var name = Prompt.Input<string>("What's your name?",
    defaultValue: "John Smith",
    placeholder: "At least 3 characters",
    validators: new[] { Validators.Required(), Validators.MinLength(3) });
```
