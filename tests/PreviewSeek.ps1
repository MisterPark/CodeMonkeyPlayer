$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms
$bin = Join-Path (Get-Location) 'CodeMonkeyPlayer/bin/Release-updated'
[Reflection.Assembly]::LoadFrom((Join-Path $bin 'Interop.WMPLib.dll')) | Out-Null
[Reflection.Assembly]::LoadFrom((Join-Path $bin 'AxInterop.WMPLib.dll')) | Out-Null
$a = [Reflection.Assembly]::LoadFrom((Join-Path $bin 'CodeMonkeyPlayer.exe'))
$f = [Activator]::CreateInstance($a.GetType('CodeMonkeyPlayer.Form1'))
$flags = [Reflection.BindingFlags]'Instance,NonPublic'
$seek = $f.GetType().GetField('seek',$flags).GetValue($f)
$seek.Dock = 'None'
$seek.Size = [Drawing.Size]::new(640,46)
$seek.Value = 3500
$seek.Duration = 600
$result = [Drawing.Bitmap]::new(640,110)
$graphics = [Drawing.Graphics]::FromImage($result)
$graphics.Clear($seek.BackColor)
try {
    for ($i=0; $i -lt 2; $i++) {
        $seek.GetType().GetField('hovering',$flags).SetValue($seek, ($i -eq 1))
        $seek.GetType().GetField('pointerX',$flags).SetValue($seek, [int]400)
        $bitmap = [Drawing.Bitmap]::new(640,46)
        $seek.DrawToBitmap($bitmap, [Drawing.Rectangle]::new(0,0,640,46))
        $graphics.DrawImage($bitmap,0,($i*55))
        $bitmap.Dispose()
    }
    $result.Save((Join-Path (Get-Location) 'tests/seek-preview.png'))
} finally {
    $graphics.Dispose()
    $result.Dispose()
    $f.Dispose()
}
