# CodeMonkey Player

libmpv 엔진을 내장한 Windows x64용 .NET Framework 4.8 WinForms 미디어 플레이어입니다.

## 실행

빌드 결과인 CodeMonkeyPlayer/bin/Release/CodeMonkeyPlayer.exe를 실행하세요.
배포할 때는 같은 폴더의 DLL 및 config 파일을 함께 복사해야 합니다.
64비트 Windows와 .NET Framework 4.8이 필요합니다. WMP나 별도 시스템 코덱 설치는 필요하지 않습니다.

## 기능

- 여러 파일 열기 및 영상 영역 / 재생 목록으로 파일 끌어 놓기
- 영상 한 번 클릭으로 재생 / 일시 정지, 두 번 클릭으로 전체 화면 전환
- 재생 / 일시 정지 / 정지, 이전 / 다음 파일
- 재생 위치와 전체 시간 표시, 탐색 막대 및 5초 이동
- 음량 조절 및 음소거
- 목록 순차 자동 재생 및 목록 반복
- 목록 항목 삭제, 전체 비우기 및 중복 파일 방지
- 전체 화면 전환 (하단 조작부 자동 숨김, 마우스를 화면 하단으로 옮기면 표시)
- 실행 인수로 파일 경로 전달
- 파일 누락 및 재생 실패 상태 안내

재생 목록을 두 번 클릭하면 해당 파일을 재생합니다. 파일 추가 시 처음 추가한 파일부터 재생합니다.
정지 또는 재생 완료 후 재생 버튼·Space를 누르면 현재 선택한 목록 항목을 처음부터 재생합니다.
선택 항목이 없으면 마지막 재생 항목(없으면 첫 항목)을 사용하며, 일시 정지 상태에서는 기존 영상을 이어서 재생합니다.
탐색기에서 다른 파일을 열면 기존 사용자 세션의 실행 창에 전달되어 재생 목록에 추가됩니다.
새로 연 파일로 즉시 전환하여 재생합니다. 이미 목록에 있는 파일은 중복 추가하지 않고 해당 항목을 재생합니다.
현재 재생 중인 항목을 삭제하면 남아 있는 다음 항목을 재생합니다.
목록은 프로그램 종료 시 저장되지 않습니다.

MP4, AVI, WMV, MKV, MOV, MPEG, WebM 및 주요 오디오 확장자를 목록에 추가할 수 있습니다.
실제 디코딩 가능 여부는 내장 libmpv와 FFmpeg가 지원하는 형식에 따릅니다.
Windows Media Player용 코덱을 별도로 설치할 필요가 없습니다.

## 설정 저장

설정은 `%LocalAppData%\CodeMonkeyPlayer\CodeMonkeyPlayer.ini`에 자동 생성합니다.
음량, 음소거, 목록 반복, 창 위치·크기·최대화 상태를 변경하면 0.5초 뒤 저장하며 종료 시에도 저장합니다.
다음 실행에서 자동 복원합니다. 프로그램을 종료한 상태에서 직접 편집할 수도 있습니다.

```ini
[CodeMonkeyPlayer]
Volume=70
Muted=false
Repeat=false
```

Volume은 0~100이며 범위를 벗어난 숫자는 범위 내로 제한합니다.
없거나 잘못된 항목은 기본값(음량 70, 음소거·반복 꺼짐)을 사용합니다.
새 설정 파일이 없고 실행 파일 옆에 기존 INI가 있으면 해당 값을 가져옵니다. 기존 파일은 삭제하지 않으며 사용자 폴더의 설정이 항상 우선합니다. 설치 프로그램은 개인 설정을 포함하거나 제거하지 않습니다.

창 배치는 `WindowX`, `WindowY`, `WindowWidth`, `WindowHeight`, `WindowMaximized`로 저장합니다.
최소화 상태로 종료하면 그 이전 일반/최대화 상태를 복원합니다.
전체 화면으로 종료하면 전체 화면 진입 전 창 배치를 복원하며, 전체 화면 자체는 자동 재진입하지 않습니다.
모니터가 제거되거나 해상도가 변경되어 저장 위치가 화면 밖이면 현재 모니터의 작업 영역 안으로 조정합니다.

## 단축키

