# CodeMonkey Player

<!-- languages:start -->
[한국어](../../README.md) · [English](README.en.md) · **日本語** · [简体中文](README.zh-Hans.md) · [繁體中文](README.zh-Hant.md) · [Español](README.es.md) · [Français](README.fr.md) · [Deutsch](README.de.md) · [Português (Brasil)](README.pt-BR.md) · [Русский](README.ru.md) · [العربية](README.ar.md) · [हिन्दी](README.hi.md) · [Italiano](README.it.md) · [Bahasa Indonesia](README.id.md) · [Tiếng Việt](README.vi.md) · [ไทย](README.th.md) · [Türkçe](README.tr.md) · [Polski](README.pl.md)
<!-- languages:end -->

libmpvを内蔵した、Windows x64向けの.NET Framework 4.8／WinFormsメディアプレーヤーです。

## ダウンロードとインストール

[最新リリース](https://github.com/MisterPark/CodeMonkeyPlayer/releases/latest)から **CodeMonkeyPlayer-Setup.exe** をダウンロードしてください。言語、インストール先、デスクトップのショートカット、動画の「プログラムから開く」への登録を選択できます。Windows x64と.NET Framework 4.8が必要です。WMP用コーデックの追加は不要です。MSIを直接実行すると英語のウィザードが開きます。インストールには管理者権限が必要です。

## 主な機能

- 動画領域やプレイリストへファイルをドロップできます。動画をクリックすると再生・一時停止、ダブルクリックすると全画面になります。全画面では操作部が隠れ、ポインターを下端へ移動すると再表示されます。
- シークバーのクリック、音量・ミュート、コマ送り・コマ戻し、リストのリピートに対応します。コマ戻しは再デコードが必要な場合があり、時間がかかることがあります。
- 別のファイルを開くと既存のプロセスのリストに追加し、すぐ再生します。重複登録はしません。停止・再生終了後の再生ボタン／Spaceは選択項目を先頭から再生し、一時停止中は現在の動画を再開します。
- 本体とインストーラーは18言語に対応します。下部のメニューで言語を選ぶと保存されます。システムダイアログはWindowsの言語に従います。

MP4、AVI、WMV、MKV、MOV、MPEG、WebMと主な音声形式に対応します。デコードは内蔵libmpv／FFmpegに依存します。エクスプローラーのサムネイルはWindows側の対応に依存するため、アイコンのままの場合があります。プレイリストは終了時に保存されません。

## 設定

`%LocalAppData%\CodeMonkeyPlayer\CodeMonkeyPlayer.ini`に音量（0～100）、ミュート、リピート、言語、ウィンドウ位置・サイズ・最大化状態を保存します。初期値は音量70、ミュートとリピートは無効です。保存済みの個人言語設定はインストール時の選択より優先されます。更新・アンインストール後も個人設定は残ります。全画面は自動復元しません。

## キーボード操作

- `Ctrl+O`：ファイルを開く。`Space`：再生／一時停止。`,`／`.`：前／次のフレームへ移動して一時停止。
- `←`／`→`：5秒戻す／進める。`↑`／`↓`：音量を5増減。`M`：ミュート。
- `PageUp`／`PageDown`：前／次のファイル。`F11`：全画面切り替え。`Esc`：全画面解除。`Delete`：リストにフォーカスがあるとき選択項目を削除。

## ビルドと検証

Visual Studioの.NETデスクトップ開発ツールと.NET Framework 4.8ターゲットパックが必要です。WiX 6インストーラーには.NET SDK 6以降も必要です。リポジトリのルートでDeveloper PowerShellから以下を実行します。依存関係の復元にはインターネット接続が必要です。インストーラーの出力先は `CodeMonkeyPlayer.Setup/bin/Release/` です。

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File CodeMonkeyPlayer/Native/Restore-Mpv.ps1
MSBuild CodeMonkeyPlayer/CodeMonkeyPlayer.csproj /p:Configuration=Release
powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File tests/Smoke.ps1 -Configuration Release
powershell.exe -NoProfile -ExecutionPolicy Bypass -File CodeMonkeyPlayer.Setup/Build-Installer.ps1
powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File tests/VerifySetupLauncher.ps1 -Configuration Release
powershell.exe -NoProfile -ExecutionPolicy Bypass -File tests/VerifyInstaller.ps1
```

## 外部コンポーネント

内蔵libmpvはGPLを有効にしたビルドです。[外部コンポーネントの告知](../../CodeMonkeyPlayer/Native/THIRD-PARTY-NOTICES.txt)と[mpvのライセンス](../../CodeMonkeyPlayer/Native/COPYING-mpv.txt)でソースと再配布条件を確認してください。EXEとMSIはコード署名されていません。
