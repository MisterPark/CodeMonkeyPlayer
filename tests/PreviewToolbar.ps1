$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Windows.Forms
$bin=Join-Path (Get-Location) 'CodeMonkeyPlayer/bin/Release'
$a=[Reflection.Assembly]::LoadFrom((Join-Path $bin 'CodeMonkeyPlayer.exe'))
$f=[Activator]::CreateInstance($a.GetType('CodeMonkeyPlayer.Form1'))
$flags=[Reflection.BindingFlags]'Instance,NonPublic'
$panel=$f.GetType().GetField('transport',$flags).GetValue($f)
$seek=$f.GetType().GetField('seek',$flags).GetValue($f)
$seek.Value=3500
$time=$f.GetType().GetField('time',$flags).GetValue($f)
$time.Text='03:30 / 10:00'
$f.PerformLayout()
$panel.Parent=$null
$panel.Visible=$true
$panel.CreateControl()
$panel.PerformLayout()
$bitmap=[Drawing.Bitmap]::new($panel.Width,$panel.Height)
try {
    $panel.DrawToBitmap($bitmap,[Drawing.Rectangle]::new(0,0,$panel.Width,$panel.Height))
    $bitmap.Save((Join-Path (Get-Location) 'tests/toolbar-preview.png'))
} finally {
    $bitmap.Dispose()
    $panel.Dispose()
    $f.Dispose()
}

