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

### 表示名とローカライズ

項目のテキストは `[Display(Name = ...)]` で、`Name` がなければメンバー名になります。ローカライズするには `ResourceType` も指定します。このとき `Name` は、通常の `DisplayAttribute` と同じく、その型のプロパティ名を表します。型は `public` で、プロパティは `public static string` である必要があります（`PublicResXFileCodeGenerator` が生成するプロパティなど）。

```csharp
public enum Priority
{
    [Display(Name = nameof(Strings.PriorityLow), ResourceType = typeof(Strings))]
    Low,
    [Display(Name = nameof(Strings.PriorityHigh), ResourceType = typeof(Strings))]
    High
}
```

プロパティは項目を表示するたびに読み取られるため、表示名は `CultureInfo.CurrentUICulture`（またはリソースクラスに設定したカルチャ）に従います。`Prompt.Culture` が変更するのは Sharprompt 自身のメッセージだけで、アプリのリソースには影響しません。プロパティが `null` を返した場合はメンバー名を表示します。

### 注意事項と制限

- `[Display]` のうち使われるのは `Name`、`ResourceType`、`Order` だけです。`ShortName`、`Description`、`GroupName`、`Prompt` は無視されます。`[Description]` や `[EnumMember]` などのほかの属性も無視されます。
- `[Flags]` の Enum では、組み合わせ値のメンバーも含めて各メンバーが別々の項目になります。`MultiSelect` は選択されたメンバーをコレクションで返し、1 つのフラグ値には結合しません。
- いずれかのメンバーで `ResourceType` を指定した Enum については、表示名を `DisplayAttribute.GetName` で解決するため、ソースジェネレーターはメタデータを生成しません。別アセンブリで宣言された Enum、`private`・`protected`・ジェネリック型の入れ子の Enum、`file` で宣言された Enum も同様です。これらの Enum はリフレクションによるフォールバックを使いますが、適用されるルールは同じで、Native AOT でもサポートされます。不正なリソース参照は、`DisplayAttribute.GetName` と同じく、項目を表示するときに例外をスローします。
- 項目や表示名を変えたい場合は、起動時に `EnumMetadataRegistry.Register` で Enum を登録してください。登録したメタデータは、どちらの経路のメタデータよりも優先されます。

## Unicode サポート

Sharprompt はマルチバイト文字と絵文字をサポートしています。最良の結果を得るには、出力エンコーディングを UTF-8 に設定してください:

```csharp
Console.OutputEncoding = Encoding.UTF8;

var name = Prompt.Input<string>("What's your name?");
Console.WriteLine($"Hello, {name}!");
```

### 制御文字

メッセージ、項目、回答、デフォルト値、エラーメッセージには、外部のデータが含まれることがあります。Sharprompt はこれらを表示幅の計算と出力に使う前に制御文字を正規化し、プロンプトのレイアウトとカーソル位置が崩れないようにします:

| 文字 | 表示 |
|------|------|
| 改行（`\r\n`、`\n`、`\r`、U+0085、U+2028、U+2029） | 改行します。複数行のメッセージやエラーメッセージは複数行で表示されます。 |
| タブ（`\t`） | 行頭から数えて次の 8 桁の倍数まで空白で埋めます。 |
| 色やスタイルを設定する ANSI SGR シーケンス（パラメーターが数字、`;`、`:` だけの `ESC [ ... m`） | そのまま出力し、幅には数えません。スタイルはその行の終わりで終わり、次の行はプロンプト自身の色で始まります。複数行に同じスタイルを付けたい場合は、行ごとにシーケンスを書いてください。 |
| カーソル移動、画面消去、OSC など、その他のエスケープシーケンス | カーソルを動かしたり端末の状態を変えたりするため、取り除きます。 |
| その他の制御文字 | 見える記号で表示します。U+0000〜U+001F は `␀`〜`␟`、DEL は `␡`、U+0080〜U+009F は `�` です。 |

変わるのは表示だけです。`Select` と `MultiSelect` は元の項目を返し、`Input` は入力されたとおりのテキストを返します。制御文字になるキー入力は、入力欄に挿入されません。<kbd>Tab</kbd> で挿入したデフォルト値など、編集中のテキストではエスケープシーケンスを適用せず、`ESC` を `␛` で表示するため、カーソルはすべての文字の上を移動します。

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

