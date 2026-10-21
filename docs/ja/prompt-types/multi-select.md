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

## 選択のルール

- `Items` はプロンプトの開始時に 1 回だけ読み取ります。等しい項目（`EqualityComparer<T>.Default` で比較）は、最初の位置に 1 つだけ表示します。
- `Items` に含まれない `DefaultValues` は無視されます。
- 結果には、選択された項目が `Items` の順序で含まれます。
- `Minimum` と `Maximum` は選択数を制限します:

| 操作 | 結果 |
|------|------|
| 選択数が `Maximum` のときに、未選択の項目で <kbd>Space</kbd> | 項目は選択されず、エラーを表示します。 |
| `Maximum` を超える <kbd>Ctrl</kbd>+<kbd>A</kbd> または <kbd>Ctrl</kbd>+<kbd>I</kbd> | 何も変更せず、エラーを表示します。 |
| `Maximum` より多い `DefaultValues` | すべて選択された状態で始まります。十分に選択を外すまで <kbd>Enter</kbd> は拒否されます。 |
| `Minimum` 未満または `Maximum` 超過での <kbd>Enter</kbd> | エラーを表示して拒否します。 |

<kbd>Ctrl</kbd>+<kbd>A</kbd> と <kbd>Ctrl</kbd>+<kbd>I</kbd> は、現在のフィルターに一致する項目だけに作用します。フィルターで隠れている選択済みの項目は、選択されたままです。

`Minimum` が負の値、`Maximum` が `Minimum` より小さい、または `Minimum` が項目数より大きい場合は、プロンプトに回答できないため、表示する前に `ArgumentOutOfRangeException` をスローします。`Items` が空の場合は `Minimum` を `0` にしてください。

## Enum 型の使用

```csharp
var values = Prompt.MultiSelect<MyEnum>("Select enum values", defaultValues: new[] { MyEnum.Bar });
Console.WriteLine($"You picked {string.Join(", ", values)}");
```

項目は `Select` と同じ順序で並びます。[項目の並び順](/ja/advanced#項目の並び順) を参照してください。

## Fluent API

```csharp
using Sharprompt.Fluent;

var cities = Prompt.MultiSelect<string>(o => o.WithMessage("Which cities would you like to visit?")
                                               .WithItems(new[] { "Seattle", "London", "Tokyo" })
                                               .WithPageSize(3));
```
