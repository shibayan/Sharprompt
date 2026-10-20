# Bind (モデルベースプロンプト)

`Bind` プロンプトは、モデルクラスまたはレコードに基づいて一連のプロンプトを自動生成します。

## 基本的な使い方

`[PromptBindable]` とデータアノテーションを付与したモデルクラス（またはレコード）を定義します:

```csharp
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Sharprompt;

[PromptBindable]
public class MyFormModel
{
    [Display(Name = "What's your name?")]
    [Required]
    public string Name { get; set; } = null!;

    [Display(Name = "Type new password")]
    [DataType(DataType.Password)]
    [Required]
    [MinLength(8)]
    public string Password { get; set; } = null!;

    [Display(Name = "Select your city")]
    [Required]
    [InlineItems("Seattle", "London", "Tokyo")]
    public string City { get; set; } = null!;

    [Display(Name = "Select favorite fruit")]
    [MemberItems(nameof(Fruits))]
    public string Fruit { get; set; } = null!;

    [Display(Name = "Are you ready?")]
    public bool? Ready { get; set; }

    public static IEnumerable<string> Fruits => new[] { "Apple", "Banana", "Orange" };
}
```

`Prompt.Bind` を使用してモデルのプロンプトを実行します:

```csharp
var result = Prompt.Bind<MyFormModel>();
```

## モデルの要件

ソースジェネレーターによるモデルバインディング（Native AOT を含む）を利用するには、以下の要件を満たす必要があります:
- `class` または `record`（`record class`）として宣言されている必要があります。`record struct` を含む構造体はサポートされていません。
- モデルに `[PromptBindable]` 属性が付与されている必要があります（コンパイル時に Roslyn ソースジェネレーターがバインダーを生成します）。
- `Prompt.Bind<T>()` を呼び出す場合は、パブリックなパラメーターなしコンストラクターが必要です（インスタンスを渡す `Prompt.Bind(model)` を使用することも可能です）。
- バインド対象のプロパティは、アクセス可能な `set` アクセサー（`public`、`internal`、または `protected internal`）を持つパブリックインスタンスプロパティである必要があります。読み取り専用プロパティはスキップされます。
- init-only プロパティは `SHARP002` 警告を出力してスキップされます。位置指定レコードのパラメーターから生成されるプロパティも init-only になるため、バインド対象のプロパティはレコード本体に `{ get; set; }` で宣言してください。
- コレクション型のプロパティは `T[]`、`List<T>`、`HashSet<T>`、`IEnumerable<T>`、またはこれらが実装するインターフェイス（`IList<T>` や `IReadOnlyCollection<T>` など）である必要があります。それ以外のコレクション型は `SHARP003` 警告を出力してスキップされます。

```csharp
[PromptBindable]
public record MyRecordModel
{
    [Display(Name = "What's your name?")]
    public string Name { get; set; } = "";
}
```

## 仕組み

Sharprompt はプロパティの型と属性に基づいてプロンプトタイプを自動的に決定します:

| プロパティの型 / 属性 | プロンプトタイプ |
|--------------------------|-------------|
| `string` | Input |
| `string` + `[DataType(DataType.Password)]` | Password |
| `bool` または `bool?` | Confirm |
| 単一値 + `[InlineItems(...)]` または `[MemberItems(...)]` | Select |
| `IEnumerable<T>` + `[InlineItems(...)]` または `[MemberItems(...)]` | MultiSelect |
| Enum 型 | Select (コレクションの場合は MultiSelect) |
| `IEnumerable<T>` (項目指定なし) | List |

## サポートされている属性

| 属性 | 説明 |
|-----------|-------------|
| `[PromptBindable]` | ソース生成モデルバインディングの対象としてクラスまたはレコードを指定 |
| `[Display(Name = "...")]` | プロンプトメッセージ（`Name`）とプレースホルダー（`Prompt`）を設定 |
| `[Display(Order = n)]` | プロンプトの表示順を設定。`Order` を指定したプロパティは昇順で表示され、指定していないプロパティはすべての順序指定プロパティの後に宣言順で表示 |
| `[Required]` | フィールドを必須にする |
| `[MinLength(n)]` | 最小文字数のバリデーションを設定 |
| `[MaxLength(n)]` | 最大文字数のバリデーションを設定 |
| `[DataType(DataType.Password)]` | パスワードプロンプトを使用 |
| `[InlineItems("a", "b", "c")]` | インライン配列で選択項目を提供 |
| `[MemberItems(nameof(Member))]` | モデル（または 2 番目の引数で指定した型）のパブリックな静的プロパティまたはパラメーターなしメソッドから動的に項目を提供 |
| `[BindIgnore]` | バインディング時にプロパティをスキップ |

## コンパイラ診断

ソースジェネレーターは `[PromptBindable]` モデルに対して以下の警告を出力します:

| ID | 説明 |
|----|-------------|
| `SHARP001` | `[MemberItems]` で参照したメンバーが存在しないか、パブリックな静的プロパティまたはパラメーターなしメソッドではありません。項目の提供元は無視されます。 |
| `SHARP002` | プロパティが init-only セッターを持つため、スキップされます。`set` アクセサーを使用するか、`[BindIgnore]` を付与してください。 |
| `SHARP003` | プロパティのコレクション型がサポートされていないため、スキップされます。サポートされているコレクション型を使用するか、`[BindIgnore]` を付与してください。 |

## キャンセル

`CancellationToken` を渡すことで、モデルバインディングをキャンセルできます。プロンプトが入力を待っている間にトークンがキャンセルされると、そのプロンプトを閉じ、残りのプロパティは入力を求めずに `OperationCanceledException` をスローします。`Prompt.Bind` を呼び出した時点でトークンがキャンセル済みの場合は、プロンプトを表示する前に例外をスローします:

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

try
{
    var result = Prompt.Bind<MyFormModel>(cts.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("プロンプトのバインディングがキャンセルされました。");
}
```

カスタムコンソールドライバーで `IConsoleDriver.ReadKey(CancellationToken)` をオーバーライドする方法を含め、詳細は [コードからのキャンセル](/ja/configuration#コードからのキャンセル) を参照してください。
