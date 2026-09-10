# CodeMonkey Player

<!-- languages:start -->
[한국어](../../README.md) · [English](README.en.md) · [日本語](README.ja.md) · **简体中文** · [繁體中文](README.zh-Hant.md) · [Español](README.es.md) · [Français](README.fr.md) · [Deutsch](README.de.md) · [Português (Brasil)](README.pt-BR.md) · [Русский](README.ru.md) · [العربية](README.ar.md) · [हिन्दी](README.hi.md) · [Italiano](README.it.md) · [Bahasa Indonesia](README.id.md) · [Tiếng Việt](README.vi.md) · [ไทย](README.th.md) · [Türkçe](README.tr.md) · [Polski](README.pl.md)
<!-- languages:end -->

适用于 Windows x64 的媒体播放器，基于 .NET Framework 4.8 和 WinForms，内置 libmpv 引擎。

## 下载与安装

从[最新版本](https://github.com/MisterPark/CodeMonkeyPlayer/releases/latest)下载 **CodeMonkeyPlayer-Setup.exe**。安装前可选择语言、安装目录、桌面快捷方式及视频“打开方式”注册选项。需要 Windows x64 和 .NET Framework 4.8，无需另装 WMP 解码器。直接运行 MSI 会打开英文向导。安装需要管理员权限。

## 功能

- 将文件拖放到视频区域或播放列表。单击视频可播放／暂停，双击切换全屏。全屏时控件自动隐藏，将鼠标移至底部即可显示。
- 点击进度条跳转，调节音量、静音、逐帧移动及循环播放列表。向前一帧移动可能需要重新解码，因此可能较慢。
- 打开另一文件时，将其添加到现有进程的列表并立即播放，不重复添加。停止或播放结束后，播放按钮／Space 会从头播放选中项；暂停时则继续当前视频。
- 播放器和安装程序支持18种语言。底部菜单的语言选择会保存。系统对话框遵循 Windows 语言。

可添加 MP4、AVI、WMV、MKV、MOV、MPEG、WebM 及常见音频格式。解码能力取决于内置 libmpv／FFmpeg。资源管理器缩略图依赖 Windows 解码支持，部分视频仍可能显示图标。退出时不保存播放列表。

## 设置

`%LocalAppData%\CodeMonkeyPlayer\CodeMonkeyPlayer.ini` 保存音量（0–100）、静音、循环、语言、窗口位置、大小及最大化状态。默认音量70，静音和循环关闭。已有个人语言设置优先于安装时选择的语言。更新和卸载保留个人设置，不自动恢复全屏。

## 快捷键

- `Ctrl+O`：打开文件。`Space`：播放／暂停。`,`／`.`：上一帧／下一帧并暂停。
- `←`／`→`：后退／前进5秒。`↑`／`↓`：音量增减5。`M`：静音。
- `PageUp`／`PageDown`：上一／下一文件。`F11`：切换全屏。`Esc`：退出全屏。`Delete`：列表获得焦点时删除选中项。

## 构建与验证

需要 Visual Studio 的 .NET 桌面开发工具和 .NET Framework 4.8 目标包；WiX 6 安装程序还需要 .NET SDK 6 或更新版本。在仓库根目录使用 Developer PowerShell 执行以下命令。恢复依赖项需要联网。安装文件输出至 `CodeMonkeyPlayer.Setup/bin/Release/`。

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File CodeMonkeyPlayer/Native/Restore-Mpv.ps1
MSBuild CodeMonkeyPlayer/CodeMonkeyPlayer.csproj /p:Configuration=Release
powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File tests/Smoke.ps1 -Configuration Release
powershell.exe -NoProfile -ExecutionPolicy Bypass -File CodeMonkeyPlayer.Setup/Build-Installer.ps1
powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File tests/VerifySetupLauncher.ps1 -Configuration Release
powershell.exe -NoProfile -ExecutionPolicy Bypass -File tests/VerifyInstaller.ps1
```

## 第三方组件

内置 libmpv 为启用 GPL 的构建版本。源码来源和再分发要求见[第三方声明](../../CodeMonkeyPlayer/Native/THIRD-PARTY-NOTICES.txt)及[mpv 许可证](../../CodeMonkeyPlayer/Native/COPYING-mpv.txt)。EXE 和 MSI 未进行代码签名。
