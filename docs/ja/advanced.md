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

## Native AOT とソースジェネレーター

Sharprompt は `<IsAotCompatible>true</IsAotCompatible>` に対応しており、.NET 8 および .NET 10 で Native AOT コンパイルをサポートしています。

### 仕組み

Sharprompt は Roslyn インクリメンタルソースジェネレーターを利用してコンパイル時にコードを生成し、実行時のリフレクションを排除しています:

1. **`[PromptBindable]` ジェネレーター**: `[PromptBindable]` 属性が付与されたクラスやレコードを解析し、C# の `[ModuleInitializer]` 経由で登録される型安全なモデルバインダーコードを生成します。`Prompt.Bind<T>()` の呼び出し時、リフレクションなしで事前コンパイルされたバインダーが直接実行されます。
2. **Enum メタデータジェネレーター**: プロジェクト内の enum 宣言を解析し、値の一覧や `[Display]` 属性（表示名・並び順）を `[ModuleInitializer]` 経由で登録するコードを生成します。

### プロジェクト構成と発行

Native AOT でアプリケーションを発行するには、プロジェクトファイルに `<PublishAot>true</PublishAot>` を追加します:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <PublishAot>true</PublishAot>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Sharprompt" Version="..." />
  </ItemGroup>

</Project>
```

対象のランタイムを指定して `dotnet publish` コマンドを実行します:

```sh
dotnet publish -c Release -r win-x64
```

※ `win-x64` はターゲットのランタイム識別子（例: `linux-x64`、`osx-arm64`）に適宜置き換えてください。

### Enum の取り扱いと外部アセンブリ

- **同一プロジェクト内の Enum**: 同一プロジェクト内で宣言された enum は、コンパイル時にソースジェネレーターによって自動検出されます。`[Display]` で指定された表示名や並び順もコードとして直接生成されます。
- **外部アセンブリの Enum**: 外部アセンブリの enum（BCL の `DayOfWeek` や参照パッケージの enum など）はソースジェネレーターの解析対象外です。実行時、Sharprompt は Native AOT に配慮したリフレクションフォールバックを使用します。リフレクションを完全に排除したい場合や表示をカスタマイズしたい場合は、アプリケーション起動時に手動で登録できます:

```csharp
using Sharprompt;

EnumMetadataRegistry.Register<DayOfWeek>(
    Enum.GetValues<DayOfWeek>(),
    day => day.ToString()
);
```

### 型変換とカスタム型

プロンプト入力の型変換は `TypeDescriptor` コンバーターを利用しています。組み込みの標準コンバーター（`string`、数値型、`bool`、`char`、`Guid`、`DateTime`、enum、およびこれらの Nullable）は Native AOT 環境でも完全に保持されます。

`[TypeConverter]` 属性を持つ独自のカスタム型を使用する場合は、トリミングによってコンバーター型が削除されないよう、アプリケーション側で型を保持する設定（`[DynamicallyAccessedMembers]` やトリマー記述ファイルなど）を行ってください。

### キャンセル

モデルバインディングを含むすべてのプロンプトで `CancellationToken` によるキャンセルをサポートしています。`Prompt.Bind` にトークンを渡します:

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

try
{
    var form = Prompt.Bind<MyFormModel>(cts.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("操作がタイムアウトまたはキャンセルされました。");
}
```

トークンがキャンセル済みの場合、`Prompt.Bind` はプロンプトを表示する前に例外をスローします。プロンプトは `IConsoleDriver.ReadKey(CancellationToken)` を通じてキー入力を待機します。入力ソース自体がキャンセルに対応しているカスタムコンソールドライバーでは、このメソッドをオーバーライドできます。詳細は [コードからのキャンセル](/ja/configuration#コードからのキャンセル) を参照してください。
