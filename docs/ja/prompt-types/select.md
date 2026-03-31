# Select

`Select` プロンプトはリストから 1 つの項目を選択させます。

## 基本的な使い方

```csharp
var city = Prompt.Select("Select your city", new[] { "Seattle", "London", "Tokyo" });
Console.WriteLine($"Hello, {city}!");
```

## パラメータ

| パラメータ | 型 | 説明 |
|-----------|------|-------------|
| `message` | `string` | ユーザーに表示するメッセージ |
| `items` | `IEnumerable<T>?` | 選択肢の項目 (Enum 型の場合は自動生成) |
| `pageSize` | `int` | 1 ページあたりの表示項目数 (デフォルト: 無制限) |
| `defaultValue` | `object?` | デフォルトの選択値 |
| `textSelector` | `Func<T, string>?` | 項目を表示テキストに変換する関数 |

## Options クラス

```csharp
var city = Prompt.Select(new SelectOptions<string>
{
    Message = "Select your city",
    Items = new[] { "Seattle", "London", "Tokyo", "New York", "Singapore", "Shanghai" },
    PageSize = 3,
    DefaultValue = "Seattle"
});
```

## プロパティ

| プロパティ | 型 | デフォルト | 説明 |
|----------|------|---------|-------------|
| `Items` | `IEnumerable<T>` | — | 選択肢の項目 |
| `DefaultValue` | `object?` | `null` | デフォルトの選択値 |
| `PageSize` | `int` | `int.MaxValue` | 1 ページあたりの項目数 |
| `TextSelector` | `Func<T, string>` | `x => x.ToString()!` | 表示テキストセレクタ |
| `LoopingSelection` | `bool` | `true` | リストの先頭/末尾でループするかどうか |

## ページネーション付き

```csharp
var city = Prompt.Select("Select your city",
    new[] { "Seattle", "London", "Tokyo", "New York", "Singapore", "Shanghai" },
    pageSize: 3);
Console.WriteLine($"Hello, {city}!");
```

## Enum 型の使用

Enum 型を使用すると、項目が自動的に生成されます:

```csharp
var value = Prompt.Select<MyEnum>("Select enum value");
Console.WriteLine($"You selected {value}");
```

詳細は [Enum サポート](/ja/advanced#enum-型のサポート) を参照してください。

## Fluent API

```csharp
using Sharprompt.Fluent;

var city = Prompt.Select<string>(o => o.WithMessage("Select your city")
                                       .WithItems(new[] { "Seattle", "London", "Tokyo" })
                                       .WithDefaultValue("Seattle"));
```
