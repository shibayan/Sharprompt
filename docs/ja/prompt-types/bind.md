# Bind (モデルベースプロンプト)

`Bind` プロンプトは、モデルクラスに基づいて一連のプロンプトを自動生成します。

## 基本的な使い方

`[PromptBindable]` とデータアノテーションを付与したモデルクラスを定義します:

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
- `class` として宣言されている必要があります。
- クラスに `[PromptBindable]` 属性が付与されている必要があります（コンパイル時に Roslyn ソースジェネレーターがバインダーを生成します）。
- `Prompt.Bind<T>()` を呼び出す場合は、パブリックなパラメーターなしコンストラクターが必要です（インスタンスを渡す `Prompt.Bind(model)` を使用することも可能です）。
- バインド対象のプロパティは、アクセス可能なセッター（`public`、`internal`、または `protected internal`）を持つパブリックインスタンスプロパティである必要があります。読み取り専用や init-only プロパティはスキップされます。

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
| `[PromptBindable]` | ソース生成モデルバインディングの対象としてクラスを指定 |
| `[Display(Name = "...")]` | プロンプトメッセージを設定（`Prompt` でプレースホルダー、`Order` で表示順） |
| `[Required]` | フィールドを必須にする |
| `[MinLength(n)]` | 最小文字数のバリデーションを設定 |
| `[MaxLength(n)]` | 最大文字数のバリデーションを設定 |
| `[DataType(DataType.Password)]` | パスワードプロンプトを使用 |
| `[InlineItems("a", "b", "c")]` | インライン配列で選択項目を提供 |
| `[MemberItems(nameof(Member))]` | 静的プロパティまたはメソッドから動的に項目を提供 |
| `[BindIgnore]` | バインディング時にプロパティをスキップ |

## キャンセル

`CancellationToken` を渡すことで、モデルバインディング全体をキャンセルできます:

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
