# 高度な機能

## Enum 型のサポート

`Select` や `MultiSelect` で Enum 型を使用すると、Enum の値から自動的に項目が生成されます。`[Display]` 属性を使って表示名をカスタマイズできます:

```csharp
using System.ComponentModel.DataAnnotations;

public enum MyEnum
{
    [Display(Name = "First value")]
    First,
    [Display(Name = "Second value")]
    Second,
    [Display(Name = "Third value")]
    Third
}

var value = Prompt.Select<MyEnum>("Select enum value");
Console.WriteLine($"You selected {value}");
```

`MultiSelect` の場合:

```csharp
var values = Prompt.MultiSelect<MyEnum>("Select enum values");
Console.WriteLine($"You picked {string.Join(", ", values)}");
```

## Unicode サポート

Sharprompt はマルチバイト文字と絵文字をサポートしています。最良の結果を得るには、出力エンコーディングを UTF-8 に設定してください:

```csharp
Console.OutputEncoding = Encoding.UTF8;

var name = Prompt.Input<string>("What's your name?");
Console.WriteLine($"Hello, {name}!");
```

## Fluent インターフェース

すべてのプロンプトタイプは `Sharprompt.Fluent` 名前空間を通じて Fluent インターフェースをサポートしています:

```csharp
using Sharprompt.Fluent;

var city = Prompt.Select<string>(o => o.WithMessage("Select your city")
                                       .WithItems(new[] { "Seattle", "London", "Tokyo" })
                                       .WithDefaultValue("Seattle"));
```

Fluent API はプロパティを設定するための `With*` メソッドと、コレクション（バリデータなど）に追加するための `Add*` メソッドを提供しています。

## テキストセレクタ

`Select` と `MultiSelect` では、項目の表示方法を制御するカスタム関数を提供できます:

```csharp
var user = Prompt.Select("Select user", users, textSelector: u => u.Name);
```

## ページネーション

`Select` と `MultiSelect` は `pageSize` パラメータによるページネーションをサポートしています:

```csharp
var city = Prompt.Select("Select your city",
    new[] { "Seattle", "London", "Tokyo", "New York", "Singapore", "Shanghai" },
    pageSize: 3);
```

## ループ選択

デフォルトでは、選択リストは先頭または末尾に到達するとループします。これはオプションクラスで無効にできます:

```csharp
var city = Prompt.Select(new SelectOptions<string>
{
    Message = "Select your city",
    Items = new[] { "Seattle", "London", "Tokyo" },
    LoopingSelection = false
});
```
