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

## 項目のルール

- `Items` はプロンプトの開始時に 1 回だけ読み取ります。等しい項目（`EqualityComparer<T>.Default` で比較）は、最初の位置に 1 つだけ表示します。
- `DefaultValue` は、プロンプトの開始時に一致する項目を選択します。`Items` に含まれない場合、`DefaultValue` は何も選択しません。互換性のある型の値（`long` のプロンプトに対する `int` など）は変換され、互換性のない型は `ArgumentException` をスローします。
- 何も選択されていない状態で <kbd>Enter</kbd> を押すとエラーを表示します。表示される項目が 1 つだけの場合（フィルターの有無にかかわらず）や、フィルターが項目のテキストと完全に一致する場合は、その項目が選択されます。
- `Items` が空の場合は、何も選択できないため、プロンプトを表示する前に `ArgumentException` をスローします。

## Enum 型の使用

Enum 型を使用すると、項目が自動的に生成されます:

```csharp
var value = Prompt.Select<MyEnum>("Select enum value");
Console.WriteLine($"You selected {value}");
```

項目は値の順に並び、`[Display(Order)]` を指定したメンバーが先頭になります。正確な規則は [項目の並び順](/ja/advanced#項目の並び順) を、詳細は [Enum サポート](/ja/advanced#enum-型のサポート) を参照してください。

## Fluent API

```csharp
using Sharprompt.Fluent;

var city = Prompt.Select<string>(o => o.WithMessage("Select your city")
                                       .WithItems(new[] { "Seattle", "London", "Tokyo" })
                                       .WithDefaultValue("Seattle"));
```
