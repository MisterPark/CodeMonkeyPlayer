# CodeMonkey Player

<!-- languages:start -->
[한국어](../../README.md) · [English](README.en.md) · [日本語](README.ja.md) · [简体中文](README.zh-Hans.md) · [繁體中文](README.zh-Hant.md) · [Español](README.es.md) · [Français](README.fr.md) · **Deutsch** · [Português (Brasil)](README.pt-BR.md) · [Русский](README.ru.md) · [العربية](README.ar.md) · [हिन्दी](README.hi.md) · [Italiano](README.it.md) · [Bahasa Indonesia](README.id.md) · [Tiếng Việt](README.vi.md) · [ไทย](README.th.md) · [Türkçe](README.tr.md) · [Polski](README.pl.md)
<!-- languages:end -->

Ein Mediaplayer für Windows x64 mit WinForms, .NET Framework 4.8 und integrierter libmpv-Engine.

## Download und Installation

Lade **CodeMonkeyPlayer-Setup.exe** aus der [aktuellen Veröffentlichung](https://github.com/MisterPark/CodeMonkeyPlayer/releases/latest) herunter. Sprache, Installationsordner, Desktopverknüpfung und Registrierung unter „Öffnen mit“ für Videos sind wählbar. Windows x64 und .NET Framework 4.8 werden benötigt; zusätzliche WMP-Codecs sind unnötig. Die direkt gestartete MSI zeigt einen englischen Assistenten. Die Installation benötigt Administratorrechte.

## Funktionen

- Dateien auf das Video oder die Wiedergabeliste ziehen. Ein Klick startet oder pausiert, ein Doppelklick wechselt zum Vollbild. Ausgeblendete Bedienelemente erscheinen wieder, wenn der Mauszeiger den unteren Rand erreicht.
- Per Fortschrittsleiste springen, Lautstärke ändern, stummschalten, Einzelbilder vor- und zurückgehen und die Liste wiederholen. Ein Bild zurück kann wegen erneuter Dekodierung länger dauern.
- Eine weitere Datei wird ohne Duplikat zur bestehenden Instanz hinzugefügt und sofort abgespielt. Nach Stopp oder Wiedergabeende startet Wiedergabe/Space den ausgewählten Eintrag von vorn; bei Pause wird das aktuelle Video fortgesetzt.
- Player und Installationsprogramm unterstützen 18 Sprachen. Die Auswahl im unteren Menü wird gespeichert. Systemdialoge verwenden die Windows-Sprache.

MP4, AVI, WMV, MKV, MOV, MPEG, WebM und gängige Audioformate werden angenommen. Die Dekodierung hängt vom integrierten libmpv/FFmpeg ab. Explorer-Vorschaubilder benötigen Windows-Unterstützung und können weiterhin als Symbol erscheinen. Die Liste wird beim Beenden nicht gespeichert.

## Einstellungen

`%LocalAppData%\CodeMonkeyPlayer\CodeMonkeyPlayer.ini` speichert Lautstärke (0–100), Stummschaltung, Wiederholung, Sprache sowie Fensterposition, Größe und Maximierungszustand. Standard: Lautstärke 70, Stummschaltung und Wiederholung aus. Die gespeicherte persönliche Sprache hat Vorrang vor der Installationssprache. Aktualisierungen und Deinstallation erhalten persönliche Einstellungen. Vollbild wird nicht automatisch wiederhergestellt.

## Tastenkürzel

- `Ctrl+O`: Dateien öffnen. `Space`: Wiedergabe/Pause. `,` / `.`: vorheriges/nächstes Einzelbild, dann Pause.
- `←` / `→`: 5 Sekunden zurück/vor. `↑` / `↓`: Lautstärke um 5 erhöhen/verringern. `M`: stummschalten.
- `PageUp` / `PageDown`: vorherige/nächste Datei. `F11`: Vollbild umschalten. `Esc`: Vollbild verlassen. `Delete`: Auswahl entfernen, wenn die Liste den Fokus hat.

## Erstellen und Prüfen

Benötigt werden Visual Studios .NET-Desktopentwicklungstools und das .NET Framework 4.8 Targeting Pack. Für den WiX-6-Installer ist zusätzlich .NET SDK 6 oder neuer erforderlich. Die Befehle im Stammverzeichnis des Repositorys in Developer PowerShell ausführen. Zum Wiederherstellen der Abhängigkeiten ist Internet nötig. Die Installationsdateien entstehen unter `CodeMonkeyPlayer.Setup/bin/Release/`.

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File CodeMonkeyPlayer/Native/Restore-Mpv.ps1
MSBuild CodeMonkeyPlayer/CodeMonkeyPlayer.csproj /p:Configuration=Release
powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File tests/Smoke.ps1 -Configuration Release
powershell.exe -NoProfile -ExecutionPolicy Bypass -File CodeMonkeyPlayer.Setup/Build-Installer.ps1
powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File tests/VerifySetupLauncher.ps1 -Configuration Release
powershell.exe -NoProfile -ExecutionPolicy Bypass -File tests/VerifyInstaller.ps1
```

## Drittanbieterkomponenten

Das integrierte libmpv wurde mit aktivierter GPL gebaut. Quellverweise und Weitergabebedingungen stehen in den [Drittanbieterhinweisen](../../CodeMonkeyPlayer/Native/THIRD-PARTY-NOTICES.txt) und der [mpv-Lizenz](../../CodeMonkeyPlayer/Native/COPYING-mpv.txt). EXE und MSI besitzen keine Codesignatur.
