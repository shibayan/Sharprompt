# 設定

Sharprompt では、プロンプトの外観と動作をグローバルにカスタマイズできます。

## シンボル

プロンプトで使用されるシンボルをカスタマイズできます:

```csharp
Prompt.Symbols.Prompt = new Symbol("🤔", "?");
Prompt.Symbols.Done = new Symbol("😎", "V");
Prompt.Symbols.Error = new Symbol("😱", ">>");

var name = Prompt.Input<string>("What's your name?");
```

`Symbol` クラスは 2 つの引数を取ります: Unicode 値と、Unicode をサポートしないターミナル用のフォールバック値です。

### 利用可能なシンボル

| シンボル | デフォルト (Unicode) | デフォルト (フォールバック) | 説明 |
|--------|-------------------|-------------------|-------------|
| `Prompt` | `?` | `?` | プロンプトメッセージの前に表示 |
| `Done` | `✔` | `V` | プロンプト完了時に表示 |
| `Error` | `»` | `>>` | バリデーション失敗時に表示 |
| `Selector` | `›` | `>` | 現在ハイライトされている項目を指す |
| `Selected` | `◉` | `(*)` | MultiSelect で選択済みの項目を示す |
| `NotSelect` | `◯` | `( )` | MultiSelect で未選択の項目を示す |

## カラースキーマ

プロンプトで使用される色をカスタマイズできます:

```csharp
Prompt.ColorSchema.Answer = ConsoleColor.DarkRed;
Prompt.ColorSchema.Select = ConsoleColor.DarkCyan;

var name = Prompt.Input<string>("What's your name?");
```

### 利用可能な色

| プロパティ | デフォルト | 説明 |
|----------|---------|-------------|
| `DoneSymbol` | `Green` | 完了シンボルの色 |
| `PromptSymbol` | `Green` | プロンプトシンボルの色 |
| `Answer` | `Cyan` | ユーザーの回答の色 |
| `Select` | `Green` | 選択項目の色 |
| `Error` | `Red` | エラーメッセージの色 |
| `Hint` | `DarkGray` | ヒントとプレースホルダーの色 |
| `DisabledOption` | `DarkCyan` | 無効な選択肢の色 |

## キャンセルのサポート

デフォルトでは、`Ctrl+C` または `Esc` を押すとプロセスが終了します。この動作を変更して例外をスローさせることができます:

```csharp
Prompt.ThrowExceptionOnCancel = true;

try
{
    var name = Prompt.Input<string>("What's your name?");
    Console.WriteLine($"Hello, {name}!");
}
catch (PromptCanceledException ex)
{
    Console.WriteLine("Prompt canceled");
}
```

### コードからのキャンセル

すべてのプロンプトは、オプションオブジェクトまたは構成デリゲートを受け取るオーバーロードで `CancellationToken` を指定できます。`Prompt.Bind` も同様です。入力待ちの間にトークンがキャンセルされると、プロンプトを閉じて `OperationCanceledException` をスローします。この動作は `ThrowExceptionOnCancel` の設定に関係ありません。

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

try
{
    var name = Prompt.Input<string>(options => options.Message = "What's your name?", cts.Token);
    Console.WriteLine($"Hello, {name}!");
}
catch (OperationCanceledException)
{
    Console.WriteLine("Timed out");
}
```

プロンプトを呼び出した時点でトークンがキャンセル済みの場合は、コンソールドライバーを作成する前に `OperationCanceledException` をスローします。

カスタムコンソールドライバーを実装する場合: プロンプトは `IConsoleDriver.ReadKey(CancellationToken)` を通じてキー入力を待機します。トークンがキャンセル可能な場合、既定の実装は `KeyAvailable` をポーリングするため、ドライバーは `KeyAvailable` を正しく返す必要があります。入力ソース自体がキャンセルに対応している場合や `KeyAvailable` を提供できない場合は、`ReadKey(CancellationToken)` をオーバーライドし、トークンがキャンセルされたらそのトークンを指定した `OperationCanceledException` をスローしてください。

## カルチャ

ローカライズされたメッセージのカルチャを設定します:

```csharp
Prompt.Culture = new CultureInfo("ja");
```

## コンソールドライバー

カスタムコンソールドライバーファクトリを提供できます:

```csharp
Prompt.ConsoleDriverFactory = () => new MyCustomConsoleDriver();
```
