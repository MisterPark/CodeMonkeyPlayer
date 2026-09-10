# CodeMonkey Player

<!-- languages:start -->
[한국어](../../README.md) · [English](README.en.md) · [日本語](README.ja.md) · [简体中文](README.zh-Hans.md) · [繁體中文](README.zh-Hant.md) · [Español](README.es.md) · [Français](README.fr.md) · [Deutsch](README.de.md) · [Português (Brasil)](README.pt-BR.md) · [Русский](README.ru.md) · [العربية](README.ar.md) · [हिन्दी](README.hi.md) · **Italiano** · [Bahasa Indonesia](README.id.md) · [Tiếng Việt](README.vi.md) · [ไทย](README.th.md) · [Türkçe](README.tr.md) · [Polski](README.pl.md)
<!-- languages:end -->

Lettore multimediale per Windows x64, sviluppato con WinForms e .NET Framework 4.8, con motore libmpv integrato.

## Download e installazione

Scarica **CodeMonkeyPlayer-Setup.exe** dall’[ultima versione](https://github.com/MisterPark/CodeMonkeyPlayer/releases/latest). Scegli lingua, cartella, collegamento sul desktop e registrazione in «Apri con» per i video. Servono Windows x64 e .NET Framework 4.8; non occorrono codec WMP aggiuntivi. Il MSI avviato direttamente apre una procedura guidata in inglese. L’installazione richiede privilegi di amministratore.

## Funzioni

- Trascina i file sul video o sulla playlist. Un clic avvia o mette in pausa; un doppio clic alterna lo schermo intero. I controlli nascosti riappaiono spostando il puntatore sul bordo inferiore.
- Clicca sulla barra di avanzamento per spostarti; regola il volume, disattiva l’audio, avanza o torna indietro di un fotogramma e ripeti la lista. Il ritorno al fotogramma precedente può richiedere una nuova decodifica e più tempo.
- Aprire un altro file lo aggiunge senza duplicati all’istanza esistente e lo riproduce subito. Dopo l’arresto o la fine, Riproduci/Space avvia dall’inizio l’elemento selezionato; durante la pausa riprende il video attuale.
- Lettore e programma di installazione supportano 18 lingue. La scelta nel menu inferiore viene salvata. Le finestre di sistema seguono la lingua di Windows.

Accetta MP4, AVI, WMV, MKV, MOV, MPEG, WebM e formati audio comuni. La decodifica dipende da libmpv/FFmpeg integrato. Le miniature di Esplora file dipendono da Windows e possono restare icone. La playlist non viene salvata all’uscita.

## Impostazioni

`%LocalAppData%\CodeMonkeyPlayer\CodeMonkeyPlayer.ini` salva volume (0–100), audio disattivato, ripetizione, lingua, posizione, dimensioni e stato ingrandito della finestra. Valori iniziali: volume 70, audio attivo e ripetizione disattivata. La lingua personale salvata ha precedenza su quella dell’installazione. Aggiornamenti e disinstallazione conservano le impostazioni personali. Lo schermo intero non viene ripristinato automaticamente.

## Scorciatoie

- `Ctrl+O`: apri file. `Space`: riproduci/pausa. `,` / `.`: fotogramma precedente/successivo, poi pausa.
- `←` / `→`: indietro/avanti di 5 secondi. `↑` / `↓`: aumenta/riduci il volume di 5. `M`: disattiva l’audio.
- `PageUp` / `PageDown`: file precedente/successivo. `F11`: alterna schermo intero. `Esc`: esci dallo schermo intero. `Delete`: elimina la selezione quando la playlist ha il focus.

## Compilazione e verifica

Installa gli strumenti di sviluppo desktop .NET di Visual Studio e il targeting pack di .NET Framework 4.8. L’installatore WiX 6 richiede anche .NET SDK 6 o successivo. Esegui questi comandi dalla radice del repository in Developer PowerShell. Il ripristino delle dipendenze richiede Internet. I file di installazione vengono creati in `CodeMonkeyPlayer.Setup/bin/Release/`.

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File CodeMonkeyPlayer/Native/Restore-Mpv.ps1
MSBuild CodeMonkeyPlayer/CodeMonkeyPlayer.csproj /p:Configuration=Release
powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File tests/Smoke.ps1 -Configuration Release
powershell.exe -NoProfile -ExecutionPolicy Bypass -File CodeMonkeyPlayer.Setup/Build-Installer.ps1
powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File tests/VerifySetupLauncher.ps1 -Configuration Release
powershell.exe -NoProfile -ExecutionPolicy Bypass -File tests/VerifyInstaller.ps1
```

## Componenti di terze parti

Il libmpv integrato è compilato con GPL abilitata. Fonti e condizioni di ridistribuzione sono negli [avvisi di terze parti](../../CodeMonkeyPlayer/Native/THIRD-PARTY-NOTICES.txt) e nella [licenza mpv](../../CodeMonkeyPlayer/Native/COPYING-mpv.txt). EXE e MSI non hanno una firma del codice.
