# Bind (モデルベースプロンプト)

`Bind` プロンプトはデータアノテーションを使用してモデルクラスに基づいたプロンプトを自動生成します。

## 基本的な使い方

データアノテーション付きのモデルクラスを定義します:

```csharp
using System.ComponentModel.DataAnnotations;

public class MyFormModel
{
    [Display(Name = "What's your name?")]
    [Required]
    public string Name { get; set; }

    [Display(Name = "Type new password")]
    [DataType(DataType.Password)]
    [Required]
    [MinLength(8)]
    public string Password { get; set; }

    [Display(Name = "Select your city")]
    [Required]
    [InlineItems("Seattle", "London", "Tokyo")]
    public string City { get; set; }

    [Display(Name = "Are you ready?")]
    public bool? Ready { get; set; }
}
```

`Prompt.Bind` を使用してモデルのプロンプトを生成します:

```csharp
var result = Prompt.Bind<MyFormModel>();
```

## 仕組み

Sharprompt はプロパティの型と属性に基づいてプロンプトタイプを自動的に決定します:

| プロパティの型 / 属性 | プロンプトタイプ |
|--------------------------|-------------|
| `string` | Input |
| `string` + `[DataType(DataType.Password)]` | Password |
| `bool?` | Confirm |
| `string` + `[InlineItems(...)]` | Select |
| Enum 型 | Select |

## サポートされている属性

| 属性 | 説明 |
|-----------|-------------|
| `[Display(Name = "...")]` | プロンプトメッセージを設定 |
| `[Required]` | フィールドを必須にする |
| `[MinLength(n)]` | 最小文字数のバリデーションを設定 |
| `[MaxLength(n)]` | 最大文字数のバリデーションを設定 |
| `[DataType(DataType.Password)]` | パスワードプロンプトを使用 |
| `[InlineItems("a", "b", "c")]` | Select プロンプトの項目を提供 |
| `[BindIgnore]` | バインディング時にプロパティをスキップ |
