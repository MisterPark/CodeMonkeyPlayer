# CodeMonkey Player

<!-- languages:start -->
[한국어](../../README.md) · [English](README.en.md) · [日本語](README.ja.md) · [简体中文](README.zh-Hans.md) · **繁體中文** · [Español](README.es.md) · [Français](README.fr.md) · [Deutsch](README.de.md) · [Português (Brasil)](README.pt-BR.md) · [Русский](README.ru.md) · [العربية](README.ar.md) · [हिन्दी](README.hi.md) · [Italiano](README.it.md) · [Bahasa Indonesia](README.id.md) · [Tiếng Việt](README.vi.md) · [ไทย](README.th.md) · [Türkçe](README.tr.md) · [Polski](README.pl.md)
<!-- languages:end -->

適用於 Windows x64 的媒體播放器，以 .NET Framework 4.8 和 WinForms 建立，內建 libmpv 引擎。

## 下載與安裝

從[最新版本](https://github.com/MisterPark/CodeMonkeyPlayer/releases/latest)下載 **CodeMonkeyPlayer-Setup.exe**。安裝前可選擇語言、安裝資料夾、桌面捷徑及影片「開啟檔案」應用程式的註冊選項。需要 Windows x64 和 .NET Framework 4.8，無須另裝 WMP 解碼器。直接執行 MSI 會開啟英文精靈。安裝需要系統管理員權限。

## 功能

- 將檔案拖放至影片區域或播放清單。按一下影片可播放／暫停，按兩下切換全螢幕。全螢幕時控制項自動隱藏，將滑鼠移至底部即可顯示。
- 點選進度列跳轉、調整音量、靜音、逐格移動及循環播放清單。移至上一格可能需要重新解碼，因此可能較慢。
- 開啟另一檔案時，會加入現有執行個體的清單並立即播放，不重複加入。停止或播放結束後，播放按鈕／Space 會從頭播放選取項目；暫停時則繼續目前影片。
- 播放器與安裝程式支援18種語言。底部選單的語言選擇會儲存。系統對話方塊依 Windows 語言顯示。

可加入 MP4、AVI、WMV、MKV、MOV、MPEG、WebM 及常見音訊格式。解碼能力取決於內建 libmpv／FFmpeg。檔案總管縮圖依賴 Windows 的解碼支援，部分影片仍可能顯示圖示。結束程式時不儲存播放清單。

## 設定

`%LocalAppData%\CodeMonkeyPlayer\CodeMonkeyPlayer.ini` 儲存音量（0–100）、靜音、循環、語言、視窗位置、大小及最大化狀態。預設音量70，靜音和循環關閉。既有個人語言設定優先於安裝時選擇的語言。更新與解除安裝會保留個人設定，不自動恢復全螢幕。

## 快速鍵

- `Ctrl+O`：開啟檔案。`Space`：播放／暫停。`,`／`.`：上一格／下一格並暫停。
- `←`／`→`：倒退／前進5秒。`↑`／`↓`：音量增減5。`M`：靜音。
- `PageUp`／`PageDown`：上一／下一檔案。`F11`：切換全螢幕。`Esc`：離開全螢幕。`Delete`：清單取得焦點時刪除選取項目。

## 建置與驗證

需要 Visual Studio 的 .NET 桌面開發工具和 .NET Framework 4.8 目標套件；WiX 6 安裝程式還需要 .NET SDK 6 或更新版本。在儲存庫根目錄使用 Developer PowerShell 執行以下命令。還原相依套件需要網路連線。安裝檔輸出至 `CodeMonkeyPlayer.Setup/bin/Release/`。

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File CodeMonkeyPlayer/Native/Restore-Mpv.ps1
MSBuild CodeMonkeyPlayer/CodeMonkeyPlayer.csproj /p:Configuration=Release
powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File tests/Smoke.ps1 -Configuration Release
powershell.exe -NoProfile -ExecutionPolicy Bypass -File CodeMonkeyPlayer.Setup/Build-Installer.ps1
powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File tests/VerifySetupLauncher.ps1 -Configuration Release
powershell.exe -NoProfile -ExecutionPolicy Bypass -File tests/VerifyInstaller.ps1
```

## 第三方元件

內建 libmpv 為啟用 GPL 的建置版本。原始碼來源與再散布要求請參閱[第三方聲明](../../CodeMonkeyPlayer/Native/THIRD-PARTY-NOTICES.txt)及[mpv 授權條款](../../CodeMonkeyPlayer/Native/COPYING-mpv.txt)。EXE 與 MSI 未經程式碼簽章。
