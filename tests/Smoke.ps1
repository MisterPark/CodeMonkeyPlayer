param([string]$Configuration = "Release")
$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.Windows.Forms
Add-Type @'
using System;
using System.Runtime.InteropServices;
public static class SeekMouseInput {
    [DllImport("user32.dll")]
    public static extern IntPtr SendMessage(IntPtr handle, int message, IntPtr wParam, IntPtr lParam);
}
'@
Add-Type -ReferencedAssemblies System.Windows.Forms @'
using System;
using System.Collections.Specialized;
using System.Windows.Forms;
public static class VideoDropInput {
    public static uint Drop(object target, string path) {
        var data = new DataObject();
        data.SetFileDropList(new StringCollection { path });
        var method = target.GetType().GetMethod("Drop");
        var point = Activator.CreateInstance(method.GetParameters()[2].ParameterType);
        object[] args = { data, (uint)0, point, (uint)1 };
        method.Invoke(target, args);
        return (uint)args[3];
    }
}
'@
$root = Split-Path -Parent $PSScriptRoot
$bin = Join-Path $root "CodeMonkeyPlayer/bin/$Configuration"
$assembly = [Reflection.Assembly]::LoadFrom((Join-Path $bin 'CodeMonkeyPlayer.exe'))
$testPreferences = Join-Path $PSScriptRoot ('settings-test-'+[Guid]::NewGuid().ToString('N'))
$constructor = $assembly.GetType('CodeMonkeyPlayer.Form1').GetConstructor([Reflection.BindingFlags]'Instance,NonPublic',$null,[type[]]@([string],[string]),$null)
$form = $constructor.Invoke(@((Join-Path $testPreferences 'CodeMonkeyPlayer.ini').PSObject.BaseObject,(Join-Path $testPreferences 'legacy.ini').PSObject.BaseObject))
$flags = [Reflection.BindingFlags]'Instance,NonPublic'
function Field($name) { $form.GetType().GetField($name,$flags).GetValue($form) }
function InvokePrivate($name, [object[]]$arguments) { $form.GetType().GetMethod($name,$flags).Invoke($form,$arguments) }
function Assert($condition,$message) { if (!$condition) { throw $message } }
function Pump([int]$milliseconds) {
    $watch = [Diagnostics.Stopwatch]::StartNew()
    while ($watch.ElapsedMilliseconds -lt $milliseconds) {
        [Windows.Forms.Application]::DoEvents()
        Start-Sleep -Milliseconds 20
    }
}
$wav = Join-Path $PSScriptRoot 'smoke.wav'
try {
    $stream = [IO.File]::Create($wav)
    $writer = New-Object IO.BinaryWriter($stream)
    $samples = 8000 * 3
    $writer.Write([Text.Encoding]::ASCII.GetBytes('RIFF'))
    $writer.Write([int](36 + $samples * 2))
    $writer.Write([Text.Encoding]::ASCII.GetBytes('WAVEfmt '))
    $writer.Write([int]16)
    $writer.Write([int16]1)
    $writer.Write([int16]1)
    $writer.Write([int]8000)
    $writer.Write([int]16000)
    $writer.Write([int16]2)
    $writer.Write([int16]16)
    $writer.Write([Text.Encoding]::ASCII.GetBytes('data'))
    $writer.Write([int]($samples * 2))
    $writer.Write((New-Object byte[] ($samples * 2)))
    $writer.Dispose()
    # Create ActiveX handles without displaying a window.
    $form.CreateControl()
    $player = Field 'video'
    $player.CreateControl()
    InvokePrivate 'OnShown' @([EventArgs]::Empty) | Out-Null
    $player.Volume = 0
    Assert (InvokePrivate 'HandleShortcut' @([Windows.Forms.Keys]::Oemcomma)) 'Previous-frame shortcut was not handled without media'
    Assert (InvokePrivate 'HandleShortcut' @([Windows.Forms.Keys]::OemPeriod)) 'Next-frame shortcut was not handled without media'
    InvokePrivate 'AddFiles' (,([string[]]@($wav, $wav))) | Out-Null
    Pump 1200
    Assert ((Field 'playlist').Items.Count -eq 1) 'Duplicate filtering failed'
    Assert ($player.Duration -gt 0) 'Media duration was not loaded'
    Assert ($player.State -eq 'Playing') 'Media did not start playing'
    Assert (InvokePrivate 'HandleShortcut' @([Windows.Forms.Keys]::OemPeriod)) 'Next-frame shortcut was not handled'
    Assert (InvokePrivate 'HandleShortcut' @([Windows.Forms.Keys]::Oemcomma)) 'Previous-frame shortcut was not handled'
    InvokePrivate 'VideoClicked' @([int]1) | Out-Null
    Pump 75
    Assert ($player.State -eq 'Paused') 'Video click did not pause playback'
    InvokePrivate 'VideoClicked' @([int]1) | Out-Null
    Pump 75
    Assert ($player.State -eq 'Playing') 'Video click did not resume playback'
    InvokePrivate 'VideoClicked' @([int]1) | Out-Null
    InvokePrivate 'VideoDoubleClicked' @([int]1) | Out-Null
    Assert (!(Field 'videoClickTimer').Enabled) 'Double click left a pending playback toggle'
    Assert ((Field 'fullscreen')) 'Double click did not enter fullscreen'
    Pump 100
    Assert ($player.State -eq 'Playing') 'Double click changed playback state'
    InvokePrivate 'VideoDoubleClicked' @([int]1) | Out-Null
    InvokePrivate 'TogglePlayback' @() | Out-Null
    Pump 100
    Assert ($player.State -eq 'Paused') 'Pause failed'
    (Field 'videoClickTimer').Stop()
    InvokePrivate 'VideoClicked' @([int]1) | Out-Null
    InvokePrivate 'VideoDoubleClicked' @([int]1) | Out-Null
    Pump 75
    Assert ($player.State -eq 'Paused') 'Double click failed to preserve paused playback'
    InvokePrivate 'VideoDoubleClicked' @([int]1) | Out-Null
    InvokePrivate 'TogglePlayback' @() | Out-Null
    Pump 100
    Assert ($player.State -eq 'Playing') 'Resume failed'
    InvokePrivate 'UpdatePlayback' @() | Out-Null
    Assert ((Field 'seek').Enabled) 'Seek control was not enabled'
    (Field 'seek').Value = 5000
    InvokePrivate 'CommitSeek' @() | Out-Null
    Pump 150
    Assert ($player.Position -ge 1) 'Seek failed'
    $seekControl = Field 'seek'
    $seekControl.CreateControl()
    $seekControl.Value = 9000
    $player.Seek(1.0)
    [SeekMouseInput]::SendMessage($seekControl.Handle, 0x0101, [IntPtr]190, [IntPtr]0) | Out-Null
    Pump 150
    Assert ([Math]::Abs($player.Position - 1.0) -lt 0.2) 'Frame key release rewound playback to the stale slider position'
    $seekControl.Value = 0
    $player.Seek(0)
    $clickPoint = [IntPtr](($seekControl.Width / 2) -bor (15 -shl 16))
    [SeekMouseInput]::SendMessage($seekControl.Handle, 0x0201, [IntPtr]1, $clickPoint) | Out-Null
    Assert ([Math]::Abs($seekControl.Value - 5000) -lt 200) 'Click did not select the middle of the timeline'
    Pump 150
    Assert ([Math]::Abs($player.Position - 1.5) -lt 0.2) 'Mouse down did not seek immediately'
    [SeekMouseInput]::SendMessage($seekControl.Handle, 0x0202, [IntPtr]0, $clickPoint) | Out-Null
    InvokePrivate 'ClearPlaylist' @() | Out-Null
    Assert ((Field 'playlist').Items.Count -eq 0) 'Clear failed'
    Assert ((Field 'currentIndex') -eq -1) 'Current index was not reset'
    $dropTarget = Field 'videoDropTarget'
    $registered = $dropTarget.GetType().GetField('registered',$flags).GetValue($dropTarget)
    Assert ($registered.Count -gt 0) 'Video window drop target was not registered'
    $oleTarget = $dropTarget.GetType().GetField('target',$flags).GetValue($dropTarget)
    Assert ([VideoDropInput]::Drop($oleTarget, $wav) -eq 1) 'Video drop did not accept file data'
    Assert ((Field 'playlist').Items.Count -eq 1) 'Video drop did not populate playlist'
    InvokePrivate 'ClearPlaylist' @() | Out-Null
    InvokePrivate 'AddFiles' (,([string[]]@($wav))) | Out-Null
    Pump 200
    InvokePrivate 'RemoveSelected' @() | Out-Null
    Assert ((Field 'playlist').Items.Count -eq 0) 'Remove failed'
    $second = Join-Path $PSScriptRoot 'smoke-second.wav'
    Copy-Item -LiteralPath $wav -Destination $second
    InvokePrivate 'AddFiles' (,([string[]]@($wav, $second))) | Out-Null
    Pump 4000
    Assert ((Field 'currentIndex') -eq 1) 'Automatic next track failed'
    (Field 'repeat').Checked = $true
    $player.Seek(2.7)
    Pump 1300
    Assert ((Field 'currentIndex') -eq 0) 'Playlist repeat failed'
    (Field 'repeat').Checked=$false
    $player.Stop()
    Pump 150
    (Field 'playlist').SelectedIndex=1
    (Field 'play').GetType().GetMethod('OnClick',$flags).Invoke((Field 'play'),@([EventArgs]::Empty)) | Out-Null
    Pump 350
    Assert ($player.State -eq 'Playing' -and (Field 'currentIndex') -eq 1) 'Play button did not start selected item after stop'
    $player.Seek(2.8)
    Pump 700
    Assert ($player.State -eq 'Ended') 'Last track did not finish'
    (Field 'playlist').SelectedIndex=0
    InvokePrivate 'HandleShortcut' @([Windows.Forms.Keys]::Space) | Out-Null
    Pump 350
    Assert ($player.State -eq 'Playing' -and (Field 'currentIndex') -eq 0 -and $player.Position -lt 1) 'Space did not restart selected item after EOF'
    $player.Pause()
    Pump 150
    (Field 'playlist').SelectedIndex=1
    InvokePrivate 'HandleShortcut' @([Windows.Forms.Keys]::Space) | Out-Null
    Pump 150
    Assert ($player.State -eq 'Playing' -and (Field 'currentIndex') -eq 0) 'Paused playback unexpectedly switched to selected item'
    Write-Output 'PASS: selected-item playback after stop/EOF via button/Space; paused resume preserves current track.'
    Assert ($player.Bounds.Right -le (Field 'sidebar').Left) 'Video overlaps playlist'
    Write-Output 'PASS: automatic next track, playlist repeat, layout.'
    Write-Output 'PASS: video click pause/resume, double-click fullscreen, OLE video drop registration and file delivery.'
    Write-Output 'PASS: mpv initialization, media playback, pause/resume, seek, duplicate filtering, clear, remove.'
}
finally {
    $form.Dispose()
    if ($second -and (Test-Path -LiteralPath $second)) { Remove-Item -LiteralPath $second }
    if (Test-Path -LiteralPath $wav) { Remove-Item -LiteralPath $wav }
}




