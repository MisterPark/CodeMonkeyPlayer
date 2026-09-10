# CodeMonkey Player

<!-- languages:start -->
[한국어](../../README.md) · [English](README.en.md) · [日本語](README.ja.md) · [简体中文](README.zh-Hans.md) · [繁體中文](README.zh-Hant.md) · [Español](README.es.md) · **Français** · [Deutsch](README.de.md) · [Português (Brasil)](README.pt-BR.md) · [Русский](README.ru.md) · [العربية](README.ar.md) · [हिन्दी](README.hi.md) · [Italiano](README.it.md) · [Bahasa Indonesia](README.id.md) · [Tiếng Việt](README.vi.md) · [ไทย](README.th.md) · [Türkçe](README.tr.md) · [Polski](README.pl.md)
<!-- languages:end -->

Lecteur multimédia pour Windows x64, développé avec WinForms et .NET Framework 4.8, intégrant le moteur libmpv.

## Téléchargement et installation

Téléchargez **CodeMonkeyPlayer-Setup.exe** depuis la [dernière version](https://github.com/MisterPark/CodeMonkeyPlayer/releases/latest). Choisissez la langue, le dossier, le raccourci sur le bureau et l’ajout à « Ouvrir avec » pour les vidéos. Windows x64 et .NET Framework 4.8 sont nécessaires, sans codec WMP supplémentaire. Le MSI lancé directement affiche un assistant en anglais. L’installation nécessite les droits administrateur.

## Fonctionnalités

- Déposez des fichiers sur la vidéo ou la liste de lecture. Un clic lance ou suspend la lecture ; un double-clic bascule en plein écran. Les commandes masquées réapparaissent lorsque le pointeur atteint le bas de l’écran.
- Cliquez sur la barre de progression pour naviguer ; réglez le volume, coupez le son, avancez ou reculez image par image et répétez la liste. Le recul d’une image peut prendre davantage de temps en raison du décodage.
- Un autre fichier s’ajoute sans doublon à l’instance existante et démarre immédiatement. Après un arrêt ou une fin de lecture, Lecture/Space démarre la sélection depuis le début ; en pause, la vidéo actuelle reprend.
- Le lecteur et l’installateur proposent 18 langues. Le choix du menu inférieur est enregistré. Les boîtes de dialogue système suivent la langue de Windows.

MP4, AVI, WMV, MKV, MOV, MPEG, WebM et les formats audio courants sont acceptés. Le décodage dépend de libmpv/FFmpeg intégré. Les miniatures de l’Explorateur dépendent de Windows et peuvent rester des icônes. La liste n’est pas enregistrée à la fermeture.

## Paramètres

`%LocalAppData%\CodeMonkeyPlayer\CodeMonkeyPlayer.ini` conserve le volume (0–100), le mode muet, la répétition, la langue, la position, les dimensions et l’état agrandi de la fenêtre. Par défaut : volume 70, son activé et répétition désactivée. La langue personnelle enregistrée prime sur celle choisie à l’installation. Les mises à jour et la désinstallation conservent les paramètres personnels. Le plein écran n’est pas restauré automatiquement.

## Raccourcis

- `Ctrl+O` : ouvrir des fichiers. `Space` : lecture/pause. `,` / `.` : image précédente/suivante, puis pause.
- `←` / `→` : reculer/avancer de 5 secondes. `↑` / `↓` : augmenter/diminuer le volume de 5. `M` : couper le son.
- `PageUp` / `PageDown` : fichier précédent/suivant. `F11` : basculer en plein écran. `Esc` : quitter le plein écran. `Delete` : supprimer la sélection lorsque la liste a le focus.

## Compilation et vérification

Installez les outils de développement de bureau .NET de Visual Studio et le pack de ciblage .NET Framework 4.8. L’installateur WiX 6 nécessite aussi .NET SDK 6 ou ultérieur. Exécutez ces commandes à la racine du dépôt dans Developer PowerShell. La restauration des dépendances nécessite Internet. Les installateurs sont produits dans `CodeMonkeyPlayer.Setup/bin/Release/`.

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File CodeMonkeyPlayer/Native/Restore-Mpv.ps1
MSBuild CodeMonkeyPlayer/CodeMonkeyPlayer.csproj /p:Configuration=Release
powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File tests/Smoke.ps1 -Configuration Release
powershell.exe -NoProfile -ExecutionPolicy Bypass -File CodeMonkeyPlayer.Setup/Build-Installer.ps1
powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File tests/VerifySetupLauncher.ps1 -Configuration Release
powershell.exe -NoProfile -ExecutionPolicy Bypass -File tests/VerifyInstaller.ps1
```

## Composants tiers

Le libmpv intégré est compilé avec la GPL activée. Consultez les sources et conditions de redistribution dans les [mentions des composants tiers](../../CodeMonkeyPlayer/Native/THIRD-PARTY-NOTICES.txt) et la [licence mpv](../../CodeMonkeyPlayer/Native/COPYING-mpv.txt). Les fichiers EXE et MSI ne sont pas signés numériquement.
