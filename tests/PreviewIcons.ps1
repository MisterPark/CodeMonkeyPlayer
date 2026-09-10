Add-Type -AssemblyName System.Windows.Forms
$bin = Join-Path (Get-Location) 'CodeMonkeyPlayer/bin/Release'
$a = [Reflection.Assembly]::LoadFrom((Join-Path $bin 'CodeMonkeyPlayer.exe'))
$f = [Activator]::CreateInstance($a.GetType('CodeMonkeyPlayer.Form1'))
$method = $f.GetType().GetMethod('PaintIcon', [Reflection.BindingFlags]'Instance,NonPublic')
$bitmap = New-Object Drawing.Bitmap(700,60)
$graphics = [Drawing.Graphics]::FromImage($bitmap)
$graphics.Clear([Drawing.Color]::FromArgb(36,39,48))
$i=0
foreach ($name in @('파일 열기','이전','재생','일시 정지','정지','다음','음소거','음소거 해제','목록 반복 켜기','전체 화면','도움말','추가','삭제','비우기')) {
    $b = [Windows.Forms.Button]::new()
    $b.Size = New-Object Drawing.Size(44,44)
    $b.AccessibleName=$name
    $state=$graphics.Save()
    $graphics.TranslateTransform(($i*50),8)
    $paintArgs=[Windows.Forms.PaintEventArgs]::new($graphics, $b.ClientRectangle)
    $method.Invoke($f, @($b.PSObject.BaseObject,$paintArgs.PSObject.BaseObject)) | Out-Null
    $graphics.Restore($state)
    $b.Dispose()
    $i++
}
$bitmap.Save((Join-Path (Get-Location) 'tests/icons-preview.png'))
$graphics.Dispose()
$bitmap.Dispose()
$f.Dispose()