재생 명령과 mpv 이벤트 처리는 백그라운드 스레드에서 수행하여 UI와 종료를 막지 않도록 구성했습니다.
연결 프로그램의 ProgID에는 Windows 동영상 썸네일 처리기를 등록합니다. Windows가 디코딩하지 못하는 영상은 여전히 아이콘으로 표시될 수 있습니다.

하단의 언어 선택 메뉴에서 총 **18개 언어**를 선택할 수 있습니다.
한국어, 영어, 일본어, 중국어 간체·번체, 스페인어, 프랑스어, 독일어, 포르투갈어(브라질), 러시아어,
아랍어, 힌디어, 이탈리아어, 인도네시아어, 베트남어, 태국어, 튀르키예어, 폴란드어를 지원합니다.
선택 메뉴는 각 언어의 원래 이름을 표시하며, 아랍어 제목·상태·도움말은 오른쪽에서 왼쪽으로 표시합니다.
재생 목록 제목, 버튼 도움말, 접근성 이름, 상태·오류 메시지, 단축키 안내와 파일 열기 창의 제목·필터에 즉시 반영합니다.
마지막 언어는 `%LocalAppData%\CodeMonkeyPlayer\CodeMonkeyPlayer.ini`의 `Language`에 언어 코드로 저장하고 다음 실행 시 복원합니다.
지원 코드: `ko`, `en`, `ja`, `zh-Hans`, `zh-Hant`, `es`, `fr`, `de`, `pt-BR`, `ru`, `ar`, `hi`, `it`, `id`, `vi`, `th`, `tr`, `pl`.
항목이 없거나 지원하지 않는 값이면 한국어를 사용합니다. Windows 기본 대화상자의 공통 버튼은 운영체제 언어를 따릅니다.

| 키 | 동작 |
| --- | --- |
| Ctrl+O | 파일 열기 |
| Space | 재생 / 일시 정지 |
| , / . | 이전 / 다음 프레임으로 이동 후 정지 (코덱 지원 필요) |
| ← / → | 5초 이전 / 이후 |
| ↑ / ↓ | 음량 5단계 조절 |
| M | 음소거 |
| PageUp / PageDown | 이전 / 다음 파일 |
| F11 | 전체 화면 전환 |
| Esc | 전체 화면 종료 |
| Delete | 재생 목록에 포커스가 있을 때 선택 항목 삭제 |

프레임 이동은 mpv의 frame-step / frame-back-step 명령으로 처리합니다.
이전 프레임은 재디코딩이 필요할 수 있어 영상에 따라 지연될 수 있습니다.

## 빌드 및 검증

### 설치 파일 만들기

