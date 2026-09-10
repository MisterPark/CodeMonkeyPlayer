# CodeMonkey Player

Windows Media Player ActiveX를 사용하는 .NET Framework 4.8 WinForms 미디어 플레이어입니다.

## 실행

빌드 결과인 CodeMonkeyPlayer/bin/Release/CodeMonkeyPlayer.exe를 실행하세요.
배포할 때는 같은 폴더의 DLL 및 config 파일을 함께 복사해야 합니다.
Windows Media Player(레거시) 구성 요소와 .NET Framework 4.8이 필요합니다.

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
현재 재생 중인 항목을 삭제하면 남아 있는 다음 항목을 재생합니다.
목록은 프로그램 종료 시 저장되지 않습니다.

MP4, AVI, WMV, MKV, MOV, MPEG, WebM 및 주요 오디오 확장자를 목록에 추가할 수 있습니다.
실제 디코딩 가능 여부는 Windows Media Player와 설치된 코덱에 따릅니다.
이 프로그램은 별도 코덱을 포함하지 않습니다.

## 설정 저장

실행 파일과 같은 폴더에 `CodeMonkeyPlayer.ini`를 첫 실행 시 자동 생성합니다.
음량, 음소거, 목록 반복을 변경하면 0.5초 뒤 저장하며 종료 시에도 저장합니다.
다음 실행에서 자동 복원합니다. 프로그램을 종료한 상태에서 직접 편집할 수도 있습니다.

```ini
[CodeMonkeyPlayer]
Volume=70
Muted=false
Repeat=false
```

Volume은 0~100이며 범위를 벗어난 숫자는 범위 내로 제한합니다.
없거나 잘못된 항목은 기본값(음량 70, 음소거·반복 꺼짐)을 사용합니다.
프로그램 폴더에 쓰기 권한이 없으면 상태 안내에 저장 실패를 표시합니다.

## 단축키

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

프레임 이동은 일시 정지 완료 후 순차 처리합니다. 역방향은 WMP의 키프레임 점프를 피하기 위해
Windows 파일 속성의 FPS를 이용해 탐색한 뒤 정방향 디코딩으로 화면을 갱신합니다.
FPS를 읽을 수 없는 파일은 역방향 이동을 실행하지 않고 안내합니다.
가변 프레임 영상이나 일부 코덱에서는 정확한 한 프레임 이동에 한계가 있습니다.

## 빌드 및 검증

Visual Studio에서 CodeMonkeyPlayer.slnx를 열고 빌드하거나,
Visual Studio Developer PowerShell에서 다음 명령을 실행합니다.

```powershell
MSBuild CodeMonkeyPlayer/CodeMonkeyPlayer.csproj /p:Configuration=Release
powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File tests/Smoke.ps1
```

MSBuild는 Visual Studio의 .NET 데스크톱 개발 도구와 .NET Framework 4.8 타기팅 팩이 필요합니다.
COM 참조를 생성하므로 dotnet build 대신 Visual Studio MSBuild를 사용하세요.
WMP 형식 라이브러리 가져오기 과정에서 MSB3305 경고가 발생할 수 있습니다.

스모크 테스트는 임시 무음 WAV 파일로 실제 WMP 컨트롤 초기화, 재생, 일시 정지,
탐색, 목록 중복 방지, 삭제, 자동 다음 파일 재생, 반복과 컨트롤 영역을 검증합니다.
동영상의 화면 출력, 실제 오디오 출력, 전체 화면의 시각적 배치는 별도 수동 확인이 필요합니다.
