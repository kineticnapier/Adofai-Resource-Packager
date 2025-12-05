# Adofai-Resource-Packager

ADOFAI の譜面ファイルを読み込み、その譜面内で参照されている音声や画像リソースを自動で検索し、譜面ファイルとまとめて zip に梱包する WPF ツールです。配布やバックアップ時に、必要なリソースを漏れなく一式にまとめられます。

## 特徴
- `.adofai` ファイルと同じフォルダー内にある参照リソース（音声・画像など）を自動検出
- 譜面ファイル本体も含めた zip アーカイブを生成
- GUI でファイル選択・保存先指定が可能
- 上書き保存時は既存の zip を削除して再作成

## 必要環境
- Windows 10 以降（WPF アプリのため）
- [.NET 8 SDK](https://dotnet.microsoft.com/ja-jp/download) または実行環境（`net8.0-windows` 対応）

## インストール / 実行方法
1. リポジトリをクローンします。
   ```bash
   git clone https://github.com/<your-account>/Adofai-Resource-Packager.git
   cd Adofai-Resource-Packager
   ```
2. .NET 8 SDK をインストールした環境でビルドします。
   ```bash
   dotnet build
   ```
3. 開発環境から実行する場合は以下で起動できます。
   ```bash
   dotnet run --project Adofai-Resource-Packager/Adofai-Resource-Packager.csproj
   ```
   生成された `bin/` 配下の `Adofai-Resource-Packager.exe` を直接実行しても動作します。

## 使い方
1. アプリを起動し、`Load File` ボタンで対象の `.adofai` ファイルを選択します（同じフォルダーに参照リソースを置いてください）。
2. `Process` ボタンを押すと、譜面内で参照されているファイルを検出し、譜面ファイルとまとめた zip を作成する保存ダイアログが表示されます。
3. 出力先とファイル名（デフォルトは `<譜面名>_resources.zip`）を指定して保存すると zip が生成されます。

## 動作仕様の補足
- `.adofai` と同じディレクトリにあるファイルのうち、譜面テキスト内にファイル名が含まれるものを対象として zip に追加します。
- zip 内で同名ファイルが重複する場合は二重追加を避けるためスキップします。
- `.adofai` 本体も zip に必ず含めます。

## ライセンス
このプロジェクトは MIT ライセンスの下で公開されています。詳細は [LICENSE.txt](LICENSE.txt) を参照してください。
