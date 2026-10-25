# 自動テスト

既定のコンソールドライバーは標準入出力のリダイレクトを拒否します。CI や AI コーディング支援による検証には、`Prompt.ConsoleDriverFactory` で `IConsoleDriver` を差し替えます。対話端末なしで公開 API とソース生成したバインディングを検証できます。

## 完全なサンプルを実行する

リポジトリのルートで実行します。

```sh
dotnet run --project samples/Sharprompt.Automation -f net8.0
dotnet run --project samples/Sharprompt.Automation -f net10.0
```

Input、Select、MultiSelect、Confirm、Password、List、バリデーション後の再入力、モデルバインディング、開始後のキャンセルを検証します。結果が異なる場合やキー入力が不足した場合は直ちに失敗します。CI でも両フレームワークを実行します。以下のコードはコンパイル対象のサンプルから直接取り込んでいます。

## Program.cs

<<< @/../samples/Sharprompt.Automation/Program.cs

## ScriptedConsoleDriver.cs

<<< @/../samples/Sharprompt.Automation/ScriptedConsoleDriver.cs

## 自分のテストへの組み込み

- `ConsoleDriverFactory` などの `Prompt` 設定はプロセス全体で共有されます。変更前の値を保存し、`try` / `finally` で復元します。同じ設定を使うテストは並列実行を無効にしてください。
- 各プロンプトはドライバーを所有し、終了時に破棄します。`Bind` の各プロパティも含め、毎回新しいドライバーを返します。プロパティごとに入力が異なる場合は、ドライバーのスクリプトをキューで管理します。
- Escape や `CancellationCallback` を使うドライバーのテストでは `ThrowExceptionOnCancel = true` にします。既定ではユーザーのキャンセル操作がプロセスを終了します。トークンによるキャンセルは常に `OperationCanceledException` です。
- サンプルは `ReadKey(CancellationToken)` を実装して待機を避け、入力不足を例外にします。インターフェイスの既定実装を使う場合、`KeyAvailable` を正確に実装しないと、キャンセル可能なプロンプトが待機し続けることがあります。
- この小さなドライバーは短い ASCII 入力向けです。最終画面ではなく書き込みを記録し、ANSI スタイル、Unicode の表示幅、折り返し、スクロールは再現しません。描画結果の検証には、より完全な端末エミュレーターを使用してください。

## README の例の検証

`dotnet test -c Release` は README のすべての C# ブロックも、標準的なコンソールプロジェクトの暗黙の using を持つ独立プログラムとしてコンパイルします。両ソースジェネレーターも実行します。対話プログラム自体は起動しません。API の変更や、トップレベルステートメントを型宣言の後に置く誤りを検出できます。
