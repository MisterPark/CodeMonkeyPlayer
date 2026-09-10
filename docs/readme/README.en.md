# CodeMonkey Player

<!-- languages:start -->
[한국어](../../README.md) · **English** · [日本語](README.ja.md) · [简体中文](README.zh-Hans.md) · [繁體中文](README.zh-Hant.md) · [Español](README.es.md) · [Français](README.fr.md) · [Deutsch](README.de.md) · [Português (Brasil)](README.pt-BR.md) · [Русский](README.ru.md) · [العربية](README.ar.md) · [हिन्दी](README.hi.md) · [Italiano](README.it.md) · [Bahasa Indonesia](README.id.md) · [Tiếng Việt](README.vi.md) · [ไทย](README.th.md) · [Türkçe](README.tr.md) · [Polski](README.pl.md)
<!-- languages:end -->

A Windows x64 media player built with WinForms and .NET Framework 4.8, with an embedded libmpv engine.

## Download and install

Download **CodeMonkeyPlayer-Setup.exe** from the [latest release](https://github.com/MisterPark/CodeMonkeyPlayer/releases/latest). Choose the language, installation folder, desktop shortcut and video “Open with” registration before installing. Windows x64 and .NET Framework 4.8 are required; separate WMP codecs are unnecessary. Running the MSI directly opens an English wizard. Installation requires administrator rights.

## Features

- Drop files onto the video or playlist. Click the video to play/pause; double-click to toggle full screen. Controls hide in full screen and reappear when the pointer reaches the bottom.
- Seek by clicking the timeline, adjust volume, mute, step frames and repeat the playlist. Backward frame stepping may take longer because it can require decoding again.
- Opening another file adds it to the existing instance and immediately plays it without duplicating playlist entries. After stopping or reaching the end, Play/Space starts the selected item from the beginning; paused playback resumes the current video.
- The player and setup launcher support 18 languages. Select a language in the bottom controls; the choice is saved. System dialogs follow the Windows language.

MP4, AVI, WMV, MKV, MOV, MPEG, WebM and common audio formats are accepted. Decoding depends on the embedded libmpv/FFmpeg build. Explorer thumbnails depend on Windows decoding support and may still show an icon. Playlists are not saved on exit.

## Settings

Settings are stored in `%LocalAppData%\CodeMonkeyPlayer\CodeMonkeyPlayer.ini`: volume (0–100), mute, repeat, language and window position, size and maximized state. Defaults are volume 70, mute off and repeat off. An existing personal language takes priority over the language chosen during installation. Updates and uninstallation preserve personal settings. Full screen is not restored automatically.

## Keyboard shortcuts

| Key | Action |
| --- | --- |
| Ctrl+O | Open files |
| Space | Play / pause |
| , / . | Previous / next frame, then pause |
| ← / → | Seek backward / forward 5 seconds |
| ↑ / ↓ | Increase / decrease volume by 5 |
| M | Mute |
| PageUp / PageDown | Previous / next file |
| F11 / Esc | Toggle / exit full screen |
| Delete | Remove selected items when the playlist has focus |

## Build and verify

Run the commands below from the repository root in Visual Studio Developer PowerShell. Install the .NET desktop development tools and .NET Framework 4.8 targeting pack; building the WiX 6 installer also requires .NET SDK 6 or newer. Engine and WiX package restoration requires an internet connection. The setup script produces `CodeMonkeyPlayer.Setup/bin/Release/CodeMonkeyPlayer-Setup.exe` and the MSI.

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File CodeMonkeyPlayer/Native/Restore-Mpv.ps1
MSBuild CodeMonkeyPlayer/CodeMonkeyPlayer.csproj /p:Configuration=Release
powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File tests/Smoke.ps1 -Configuration Release
powershell.exe -NoProfile -ExecutionPolicy Bypass -File CodeMonkeyPlayer.Setup/Build-Installer.ps1
powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File tests/VerifySetupLauncher.ps1 -Configuration Release
powershell.exe -NoProfile -ExecutionPolicy Bypass -File tests/VerifyInstaller.ps1
```

## Third-party components

The embedded libmpv is a GPL-enabled build. See [third-party notices](../../CodeMonkeyPlayer/Native/THIRD-PARTY-NOTICES.txt) and [the mpv license](../../CodeMonkeyPlayer/Native/COPYING-mpv.txt) for source references and redistribution requirements. The EXE and MSI are not code-signed.