같은 솔루션의 `CodeMonkeyPlayer.Setup`은 WiX 6 기반 MSI 프로젝트입니다.
Visual Studio의 .NET 데스크톱 개발 도구, .NET Framework 4.8 타기팅 팩과 .NET SDK 6 이상이 필요합니다.
처음 빌드할 때 NuGet에서 고정 버전의 WiX 패키지를 복원합니다.

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File CodeMonkeyPlayer.Setup/Build-Installer.ps1
```

권장 설치 파일: `CodeMonkeyPlayer.Setup/bin/Release/CodeMonkeyPlayer-Setup.exe`
이 EXE에는 MSI가 포함되어 있으므로 파일 하나만 배포하면 됩니다.
언어를 선택하면 설치 화면의 제목, 옵션, 버튼, 진행·완료·오류 안내가 18개 언어로 즉시 바뀝니다.
설치 EXE는 앱 아이콘과 어두운 테마, 설정 카드, 하단 고정 버튼을 사용합니다. 긴 안내는 줄바꿈하고 설정 영역은 스크롤되며, 완료와 오류 상태는 색상으로 구분합니다.
입력한 경로와 옵션은 언어를 변경해도 유지됩니다. 설치 중에는 옵션 변경을 잠시 잠급니다.
선택한 언어는 플레이어의 최초 실행 언어로도 전달합니다.
MSI는 UI 없이 실행하며 자동 재부팅하지 않습니다. 결과와 재부팅 필요 여부를 선택한 언어로 표시하고 임시 폴더의 로그 경로를 제공합니다.
성공한 설치의 MSI는 복구 설치를 위해 `%LocalAppData%\CodeMonkeyPlayer\SetupCache`에 보관합니다.
Windows 관리자 권한 확인창 등 운영체제 UI와 진단 로그는 운영체제 언어를 따릅니다.
`tests/VerifySetupLauncher.ps1`은 실제 설치 없이 번역·옵션 전달·내장 MSI 일치 여부를 검증합니다.

직접 MSI가 필요한 경우 `CodeMonkeyPlayer.Setup/bin/Release/CodeMonkeyPlayer-Setup.msi`도 생성됩니다.
MSI 직접 실행 시 기존 영어 마법사가 열립니다. MSI 설치 순서: 시작 → 설치 옵션 → 설치 폴더 → 설치 확인.
설치 옵션에서 바탕화면 바로가기와 동영상 연결 프로그램 등록 여부, 최초 실행 언어(18개)를 선택합니다.
시작 메뉴 바로가기는 기본으로 생성됩니다.
선택한 언어는 컴퓨터의 설치 기본값으로 기록되고, 처음 실행하는 사용자의 개인 설정 파일에 저장됩니다.
기존 개인 설정에 언어가 있으면 그 값을 우선 적용합니다.

연결 프로그램 등록은 `.mp4`, `.m4v`, `.avi`, `.wmv`, `.mkv`, `.mov`, `.mpg`, `.mpeg`, `.webm`을 지원합니다.
탐색기의 **연결 프로그램**에서 CodeMonkey Player를 선택할 수 있으며, 기본 앱은 Windows 설정에서 사용자가 선택합니다.
제거 시 설치 프로그램이 등록한 연결 프로그램 항목도 제거됩니다.
무인 설치 예: `msiexec /i CodeMonkeyPlayer-Setup.msi /qn DESKTOPSHORTCUT=0 REGISTERVIDEO=1 APPLANGUAGE=ja`.
새 설치·업그레이드에서 바로가기와 연결 프로그램 등록은 기본 선택 상태이며, 복구 설치는 기존 구성 요소를 복구합니다.

MSI는 관리자 권한으로 Program Files에 설치하며 설치 폴더 선택, 시작 메뉴·바탕화면 바로가기,
앱 제거 및 버전 업그레이드를 지원합니다. .NET Framework 4.8을 검사합니다.
필요 구성 요소나 추가 코덱을 자동 설치하지는 않습니다. MSI와 실행 파일에는 코드 서명이 없습니다.
사용자 설정은 앱과 별도로 보관하여 업데이트·제거 시 유지합니다.

설치 프로젝트의 Visual Studio 편집 지원에는 WiX용 확장이 필요할 수 있습니다.
위 스크립트는 Visual Studio 확장 없이도 MSI를 빌드합니다.
버전을 올릴 때 `Package.wxs`의 Version과 앱 어셈블리 버전을 함께 변경하고 UpgradeCode는 유지하세요.

### 플레이어 빌드

Visual Studio에서 CodeMonkeyPlayer.slnx를 열고 빌드하거나,
Visual Studio Developer PowerShell에서 다음 명령을 실행합니다.

```powershell
MSBuild CodeMonkeyPlayer/CodeMonkeyPlayer.csproj /p:Configuration=Release
powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File tests/Smoke.ps1
```

MSBuild는 Visual Studio의 .NET 데스크톱 개발 도구와 .NET Framework 4.8 타기팅 팩이 필요합니다.
COM 참조를 생성하므로 dotnet build 대신 Visual Studio MSBuild를 사용하세요.
WMP 형식 라이브러리 가져오기 과정에서 MSB3305 경고가 발생할 수 있습니다.

스모크 테스트는 임시 무음 WAV 파일로 mpv 엔진 초기화, 재생, 일시 정지,
탐색, 목록 중복 방지, 삭제, 자동 다음 파일 재생, 반복과 컨트롤 영역을 검증합니다.
동영상의 화면 출력, 실제 오디오 출력, 전체 화면의 시각적 배치는 별도 수동 확인이 필요합니다.

내장 엔진의 출처, 고정 해시와 배포 조건은 CodeMonkeyPlayer/Native/THIRD-PARTY-NOTICES.txt와 COPYING-mpv.txt를 참고하세요.
