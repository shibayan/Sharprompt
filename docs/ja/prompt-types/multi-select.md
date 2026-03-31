# MultiSelect

`MultiSelect` プロンプトはチェックボックスを使ってリストから複数の項目を選択させます。

## 基本的な使い方

```csharp
var cities = Prompt.MultiSelect("Which cities would you like to visit?",
    new[] { "Seattle", "London", "Tokyo", "New York", "Singapore", "Shanghai" },
    pageSize: 3);
Console.WriteLine($"You picked {string.Join(", ", cities)}");
```

## パラメータ

| パラメータ | 型 | 説明 |
|-----------|------|-------------|
| `message` | `string` | ユーザーに表示するメッセージ |
| `items` | `IEnumerable<T>?` | 選択肢の項目 (Enum 型の場合は自動生成) |
| `pageSize` | `int` | 1 ページあたりの表示項目数 (デフォルト: 無制限) |
| `minimum` | `int` | 選択必須の最小項目数 (デフォルト: `1`) |
| `maximum` | `int` | 選択可能な最大項目数 (デフォルト: 無制限) |
| `defaultValues` | `IEnumerable<T>?` | デフォルトで選択される項目 |
| `textSelector` | `Func<T, string>?` | 項目を表示テキストに変換する関数 |

## Options クラス

```csharp
var cities = Prompt.MultiSelect(new MultiSelectOptions<string>
{
    Message = "Which cities would you like to visit?",
    Items = new[] { "Seattle", "London", "Tokyo", "New York", "Singapore", "Shanghai" },
    PageSize = 3,
    Minimum = 1,
    Maximum = 3,
    DefaultValues = new[] { "Tokyo" }
});
```

## プロパティ

| プロパティ | 型 | デフォルト | 説明 |
|----------|------|---------|-------------|
| `Items` | `IEnumerable<T>` | — | 選択肢の項目 |
| `DefaultValues` | `IEnumerable<T>` | `[]` | デフォルトで選択される項目 |
| `PageSize` | `int` | `int.MaxValue` | 1 ページあたりの項目数 |
| `Minimum` | `int` | `1` | 選択必須の最小数 |
| `Maximum` | `int` | `int.MaxValue` | 選択可能な最大数 |
| `TextSelector` | `Func<T, string>` | `x => x.ToString()!` | 表示テキストセレクタ |
| `LoopingSelection` | `bool` | `true` | リストの先頭/末尾でループするかどうか |

## Enum 型の使用

```csharp
var values = Prompt.MultiSelect<MyEnum>("Select enum values", defaultValues: new[] { MyEnum.Bar });
Console.WriteLine($"You picked {string.Join(", ", values)}");
```

## Fluent API

```csharp
using Sharprompt.Fluent;

var cities = Prompt.MultiSelect<string>(o => o.WithMessage("Which cities would you like to visit?")
                                               .WithItems(new[] { "Seattle", "London", "Tokyo" })
                                               .WithPageSize(3));
```
