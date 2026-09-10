# CodeMonkey Player

<!-- languages:start -->
[한국어](../../README.md) · [English](README.en.md) · [日本語](README.ja.md) · [简体中文](README.zh-Hans.md) · [繁體中文](README.zh-Hant.md) · [Español](README.es.md) · [Français](README.fr.md) · [Deutsch](README.de.md) · **Português (Brasil)** · [Русский](README.ru.md) · [العربية](README.ar.md) · [हिन्दी](README.hi.md) · [Italiano](README.it.md) · [Bahasa Indonesia](README.id.md) · [Tiếng Việt](README.vi.md) · [ไทย](README.th.md) · [Türkçe](README.tr.md) · [Polski](README.pl.md)
<!-- languages:end -->

Reprodutor de mídia para Windows x64, desenvolvido com WinForms e .NET Framework 4.8, com o mecanismo libmpv integrado.

## Download e instalação

Baixe **CodeMonkeyPlayer-Setup.exe** na [versão mais recente](https://github.com/MisterPark/CodeMonkeyPlayer/releases/latest). Escolha idioma, pasta, atalho na área de trabalho e registro em “Abrir com” para vídeos. Requer Windows x64 e .NET Framework 4.8, sem codecs adicionais do WMP. Executar o MSI diretamente abre um assistente em inglês. A instalação requer privilégios de administrador.

## Recursos

- Arraste arquivos para o vídeo ou a lista. Um clique reproduz ou pausa; um clique duplo alterna a tela cheia. Os controles ocultos reaparecem ao mover o ponteiro até a borda inferior.
- Clique na barra de progresso para buscar uma posição; ajuste o volume, silencie, avance ou recue quadros e repita a lista. Recuar um quadro pode demorar por exigir nova decodificação.
- Abrir outro arquivo o adiciona sem duplicação à instância existente e inicia a reprodução imediatamente. Após parar ou terminar, Reproduzir/Space inicia o item selecionado desde o começo; durante uma pausa, retoma o vídeo atual.
- O programa e o instalador oferecem 18 idiomas. A escolha no menu inferior é salva. Os diálogos do sistema seguem o idioma do Windows.

Aceita MP4, AVI, WMV, MKV, MOV, MPEG, WebM e formatos comuns de áudio. A decodificação depende do libmpv/FFmpeg integrado. As miniaturas do Explorador dependem do Windows e podem continuar exibindo um ícone. A lista não é salva ao sair.

## Configurações

`%LocalAppData%\CodeMonkeyPlayer\CodeMonkeyPlayer.ini` salva volume (0–100), silêncio, repetição, idioma, posição, tamanho e estado maximizado da janela. Padrões: volume 70, silêncio e repetição desativados. O idioma pessoal salvo tem prioridade sobre o escolhido na instalação. Atualizações e desinstalação preservam as configurações pessoais. A tela cheia não é restaurada automaticamente.

## Atalhos

- `Ctrl+O`: abrir arquivos. `Space`: reproduzir/pausar. `,` / `.`: quadro anterior/seguinte e pausa.
- `←` / `→`: voltar/avançar 5 segundos. `↑` / `↓`: aumentar/diminuir o volume em 5. `M`: silenciar.
- `PageUp` / `PageDown`: arquivo anterior/seguinte. `F11`: alternar tela cheia. `Esc`: sair da tela cheia. `Delete`: remover a seleção quando a lista estiver em foco.

## Compilação e verificação

Instale as ferramentas de desenvolvimento para desktop .NET do Visual Studio e o pacote de direcionamento do .NET Framework 4.8. O instalador WiX 6 também exige .NET SDK 6 ou mais recente. Execute os comandos na raiz do repositório pelo Developer PowerShell. A restauração das dependências precisa de Internet. Os instaladores são gerados em `CodeMonkeyPlayer.Setup/bin/Release/`.

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File CodeMonkeyPlayer/Native/Restore-Mpv.ps1
MSBuild CodeMonkeyPlayer/CodeMonkeyPlayer.csproj /p:Configuration=Release
powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File tests/Smoke.ps1 -Configuration Release
powershell.exe -NoProfile -ExecutionPolicy Bypass -File CodeMonkeyPlayer.Setup/Build-Installer.ps1
powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File tests/VerifySetupLauncher.ps1 -Configuration Release
powershell.exe -NoProfile -ExecutionPolicy Bypass -File tests/VerifyInstaller.ps1
```

## Componentes de terceiros

O libmpv integrado foi compilado com GPL habilitada. Consulte as fontes e condições de redistribuição nos [avisos de terceiros](../../CodeMonkeyPlayer/Native/THIRD-PARTY-NOTICES.txt) e na [licença do mpv](../../CodeMonkeyPlayer/Native/COPYING-mpv.txt). O EXE e o MSI não têm assinatura de código.
