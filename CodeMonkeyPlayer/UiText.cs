using System;
using System.Collections.Generic;

namespace CodeMonkeyPlayer
{
    internal sealed partial class UiText
    {
        public string Language { get; set; } = "ko";
        public static string Normalize(string language)
        {
            foreach (var option in Languages)
                if (string.Equals(language, option.Code, StringComparison.OrdinalIgnoreCase)) return option.Code;
            return "ko";
        }
        private static readonly Dictionary<string, string> English = new Dictionary<string, string>
        {
            { "재생 목록", "Playlist" }, { "추가", "Add files" }, { "삭제", "Remove selected" },
            { "비우기", "Clear playlist" }, { "이전", "Previous" }, { "다음", "Next" },
            { "재생", "Play" }, { "일시 정지", "Pause" }, { "정지", "Stop" },
            { "음소거", "Mute" }, { "음소거 해제", "Unmute" }, { "음량", "Volume" },
            { "목록 반복 켜기", "Enable playlist repeat" }, { "목록 반복 끄기", "Disable playlist repeat" },
            { "전체 화면", "Full screen" }, { "도움말", "Help" }, { "사용 방법", "Keyboard shortcuts" },
            { "파일 열기", "Open files" }, { "재생 위치", "Playback position" }, { "언어", "Language" },
            { "재생할 파일 선택", "Select media files" }, { "미디어 파일", "Media files" }, { "모든 파일", "All files" },
            { "파일을 열거나 영상 / 재생 목록으로 끌어다 놓으세요.", "Open files or drop them onto the video or playlist." },
            { "재생할 수 없습니다. 파일 손상 여부와 Windows 코덱 지원을 확인하세요.", "Unable to play this file. Check the file and installed Windows codecs." },
            { "설정을 저장하지 못했습니다. 사용자 설정 폴더의 쓰기 권한을 확인하세요.", "Unable to save settings. Check write access to the user settings folder." },
            { "{0}개 파일을 추가하지 못했습니다. 지원하는 미디어 파일인지 확인하세요.", "Could not add {0} file(s). Check that they are supported media files." },
            { "파일을 찾을 수 없습니다: {0}", "File not found: {0}" }, { "여는 중: {0}", "Opening: {0}" },
            { "재생 중: {0}", "Playing: {0}" }, { "버퍼링 중…", "Buffering…" }, { "재생 완료", "Playback finished" },
            { "이 파일 또는 코덱은 프레임 이동을 지원하지 않습니다.", "This file or codec does not support frame stepping." },
            { "영상의 FPS를 확인할 수 없어 이전 프레임 이동을 사용할 수 없습니다.", "Previous-frame stepping is unavailable because the frame rate could not be read." },
            { "이 코덱은 정밀 탐색을 지원하지 않아 프레임 이동을 완료하지 못했습니다.", "Frame stepping could not finish because the codec does not support precise seeking." },
            { "프레임을 이동하지 못했습니다. 파일 또는 코덱의 지원 여부를 확인하세요.", "Unable to step a frame. Check file and codec support." },
            { "단축키 안내", "Ctrl+O: Open files\nSpace: Play / pause\n, / .: Previous / next frame (supported videos)\nLeft / Right: Seek 5 seconds\nUp / Down: Volume\nM: Mute\nPageUp / PageDown: Previous / next file\nF11: Full screen\nEsc: Exit full screen\nDelete: Remove selected playlist item\n\nDrop files onto the video or double-click a playlist item." }
        };
        public string Get(string key)
        {
            if (Translations.TryGetValue(Language, out var texts) && texts.TryGetValue(key, out string localized)) return localized;
            if (Language == "en" && English.TryGetValue(key, out string translation)) return translation;
            if (key == "단축키 안내") return "Ctrl+O: 파일 열기\nSpace: 재생 / 일시 정지\n, / .: 이전 / 다음 프레임 (지원하는 영상)\n← / →: 5초 이동\n↑ / ↓: 음량 조절\nM: 음소거\nPageUp / PageDown: 이전 / 다음 파일\nF11: 전체 화면\nEsc: 전체 화면 종료\nDelete: 선택한 목록 항목 삭제\n\n파일을 영상에 끌어다 놓거나 재생 목록을 두 번 클릭하세요.";
            return key;
        }
    }
}
