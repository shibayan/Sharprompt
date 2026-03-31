# バリデータ

Sharprompt は、`Input`、`Password`、`List` プロンプトの `validators` パラメータで使用できる組み込みバリデータを提供しています。

## 使い方

```csharp
var secret = Prompt.Password("Type new password",
    validators: new[] { Validators.Required(), Validators.MinLength(8) });
```

## 組み込みバリデータ

| バリデータ | 説明 |
|-----------|-------------|
| `Validators.Required()` | 入力が空でないことを確認します |
| `Validators.MinLength(length)` | 入力が指定された文字数以上であることを確認します |
| `Validators.MaxLength(length)` | 入力が指定された文字数を超えないことを確認します |
| `Validators.RegularExpression(pattern)` | 入力が指定された正規表現にマッチすることを確認します |

## バリデータの組み合わせ

複数のバリデータを組み合わせることができます:

```csharp
var name = Prompt.Input<string>("What's your name?",
    validators: new[]
    {
        Validators.Required(),
        Validators.MinLength(3),
        Validators.MaxLength(50)
    });
```

## カスタムバリデータ

バリデータは `ValidationResult` を返す関数です。カスタムバリデータを作成できます:

```csharp
var email = Prompt.Input<string>("Enter your email",
    validators: new Func<object?, ValidationResult?>[]
    {
        Validators.Required(),
        input =>
        {
            var value = input?.ToString();
            if (value != null && !value.Contains('@'))
            {
                return new ValidationResult("Please enter a valid email address");
            }
            return ValidationResult.Success;
        }
    });
```
