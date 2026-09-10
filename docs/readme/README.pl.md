# CodeMonkey Player

<!-- languages:start -->
[한국어](../../README.md) · [English](README.en.md) · [日本語](README.ja.md) · [简体中文](README.zh-Hans.md) · [繁體中文](README.zh-Hant.md) · [Español](README.es.md) · [Français](README.fr.md) · [Deutsch](README.de.md) · [Português (Brasil)](README.pt-BR.md) · [Русский](README.ru.md) · [العربية](README.ar.md) · [हिन्दी](README.hi.md) · [Italiano](README.it.md) · [Bahasa Indonesia](README.id.md) · [Tiếng Việt](README.vi.md) · [ไทย](README.th.md) · [Türkçe](README.tr.md) · **Polski**
<!-- languages:end -->

Odtwarzacz multimediów dla Windows x64, oparty na WinForms i .NET Framework 4.8, z wbudowanym silnikiem libmpv.

## Pobieranie i instalacja

Pobierz **CodeMonkeyPlayer-Setup.exe** z [najnowszego wydania](https://github.com/MisterPark/CodeMonkeyPlayer/releases/latest). Wybierz język, folder, skrót na pulpicie i rejestrację w menu „Otwórz za pomocą” dla filmów. Wymagane są Windows x64 i .NET Framework 4.8; dodatkowe kodeki WMP nie są potrzebne. Bezpośrednie uruchomienie MSI otwiera angielski kreator. Instalacja wymaga uprawnień administratora.

## Funkcje

- Przeciągaj pliki na film lub listę. Kliknięcie odtwarza/wstrzymuje, a dwukrotne kliknięcie przełącza pełny ekran. Ukryte elementy sterowania pojawiają się po przesunięciu wskaźnika do dolnej krawędzi.
- Klikaj pasek postępu, aby przewijać; reguluj głośność, wyciszaj, przechodź po klatkach i powtarzaj listę. Cofnięcie o klatkę może wymagać ponownego dekodowania i zająć więcej czasu.
- Otwarcie innego pliku dodaje go bez duplikatu do działającej instancji i od razu odtwarza. Po zatrzymaniu lub zakończeniu przycisk Odtwórz/Space uruchamia zaznaczony element od początku; podczas pauzy wznawia bieżący film.
- Odtwarzacz i instalator obsługują 18 języków. Wybór w dolnym menu jest zapisywany. Okna systemowe używają języka Windows.

Można dodawać MP4, AVI, WMV, MKV, MOV, MPEG, WebM i popularne formaty audio. Dekodowanie zależy od wbudowanego libmpv/FFmpeg. Miniatury Eksploratora zależą od obsługi w Windows i mogą pozostać ikonami. Lista nie jest zapisywana przy zamknięciu.

## Ustawienia

`%LocalAppData%\CodeMonkeyPlayer\CodeMonkeyPlayer.ini` zapisuje głośność (0–100), wyciszenie, powtarzanie, język, położenie, rozmiar i stan maksymalizacji okna. Domyślnie głośność wynosi 70, a wyciszenie i powtarzanie są wyłączone. Zapisany język użytkownika ma pierwszeństwo przed językiem instalacji. Aktualizacja i odinstalowanie zachowują ustawienia osobiste. Pełny ekran nie jest automatycznie przywracany.

## Skróty

- `Ctrl+O`: otwórz pliki. `Space`: odtwarzanie/pauza. `,` / `.`: poprzednia/następna klatka, potem pauza.
- `←` / `→`: cofnij/przewiń o 5 sekund. `↑` / `↓`: zwiększ/zmniejsz głośność o 5. `M`: wycisz.
- `PageUp` / `PageDown`: poprzedni/następny plik. `F11`: przełącz pełny ekran. `Esc`: wyjdź z pełnego ekranu. `Delete`: usuń zaznaczenie, gdy lista ma fokus.

## Budowanie i weryfikacja

Potrzebne są narzędzia Visual Studio do programowania aplikacji klasycznych .NET i pakiet docelowy .NET Framework 4.8. Instalator WiX 6 wymaga także .NET SDK 6 lub nowszego. Uruchom polecenia z katalogu głównego repozytorium w Developer PowerShell. Przywrócenie zależności wymaga Internetu. Instalatory powstają w `CodeMonkeyPlayer.Setup/bin/Release/`.

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File CodeMonkeyPlayer/Native/Restore-Mpv.ps1
MSBuild CodeMonkeyPlayer/CodeMonkeyPlayer.csproj /p:Configuration=Release
powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File tests/Smoke.ps1 -Configuration Release
powershell.exe -NoProfile -ExecutionPolicy Bypass -File CodeMonkeyPlayer.Setup/Build-Installer.ps1
powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File tests/VerifySetupLauncher.ps1 -Configuration Release
powershell.exe -NoProfile -ExecutionPolicy Bypass -File tests/VerifyInstaller.ps1
```

## Komponenty zewnętrzne

Wbudowany libmpv jest kompilowany z włączoną GPL. Źródła i warunki redystrybucji podano w [informacjach o komponentach zewnętrznych](../../CodeMonkeyPlayer/Native/THIRD-PARTY-NOTICES.txt) oraz [licencji mpv](../../CodeMonkeyPlayer/Native/COPYING-mpv.txt). EXE i MSI nie mają podpisu kodu.
