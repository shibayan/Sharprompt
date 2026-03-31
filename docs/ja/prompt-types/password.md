# Password

`Password` プロンプトは文字をマスクしながらユーザー入力を受け付けます。

## 基本的な使い方

```csharp
var secret = Prompt.Password("Type new password");
Console.WriteLine("Password OK");
```

## パラメータ

| パラメータ | 型 | 説明 |
|-----------|------|-------------|
| `message` | `string` | ユーザーに表示するメッセージ |
| `passwordChar` | `string` | 入力をマスクするために使用される文字 (デフォルト: `"*"`) |
| `placeholder` | `string?` | 入力エリアに表示されるプレースホルダーテキスト |
| `validators` | `IList<Func<object?, ValidationResult?>>?` | 入力に適用するバリデータのリスト |

## Options クラス

```csharp
var secret = Prompt.Password(new PasswordOptions
{
    Message = "Type new password",
    Placeholder = "At least 8 characters",
    PasswordChar = "*",
    Validators = { Validators.Required(), Validators.MinLength(8) }
});
```

## Fluent API

```csharp
using Sharprompt.Fluent;

var secret = Prompt.Password(o => o.WithMessage("Type new password")
                                   .WithPlaceholder("At least 8 characters")
                                   .AddValidators(Validators.Required(), Validators.MinLength(8)));
```

## バリデーション付き

```csharp
var secret = Prompt.Password("Type new password",
    placeholder: "At least 8 characters",
    validators: new[] { Validators.Required(), Validators.MinLength(8) });
```
