# List

`List` プロンプトはユーザーが対話的に複数の項目を追加できるようにします。

## 基本的な使い方

```csharp
var value = Prompt.List<string>("Please add item(s)");
Console.WriteLine($"You picked {string.Join(", ", value)}");
```

## パラメータ

| パラメータ | 型 | 説明 |
|-----------|------|-------------|
| `message` | `string` | ユーザーに表示するメッセージ |
| `minimum` | `int` | 必要な最小項目数 (デフォルト: `1`) |
| `maximum` | `int` | 許可される最大項目数 (デフォルト: 無制限) |
| `validators` | `IList<Func<object?, ValidationResult?>>?` | 各項目に適用するバリデータのリスト |

## Options クラス

```csharp
var value = Prompt.List(new ListOptions<string>
{
    Message = "Please add item(s)",
    Minimum = 1,
    Maximum = 5,
    Validators = { Validators.Required() }
});
```

## プロパティ

| プロパティ | 型 | デフォルト | 説明 |
|----------|------|---------|-------------|
| `DefaultValues` | `IEnumerable<T>` | `[]` | リストに事前入力される項目 |
| `Minimum` | `int` | `1` | 必要な最小項目数 |
| `Maximum` | `int` | `int.MaxValue` | 許可される最大項目数 |
| `Validators` | `IList<Func<object?, ValidationResult?>>` | `[]` | 各項目のバリデータ |

## Fluent API

```csharp
using Sharprompt.Fluent;

var value = Prompt.List<string>(o => o.WithMessage("Please add item(s)")
                                      .WithMinimum(1)
                                      .WithMaximum(5));
```
