param([Parameter(Mandatory=$true)][string]$MediaPath, [string]$Configuration='Release-frames')
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Windows.Forms
$root=Split-Path -Parent $PSScriptRoot
$bin=Join-Path $root "CodeMonkeyPlayer/bin/$Configuration"
[Reflection.Assembly]::LoadFrom((Join-Path $bin 'Interop.WMPLib.dll')) | Out-Null
[Reflection.Assembly]::LoadFrom((Join-Path $bin 'AxInterop.WMPLib.dll')) | Out-Null
$a=[Reflection.Assembly]::LoadFrom((Join-Path $bin 'CodeMonkeyPlayer.exe'))
$testPreferences=Join-Path $PSScriptRoot ('settings-test-'+[Guid]::NewGuid().ToString('N'))
$constructor=$a.GetType('CodeMonkeyPlayer.Form1').GetConstructor([Reflection.BindingFlags]'Instance,NonPublic',$null,[type[]]@([string],[string]),$null)
$f=$constructor.Invoke(@((Join-Path $testPreferences 'CodeMonkeyPlayer.ini').PSObject.BaseObject,(Join-Path $testPreferences 'legacy.ini').PSObject.BaseObject))
$flags=[Reflection.BindingFlags]'Instance,NonPublic'
function Field($name) { $f.GetType().GetField($name,$flags).GetValue($f) }
function InvokePrivate($name,[object[]]$values) { $f.GetType().GetMethod($name,$flags).Invoke($f,$values) }
function Pump([int]$ms) {
    $w=[Diagnostics.Stopwatch]::StartNew()
    while($w.ElapsedMilliseconds -lt $ms) { [Windows.Forms.Application]::DoEvents(); Start-Sleep -Milliseconds 10 }
}
try {
    $f.CreateControl()
    $p=Field 'axWindowsMediaPlayer1'
    $p.CreateControl()
    InvokePrivate 'OnShown' @([EventArgs]::Empty) | Out-Null
    $p.settings.volume=0
    InvokePrivate 'AddFiles' (,([string[]]@($MediaPath))) | Out-Null
    Pump 1500
    $p.Ctlcontrols.pause()
    Pump 200
    $fps=InvokePrivate 'ReadFrameRate' @($MediaPath)
    Write-Output "Duration=$($p.currentMedia.duration) FPS=$fps"
    foreach ($start in @(3.0,10.0,18.0)) {
      if ($start -ge $p.currentMedia.duration - 1) { continue }
      $p.Ctlcontrols.currentPosition=$start
      Pump 500
      $first=$true
      foreach($direction in @(1,1,1,-1,-1,-1,1,-1)) {
        $before=$p.Ctlcontrols.currentPosition
        InvokePrivate 'StepFrame' @([int]$direction) | Out-Null
        Pump 350
        $after=$p.Ctlcontrols.currentPosition
        Pump 200
        $settled=$p.Ctlcontrols.currentPosition
        if ([Math]::Abs($after-$settled) -gt 0.0002) { throw 'Position reverted after stepping' }
        if (!$first -and [Math]::Abs(($after-$before)-($direction/$fps)) -gt 0.0002) { throw 'Step did not move one frame duration' }
        if ([int]$p.playState -ne 2) { throw 'Frame stepping did not remain paused' }
        $first=$false
        Write-Output "direction=$direction before=$before after=$after settled=$settled state=$($p.playState)"
      }
    }
    Write-Output 'PASS: forward/backward frame durations and stable paused positions across tested locations.'
    Write-Output ("status="+(Field 'status').Text)
} finally { $f.Dispose() }



