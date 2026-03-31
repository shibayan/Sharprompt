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

デフォルトでは、`Ctrl+C` を押すとデフォルト値が返されます。この動作を変更して例外をスローさせることができます:

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
