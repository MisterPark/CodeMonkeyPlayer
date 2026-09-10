# CodeMonkey Player

<!-- languages:start -->
[한국어](../../README.md) · [English](README.en.md) · [日本語](README.ja.md) · [简体中文](README.zh-Hans.md) · [繁體中文](README.zh-Hant.md) · [Español](README.es.md) · [Français](README.fr.md) · [Deutsch](README.de.md) · [Português (Brasil)](README.pt-BR.md) · **Русский** · [العربية](README.ar.md) · [हिन्दी](README.hi.md) · [Italiano](README.it.md) · [Bahasa Indonesia](README.id.md) · [Tiếng Việt](README.vi.md) · [ไทย](README.th.md) · [Türkçe](README.tr.md) · [Polski](README.pl.md)
<!-- languages:end -->

Медиаплеер для Windows x64 на WinForms и .NET Framework 4.8 со встроенным движком libmpv.

## Загрузка и установка

Скачайте **CodeMonkeyPlayer-Setup.exe** из [последнего выпуска](https://github.com/MisterPark/CodeMonkeyPlayer/releases/latest). Перед установкой можно выбрать язык, папку, ярлык на рабочем столе и регистрацию в меню «Открыть с помощью» для видео. Нужны Windows x64 и .NET Framework 4.8; дополнительные кодеки WMP не требуются. При прямом запуске MSI открывается англоязычный мастер. Для установки нужны права администратора.

## Возможности

- Перетаскивайте файлы на видео или в список. Щелчок включает воспроизведение или паузу, двойной щелчок переключает полноэкранный режим. Скрытые элементы управления появляются при перемещении указателя к нижнему краю.
- Переход по шкале времени, громкость, отключение звука, покадровое перемещение и повтор списка. Переход на предыдущий кадр может занимать больше времени из-за повторного декодирования.
- Другой файл добавляется без дубликатов в уже запущенный экземпляр и сразу воспроизводится. После остановки или завершения кнопка воспроизведения/Space запускает выбранный элемент с начала; при паузе продолжается текущее видео.
- Плеер и установщик поддерживают 18 языков. Выбор в нижнем меню сохраняется. Системные диалоги используют язык Windows.

Можно добавлять MP4, AVI, WMV, MKV, MOV, MPEG, WebM и распространённые аудиоформаты. Декодирование зависит от встроенного libmpv/FFmpeg. Миниатюры Проводника зависят от поддержки Windows: вместо них может остаться значок. При выходе список не сохраняется.

## Настройки

В `%LocalAppData%\CodeMonkeyPlayer\CodeMonkeyPlayer.ini` сохраняются громкость (0–100), отключение звука, повтор, язык, положение, размер и развёрнутое состояние окна. По умолчанию громкость 70, отключение звука и повтор выключены. Сохранённый личный язык важнее выбранного при установке. Обновление и удаление программы сохраняют личные настройки. Полноэкранный режим автоматически не восстанавливается.

## Клавиши

- `Ctrl+O`: открыть файлы. `Space`: воспроизведение/пауза. `,` / `.`: предыдущий/следующий кадр с паузой.
- `←` / `→`: назад/вперёд на 5 секунд. `↑` / `↓`: увеличить/уменьшить громкость на 5. `M`: отключить звук.
- `PageUp` / `PageDown`: предыдущий/следующий файл. `F11`: переключить полноэкранный режим. `Esc`: выйти из него. `Delete`: удалить выбранные элементы, когда список находится в фокусе.

## Сборка и проверка

Нужны инструменты разработки классических приложений .NET в Visual Studio и пакет нацеливания .NET Framework 4.8. Для установщика WiX 6 также нужен .NET SDK 6 или новее. Выполните команды в корне репозитория через Developer PowerShell. Восстановление зависимостей требует Интернета. Установщики появятся в `CodeMonkeyPlayer.Setup/bin/Release/`.

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File CodeMonkeyPlayer/Native/Restore-Mpv.ps1
MSBuild CodeMonkeyPlayer/CodeMonkeyPlayer.csproj /p:Configuration=Release
powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File tests/Smoke.ps1 -Configuration Release
powershell.exe -NoProfile -ExecutionPolicy Bypass -File CodeMonkeyPlayer.Setup/Build-Installer.ps1
powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File tests/VerifySetupLauncher.ps1 -Configuration Release
powershell.exe -NoProfile -ExecutionPolicy Bypass -File tests/VerifyInstaller.ps1
```

## Сторонние компоненты

Встроенный libmpv собран с включённой GPL. Источники и условия распространения приведены в [уведомлениях о сторонних компонентах](../../CodeMonkeyPlayer/Native/THIRD-PARTY-NOTICES.txt) и [лицензии mpv](../../CodeMonkeyPlayer/Native/COPYING-mpv.txt). EXE и MSI не имеют цифровой подписи кода.
