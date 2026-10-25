# 利用ガイド

Sharprompt は .NET 8 / .NET 10 向けの同期型・対話型コンソールライブラリです。NuGet の `Sharprompt` を追加し、`using Sharprompt;` を記述します。ソースジェネレーターと API の XML ドキュメントはパッケージに含まれます。ジェネレーター用パッケージの追加は不要です。

## プロンプトの選び方

| 用途 | API | 戻り値・既定動作 |
| --- | --- | --- |
| 型付きの値を入力 | `Prompt.Input<T>(message, ...)` | invariant culture の `TypeConverter` で変換した値 |
| パスワードを入力 | `Prompt.Password(message, ...)` | 入力をマスク表示し、平文の `string` を返す。ログに出力しない |
| はい／いいえ | `Prompt.Confirm(message, defaultValue: ...)` | `bool`。既定値がなければ明示的な入力が必要 |
| 1 件を選択 | `Prompt.Select(message, items, ...)` | 表示文字列ではなく元の項目 |
| 複数件を選択 | `Prompt.MultiSelect(message, items, ...)` | `IEnumerable<T>`。既定では 1 件以上。`minimum: 0` で未選択を許可 |
| リストを入力 | `Prompt.List<T>(message, ...)` | `IEnumerable<T>`。`minimum` 件（既定は 1 件）以上で空行を入力すると終了 |
| モデルを入力 | `Prompt.Bind<T>()` / `Prompt.Bind(model)` | ソース生成したバインダーで入力したモデル |

`Bind` 以外は、メッセージ引数・Options オブジェクト・Options を設定するデリゲートの 3 形式に対応します。簡単な呼び出しはメッセージ形式、詳細設定やキャンセルには Options／デリゲート形式を使用します。Fluent API には `using Sharprompt.Fluent;` が必要です。

## 実行できる最小例

`dotnet new console` で作成し、`dotnet add package Sharprompt` で追加して、`Program.cs` を次の内容に置き換えます。

```csharp
using Sharprompt;

var name = Prompt.Input<string>("Name", validators: new[] { Validators.Required() });
var city = Prompt.Select("City", new[] { "Tokyo", "London" });
var ready = Prompt.Confirm("Continue?", defaultValue: true);
Console.WriteLine($"{name}, {city}, ready: {ready}");
```

対話端末で実行してください。無人実行での検証は[自動テスト](/ja/testing)を参照してください。

## 既定値とバリデーション

- 既定値は `T` と互換性のある値にします。互換性のあるプリミティブ型間の変換や、列挙型に対する整数値も使用できます。`Input<T>` では Enter で採用、Tab で編集用に挿入、未入力で Backspace を押すと解除します。
- 列挙型の `Select<T>` / `MultiSelect<T>` は `items` を省略できます。それ以外では項目を指定します。`textSelector` は表示と絞り込みに使われ、戻り値は元の項目です。
- 入力変換は `Prompt.Culture` にかかわらず invariant culture です。小数は `1.5` のように入力します。
- バリデーターは成功時に `ValidationResult.Success`（`null`）、失敗時にエラーを含む `ValidationResult` を返します。`Required()` は null と空文字列を拒否し、空白だけの文字列は許可します。長さ・正規表現のバリデーターは文字列以外を許可します。`RegularExpression()` はアンカーを付けない場合、部分一致です。

詳細は[バリデーター](/ja/validators)と[型変換](/ja/advanced)を参照してください。

## モデルバインディングの制約

クラスまたは record class に `[PromptBindable]` を付け、公開インスタンスプロパティにアクセス可能な `get` / `set` を用意します。`Prompt.Bind<T>()` には公開の引数なしコンストラクターが必要です。既存インスタンスは `Prompt.Bind(model)` に渡せます。

record の本体に `{ get; set; }` を宣言してください。位置パラメーターから生成されるプロパティなどの init-only プロパティはスキップされます（`SHARP002`）。ジェネリックモデルは非対応です。対象プロパティがなければバインダーは生成されません（`SHARP014`）。実行時リフレクションによるバインダーへのフォールバックはありません。

単一の `Program.cs` では、トップレベルステートメントを型宣言の前に置きます。完全な例、対応するコレクション型、`SHARP001`〜`SHARP014` の原因と修正方法は [Bind](/ja/prompt-types/bind)を参照してください。

## キャンセルと自動実行

既定ドライバーは対話型の標準入出力を必要とします。どちらかをリダイレクトすると `InvalidOperationException` になります。パイプで文字列を渡してもキー入力の代わりにはなりません。[自動テスト](/ja/testing)では `Prompt.ConsoleDriverFactory` にスクリプトドライバーを指定します。アプリ側でコマンドライン引数を受け取り、指定済みの値はプロンプトを省略する設計も可能です。Sharprompt 自体は引数を解析しません。

既定では Ctrl+C / Escape で終了コード 1 とともにプロセスが終了します。`Prompt.ThrowExceptionOnCancel = true` で `PromptCanceledException` に変更できます。`CancellationToken` によるキャンセルは設定にかかわらず `OperationCanceledException` です。トークンは `Prompt.Confirm(options => options.Message = "Continue?", token)` のように Options／デリゲート形式で渡します。メッセージ形式にはトークン引数はありません。`Prompt.Bind<T>(token)` も使用できます。

設定はプロセス全体で共有されます。変更後は `finally` で元に戻し、設定を書き換えるテストは直列実行してください。[設定](/ja/configuration)と [Native AOT](/ja/advanced)も参照してください。