`Input` と `List` は、入力されたテキストを `TypeDescriptor` のコンバーターで変換します。変換には常にインバリアントカルチャを使います。デフォルト値のヒント、<kbd>Tab</kbd> で挿入されるテキスト、回答の表示も同じ形式で書式化されるため、プロンプトに表示された値はそのまま入力し直せます。たとえば `1.5` は、現在のカルチャが `1,5` と書式化する場合でも `1.5` と表示されます。

次の型（およびその `Nullable<T>`）をサポートしています。いずれも、Sharprompt パッケージを参照する Native AOT アプリケーションを CI で発行・実行して検証しています。

| 分類 | 型 |
| --- | --- |
| テキスト | `string`、`char` |
| 真偽値 | `bool` |
| 整数 | `byte`、`sbyte`、`short`、`ushort`、`int`、`uint`、`long`、`ulong`、`Int128` |
| 浮動小数点数 | `Half`、`float`、`double`、`decimal` |
| 日付と時刻 | `DateTime`、`DateTimeOffset`、`DateOnly`、`TimeOnly`、`TimeSpan` |
| その他 | `Guid`、`Uri`、`Version`、enum |

enum の入力では、メンバー名（大文字と小文字を区別しない）と数値を受け付けます。名前付きメンバーのない値は数値で表示されます。

#### カスタム型

独自の型を使う場合は、`[TypeConverter]` 属性を付け、`string` との相互変換を実装します。

```csharp
var temperature = Prompt.Input<Temperature>("Target temperature");

[TypeConverter(typeof(TemperatureConverter))]
public readonly record struct Temperature(double Celsius);

public sealed class TemperatureConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        => value is string text && text.EndsWith('C')
            ? new Temperature(double.Parse(text[..^1], culture))
            : base.ConvertFrom(context, culture, value);

    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
        => value is Temperature temperature && destinationType == typeof(string)
            ? temperature.Celsius.ToString(culture) + "C"
            : base.ConvertTo(context, culture, value, destinationType);
}
```

Sharprompt は `culture` に `CultureInfo.InvariantCulture` を渡します。デフォルト値や回答を `ConvertFrom` が受け付ける形式で表示できるよう、`ConvertTo` も実装してください。

Native AOT では、`[TypeConverter]` が指定するコンバーターのコンストラクターが保持されます。アプリケーション内と、トリミング対象のライブラリ内で宣言したカスタムコンバーターの両方を CI で検証しています。ただし、この参照が安全であることはトリミング解析では一般には証明できないため、Sharprompt はその警告を 1 か所で抑制しています。お使いのコンバーターは、発行したアプリケーションでも確認してください。

**制限事項**

- `TypeConverter` を持たない型は、`IParsable<T>` を実装していても変換できません（`BigInteger` など）。これらの型への入力はエラーメッセージを表示して拒否されます。
- 変換には常にインバリアントカルチャを使います。カルチャ固有の形式で入力を受け付けたい場合は、カスタムコンバーターで変換するか、`string` で入力を受け取ってから解析してください。

### 検証属性

`[PromptBindable]` が生成するバインダーは、モデルの検証属性をコードで生成するため、属性を見つけるのにリフレクションを使いません。ただし、一部の属性はコンストラクターに `[RequiresUnreferencedCode]` が付いていて、扱いが分かれます:

| 属性 | トリミングと Native AOT での扱い |
|------|-------------------------------|
| `[MinLength]`、`[MaxLength]`、`[Length]` | リフレクションを使うのは、文字列でも `ICollection` でもない値の `Count` を読むときだけです。`string`、配列、`ICollection` の値（またはそれらを入力する `List` プロンプト）であれば、生成されたバインダーは警告を追加しません。 |
| `[Compare]`、`[Range(Type, string, string)]` | 実際にリフレクションを使うため、警告はそのまま残ります。可能であれば `[Range(int, int)]` や `[Range(double, double)]` を使ってください。 |

`[MinLength]`、`[MaxLength]`、`[Length]` を書いた行には、コンストラクターに `[RequiresUnreferencedCode]` が付いているため、Sharprompt を使うかどうかにかかわらず、トリミングアナライザーが `IL2026` を報告します。`string` やコレクションのプロパティであれば、その場所で抑制してください:

```csharp
#pragma warning disable IL2026 // MinLength がリフレクションを使うのは、文字列でも ICollection でもない値だけです。
[MinLength(8)]
#pragma warning restore IL2026
public string Password { get; set; } = "";
```

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
