$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Windows.Forms
$root=Split-Path -Parent $PSScriptRoot
$bin=Join-Path $root 'CodeMonkeyPlayer/bin/Release-updated'
$a=[Reflection.Assembly]::LoadFrom((Join-Path $bin 'CodeMonkeyPlayer.exe'))
$f=[Activator]::CreateInstance($a.GetType('CodeMonkeyPlayer.Form1'))
$exeIcon=[Drawing.Icon]::ExtractAssociatedIcon((Join-Path $bin 'CodeMonkeyPlayer.exe'))
if (!$f.Icon -or !$exeIcon) { throw 'Missing application icon' }
$icoPath=Join-Path $root 'CodeMonkeyPlayer/Assets/CodeMonkeyPlayer.ico'
$bytes=[IO.File]::ReadAllBytes($icoPath)
if ([BitConverter]::ToUInt16($bytes,4) -ne 9) { throw 'Missing ICO sizes' }
$preview=[Drawing.Bitmap]::new(430,280)
$g=[Drawing.Graphics]::FromImage($preview)
$g.Clear([Drawing.Color]::FromArgb(38,39,44))
$x=8
try {
    foreach ($size in @(16,32,48,256)) {
        $icon=[Drawing.Icon]::new($icoPath,$size,$size)
        $bitmap=$icon.ToBitmap()
        $g.DrawImage($bitmap,$x,12,$size,$size)
        $bitmap.Dispose()
        $icon.Dispose()
        $x += $size + 14
    }
    $preview.Save((Join-Path $PSScriptRoot 'app-icon-preview.png'))
    Write-Output ('PASS: Form icon loaded; executable icon extracted; 9 ICO sizes verified.')
} finally {
    $g.Dispose()
    $preview.Dispose()
    $exeIcon.Dispose()
    $f.Dispose()
}
