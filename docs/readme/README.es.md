# CodeMonkey Player

<!-- languages:start -->
[한국어](../../README.md) · [English](README.en.md) · [日本語](README.ja.md) · [简体中文](README.zh-Hans.md) · [繁體中文](README.zh-Hant.md) · **Español** · [Français](README.fr.md) · [Deutsch](README.de.md) · [Português (Brasil)](README.pt-BR.md) · [Русский](README.ru.md) · [العربية](README.ar.md) · [हिन्दी](README.hi.md) · [Italiano](README.it.md) · [Bahasa Indonesia](README.id.md) · [Tiếng Việt](README.vi.md) · [ไทย](README.th.md) · [Türkçe](README.tr.md) · [Polski](README.pl.md)
<!-- languages:end -->

Reproductor multimedia para Windows x64, desarrollado con WinForms y .NET Framework 4.8, con el motor libmpv integrado.

## Descargar e instalar

Descarga **CodeMonkeyPlayer-Setup.exe** de la [última versión](https://github.com/MisterPark/CodeMonkeyPlayer/releases/latest). Puedes elegir idioma, carpeta, acceso directo en el escritorio y registro en «Abrir con» para vídeos. Requiere Windows x64 y .NET Framework 4.8; no necesita códecs de WMP adicionales. El MSI directo abre un asistente en inglés. La instalación requiere permisos de administrador.

## Funciones

- Arrastra archivos al vídeo o a la lista. Un clic reproduce o pausa; un doble clic cambia a pantalla completa. Los controles se ocultan y reaparecen al acercar el puntero al borde inferior.
- Pulsa la barra de progreso para desplazarte; ajusta el volumen, silencia, avanza o retrocede fotogramas y repite la lista. Retroceder un fotograma puede tardar más por la decodificación necesaria.
- Al abrir otro archivo, se añade sin duplicados a la instancia existente y se reproduce inmediatamente. Tras detenerse o finalizar, Reproducir/Space inicia el elemento seleccionado desde el principio; durante una pausa, continúa el vídeo actual.
- El reproductor y el instalador admiten 18 idiomas. La elección en el menú inferior se guarda. Los diálogos del sistema siguen el idioma de Windows.

Admite MP4, AVI, WMV, MKV, MOV, MPEG, WebM y formatos de audio habituales. La decodificación depende del libmpv/FFmpeg integrado. Las miniaturas del Explorador dependen de Windows y pueden seguir mostrando un icono. La lista no se guarda al salir.

## Configuración

`%LocalAppData%\CodeMonkeyPlayer\CodeMonkeyPlayer.ini` guarda volumen (0–100), silencio, repetición, idioma, posición, tamaño y maximización de la ventana. Valores iniciales: volumen 70, silencio y repetición desactivados. El idioma personal guardado tiene prioridad sobre el elegido al instalar. Las actualizaciones y la desinstalación conservan los ajustes personales. No se restaura automáticamente la pantalla completa.

## Atajos

- `Ctrl+O`: abrir archivos. `Space`: reproducir/pausar. `,` / `.`: fotograma anterior/siguiente y pausa.
- `←` / `→`: retroceder/avanzar 5 segundos. `↑` / `↓`: subir/bajar el volumen en 5. `M`: silenciar.
- `PageUp` / `PageDown`: archivo anterior/siguiente. `F11`: alternar pantalla completa. `Esc`: salir de pantalla completa. `Delete`: eliminar la selección cuando la lista tiene el foco.

## Compilación y verificación

Instala las herramientas de desarrollo de escritorio .NET de Visual Studio y el paquete de destino de .NET Framework 4.8. El instalador WiX 6 también requiere .NET SDK 6 o posterior. Ejecuta estos comandos desde la raíz del repositorio en Developer PowerShell. Restaurar dependencias requiere Internet. Los instaladores se generan en `CodeMonkeyPlayer.Setup/bin/Release/`.

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File CodeMonkeyPlayer/Native/Restore-Mpv.ps1
MSBuild CodeMonkeyPlayer/CodeMonkeyPlayer.csproj /p:Configuration=Release
powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File tests/Smoke.ps1 -Configuration Release
powershell.exe -NoProfile -ExecutionPolicy Bypass -File CodeMonkeyPlayer.Setup/Build-Installer.ps1
powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File tests/VerifySetupLauncher.ps1 -Configuration Release
powershell.exe -NoProfile -ExecutionPolicy Bypass -File tests/VerifyInstaller.ps1
```

## Componentes de terceros

El libmpv integrado es una compilación con GPL habilitada. Consulta las fuentes y condiciones de redistribución en los [avisos de terceros](../../CodeMonkeyPlayer/Native/THIRD-PARTY-NOTICES.txt) y la [licencia de mpv](../../CodeMonkeyPlayer/Native/COPYING-mpv.txt). El EXE y el MSI no tienen firma de código.
