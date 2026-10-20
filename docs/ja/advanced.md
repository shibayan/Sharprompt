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

### 項目の並び順

Enum の項目は、メタデータが Sharprompt のソースジェネレーターから来た場合でも、リフレクションによるフォールバック (別アセンブリの Enum など、ジェネレーターが実行されなかった場合に使用) から来た場合でも、同じ順序で並びます:

1. メンバーは宣言順ではなく、基になる値の順に並びます。符号付きの Enum では負の値が先頭になります。`[Flags]` Enum の組み合わせ値も通常のメンバーとして一緒に並びます。
2. エイリアス (同じ値を持つ複数のメンバー) は 1 つにまとめられます。名前を序数比較 (ordinal) で並べたときに最初になるメンバーがその値を代表し、そのメンバーの `[Display]` 属性だけが使われます。
3. `[Display(Order = ...)]` を指定したメンバーが `Order` の順で先に並びます。`Order` を指定していないメンバーは、`Order = int.MaxValue` を含むすべての指定済みメンバーの後に並びます。`Order` が同じメンバー同士、および `Order` を指定していないメンバー同士は値の順を保ちます。

```csharp
public enum Priority
{
    Low = 1,
    [Display(Name = "High priority", Order = 0)]
    High = 3,
    Medium = 2,
    Normal = 2 // Medium のエイリアス。"Medium" の方が先に並ぶため、こちらが値を代表します。
}

// 項目: High priority, Low, Medium
var priority = Prompt.Select<Priority>("Select priority");
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
