param([string]$Configuration='Release')
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Windows.Forms
$root=Split-Path -Parent $PSScriptRoot
$runtime=Join-Path $PSScriptRoot ('settings-test-'+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $runtime | Out-Null
$bin=Join-Path $root "CodeMonkeyPlayer/bin/$Configuration"
foreach($name in @('CodeMonkeyPlayer.exe','Interop.WMPLib.dll','AxInterop.WMPLib.dll')) {
    Copy-Item -LiteralPath (Join-Path $bin $name) -Destination $runtime
}
[Reflection.Assembly]::LoadFrom((Join-Path $runtime 'Interop.WMPLib.dll')) | Out-Null
[Reflection.Assembly]::LoadFrom((Join-Path $runtime 'AxInterop.WMPLib.dll')) | Out-Null
$a=[Reflection.Assembly]::LoadFrom((Join-Path $runtime 'CodeMonkeyPlayer.exe'))
$flags=[Reflection.BindingFlags]'Instance,NonPublic'
$ini=Join-Path $runtime 'CodeMonkeyPlayer.ini'
function NewPlayer {
    $f=[Activator]::CreateInstance($a.GetType('CodeMonkeyPlayer.Form1'))
    $f.CreateControl()
    $p=$f.GetType().GetField('axWindowsMediaPlayer1',$flags).GetValue($f)
    $p.CreateControl()
    $f.GetType().GetMethod('RestorePreferences',$flags).Invoke($f,@()) | Out-Null
    return $f
}
function Field($f,$name) { $f.GetType().GetField($name,$flags).GetValue($f) }
function Assert($condition,$message) { if (!$condition) { throw $message } }
$f=$null
try {
    $f=NewPlayer
    Assert (Test-Path -LiteralPath $ini) 'First run did not create settings beside executable'
    Assert ((Field $f 'volume').Value -eq 70) 'Default volume is incorrect'
    (Field $f 'volume').Value=37
    (Field $f 'mute').Checked=$true
    (Field $f 'repeat').Checked=$true
    $watch=[Diagnostics.Stopwatch]::StartNew()
    while($watch.ElapsedMilliseconds -lt 750) { [Windows.Forms.Application]::DoEvents(); Start-Sleep -Milliseconds 20 }
    Assert ((Get-Content -Raw $ini).Contains('Volume=37')) 'Debounced save failed'
    $f.Dispose()
    $f=NewPlayer
    Assert ((Field $f 'volume').Value -eq 37) 'Volume was not restored'
    Assert ((Field $f 'mute').Checked) 'Mute was not restored'
    Assert ((Field $f 'repeat').Checked) 'Repeat was not restored'
    Assert ((Field $f 'axWindowsMediaPlayer1').settings.volume -eq 37) 'Engine volume differs from saved value'
    (Field $f 'volume').Value=23
    $f.GetType().GetMethod('SavePreferences',$flags).Invoke($f,@()) | Out-Null
    Assert ((Get-Content -Raw $ini).Contains('Volume=23')) 'Final flush failed'
    $f.Dispose()
    [IO.File]::WriteAllLines($ini, @('Volume=999999','Muted=invalid','Repeat=true'))
    $f=NewPlayer
    Assert ((Field $f 'volume').Value -eq 100) 'Out-of-range volume was not clamped'
    Assert (!(Field $f 'mute').Checked) 'Invalid mute did not use default'
    Assert ((Field $f 'repeat').Checked) 'Valid setting was lost'
    $f.Dispose()
    [IO.File]::WriteAllText($ini,'damaged file')
    $f=NewPlayer
    Assert ((Field $f 'volume').Value -eq 70) 'Malformed file did not use defaults'
    Write-Output 'PASS: creation, delayed save, final flush, restart restoration, engine volume, invalid values, corrupted file.'
} finally {
    if($f) { $f.Dispose() }
}
