param([string]$Configuration='Release')
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Windows.Forms
$root=Split-Path -Parent $PSScriptRoot
$runtime=Join-Path $PSScriptRoot ('settings-test-'+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $runtime | Out-Null
$bin=Join-Path $root "CodeMonkeyPlayer/bin/$Configuration"
foreach($name in @('CodeMonkeyPlayer.exe')) {
    Copy-Item -LiteralPath (Join-Path $bin $name) -Destination $runtime
}
$a=[Reflection.Assembly]::LoadFrom((Join-Path $runtime 'CodeMonkeyPlayer.exe'))
$flags=[Reflection.BindingFlags]'Instance,NonPublic'
$ini=Join-Path $runtime 'UserSettings/CodeMonkeyPlayer.ini'
$legacy=Join-Path $runtime 'CodeMonkeyPlayer.ini'
function NewPlayer {
    $f=$a.GetType('CodeMonkeyPlayer.Form1').GetConstructor($flags,$null,[type[]]@([string],[string]),$null).Invoke(@($ini.PSObject.BaseObject,$legacy.PSObject.BaseObject))
    $f.CreateControl()
    $p=$f.GetType().GetField('video',$flags).GetValue($f)
    $p.CreateControl()
    $f.GetType().GetMethod('RestorePreferences',$flags).Invoke($f,@()) | Out-Null
    return $f
}
function Field($f,$name) { $f.GetType().GetField($name,$flags).GetValue($f) }
function Assert($condition,$message) { if (!$condition) { throw $message } }
$f=$null
try {
    $f=NewPlayer
    Assert (Test-Path -LiteralPath $ini) 'First run did not create the user settings directory'
    Assert ((Field $f 'volume').Value -eq 70) 'Default volume is incorrect'
    Assert ((Field $f 'languageSelector').SelectedIndex -eq 0) 'Default language is not Korean'
    (Field $f 'languageSelector').SelectedIndex=1
    Assert ((Field $f 'playlistHeading').Text -eq 'Playlist') 'Playlist language did not change immediately'
    Assert ((Field $f 'play').AccessibleName -eq 'Play') 'Play accessibility label was not translated'
    Assert ((Field $f 'toolTips').GetToolTip((Field $f 'play')) -eq 'Play (Space)') 'Play tooltip was not translated'
    Assert ((Field $f 'status').Text -eq 'Open files or drop them onto the video or playlist.') 'Status was not translated'
    $f.GetType().GetMethod('SetStatus',$flags).Invoke($f,@('재생 중: {0}',[object[]]@('sample.mp4'))) | Out-Null
    (Field $f 'languageSelector').SelectedIndex=0
    Assert ((Field $f 'status').Text -eq '재생 중: sample.mp4') 'Switching language lost status arguments'
    (Field $f 'languageSelector').SelectedIndex=1
    Assert ((Field $f 'status').Text -eq 'Playing: sample.mp4') ("Switching back lost status arguments: " + (Field $f 'status').Text)
    (Field $f 'volume').Value=37
    (Field $f 'mute').Checked=$true
    (Field $f 'repeat').Checked=$true
    $watch=[Diagnostics.Stopwatch]::StartNew()
    while($watch.ElapsedMilliseconds -lt 750) { [Windows.Forms.Application]::DoEvents(); Start-Sleep -Milliseconds 20 }
    Assert ((Get-Content -Raw $ini).Contains('Volume=37')) 'Debounced save failed'
    Assert ((Get-Content -Raw $ini).Contains('Language=en')) 'Language was not saved'
    $f.Dispose()
    $f=NewPlayer
    Assert ((Field $f 'volume').Value -eq 37) 'Volume was not restored'
    Assert ((Field $f 'mute').Checked) 'Mute was not restored'
    Assert ((Field $f 'repeat').Checked) 'Repeat was not restored'
    Assert ((Field $f 'languageSelector').SelectedIndex -eq 1) 'Language was not restored'
    Assert ((Field $f 'playlistHeading').Text -eq 'Playlist') 'Restored language was not applied'
    Assert ((Field $f 'mute').AccessibleName -eq 'Unmute') 'Restored toggle language was not applied'
    Assert ((Field $f 'video').Volume -eq 37) 'Engine volume differs from saved value'
    (Field $f 'volume').Value=23
    $f.GetType().GetMethod('SavePreferences',$flags).Invoke($f,@()) | Out-Null
    Assert ((Get-Content -Raw $ini).Contains('Volume=23')) 'Final flush failed'
    $f.Dispose()
    [IO.File]::WriteAllLines($ini, @('Volume=999999','Muted=invalid','Repeat=true','Language=invalid'))
    $f=NewPlayer
    Assert ((Field $f 'volume').Value -eq 100) 'Out-of-range volume was not clamped'
    Assert (!(Field $f 'mute').Checked) 'Invalid mute did not use default'
    Assert ((Field $f 'repeat').Checked) 'Valid setting was lost'
    Assert ((Field $f 'languageSelector').SelectedIndex -eq 0) 'Invalid language did not fall back to Korean'
    $f.Dispose()
    [IO.File]::WriteAllText($ini,'damaged file')
    $f=NewPlayer
    Assert ((Field $f 'volume').Value -eq 70) 'Malformed file did not use defaults'
    Assert ((Field $f 'languageSelector').SelectedIndex -eq 0) 'Missing language did not fall back to Korean'
    $f.Dispose()
    Remove-Item -LiteralPath $ini
    [IO.File]::WriteAllLines($legacy,@('Volume=42','Muted=true','Repeat=false'))
    $f=NewPlayer
    Assert ((Field $f 'volume').Value -eq 42) 'Legacy volume migration failed'
    Assert (Test-Path -LiteralPath $legacy) 'Migration deleted the legacy file'
    $f.Dispose()
    [IO.File]::WriteAllLines($legacy,@('Volume=99'))
    $f=NewPlayer
    Assert ((Field $f 'volume').Value -eq 42) 'Legacy file overwrote newer user settings'
    $defaultPath=$a.GetType('CodeMonkeyPlayer.PlayerPreferences').GetProperty('DefaultPath').GetValue($null)
    $localData=[Environment]::GetFolderPath([Environment+SpecialFolder]::LocalApplicationData,[Environment+SpecialFolderOption]::DoNotVerify)
    if (!$localData) { $localData=$env:LOCALAPPDATA }
    $expected=Join-Path $localData 'CodeMonkeyPlayer/CodeMonkeyPlayer.ini'
    Assert ($defaultPath -eq $expected) 'Default path is not LocalApplicationData'
    $area=[Windows.Forms.Screen]::PrimaryScreen.WorkingArea
    $f.StartPosition='Manual'
    $target=[Drawing.Rectangle]::new($area.Left+20,$area.Top+20,[Math]::Min(1000,$area.Width-40),[Math]::Min(600,$area.Height-40))
    $f.Bounds=$target
    $target=$f.Bounds
    $f.GetType().GetMethod('SavePreferences',$flags).Invoke($f,@()) | Out-Null
    $f.Dispose()
    $f=NewPlayer
    Assert ($f.Bounds -eq $target) 'Normal window placement did not survive restart'
    $f.WindowState='Maximized'
    $f.GetType().GetMethod('RememberWindowPlacement',$flags).Invoke($f,@()) | Out-Null
    $f.WindowState='Minimized'
    $f.GetType().GetMethod('RememberWindowPlacement',$flags).Invoke($f,@()) | Out-Null
    $f.GetType().GetMethod('SavePreferences',$flags).Invoke($f,@()) | Out-Null
    $f.Dispose()
    $f=NewPlayer
    Assert ($f.WindowState -eq 'Maximized') 'Minimized window lost prior maximized state'
    Assert ((Field $f 'normalWindowBounds') -eq $target) 'Maximization overwrote normal bounds'
    $f.GetType().GetMethod('ToggleFullscreen',$flags).Invoke($f,@()) | Out-Null
    $f.GetType().GetMethod('SavePreferences',$flags).Invoke($f,@()) | Out-Null
    $f.Dispose()
    $f=NewPlayer
    Assert ($f.WindowState -eq 'Maximized') 'Fullscreen lost previous maximized state'
    Assert (!(Field $f 'fullscreen')) 'Restart unexpectedly entered fullscreen'
    Assert ((Field $f 'normalWindowBounds') -eq $target) 'Fullscreen overwrote restored bounds'
    $f.Dispose()
    [IO.File]::WriteAllLines($ini,@('WindowX=900000','WindowY=-900000','WindowWidth=30000','WindowHeight=20000'))
    $f=NewPlayer
    Assert ([Windows.Forms.Screen]::FromRectangle($f.Bounds).WorkingArea.Contains($f.Bounds)) 'Off-screen window was not clamped'
    $uiType=$a.GetType('CodeMonkeyPlayer.UiText')
    $load=$a.GetType('CodeMonkeyPlayer.PlayerPreferences').GetMethod('Load')
    $missing=Join-Path $runtime 'not-created.ini'
    $first=$load.Invoke($null,@($missing.PSObject.BaseObject,'ja'))
    Assert ($first.Language -eq 'ja') 'First run did not use installer language'
    [IO.File]::WriteAllLines($ini,@('Volume=31','Language=fr'))
    $existing=$load.Invoke($null,@($ini.PSObject.BaseObject,'ja'))
    Assert ($existing.Language -eq 'fr' -and $existing.Volume -eq 31) 'Installer language overwrote personal settings'
    [IO.File]::WriteAllLines($ini,@('Volume=31'))
    $migrated=$load.Invoke($null,@($ini.PSObject.BaseObject,'de'))
    Assert ($migrated.Language -eq 'de' -and $migrated.Volume -eq 31) 'Legacy settings did not inherit installer language'
    Write-Output 'PASS: initial installer language, legacy language fallback and existing personal settings precedence.'
    $keys=$uiType.GetField('English',[Reflection.BindingFlags]'Static,NonPublic').GetValue($null).Keys
    $count=(Field $f 'languageSelector').Items.Count
    Assert ($count -eq 18) 'Expected 18 supported languages'
    for($index=0; $index -lt $count; $index++) {
        (Field $f 'languageSelector').SelectedIndex=$index
        $ui=Field $f 'uiText'
        $code=$ui.Language
        foreach($key in $keys) {
            $translation=$uiType.GetMethod('Get').Invoke($ui,@($key))
            Assert (![string]::IsNullOrWhiteSpace($translation)) "Empty translation: $code / $key"
            if($code -ne 'ko') { Assert ($translation -notmatch '[가-힣]') "Untranslated text: $code / $key" }
            Assert (([regex]::Matches($translation,'\{0\}')).Count -eq ([regex]::Matches($key,'\{0\}')).Count) "Placeholder mismatch: $code / $key"
        }
        $f.GetType().GetMethod('SavePreferences',$flags).Invoke($f,@()) | Out-Null
        $f.Dispose()
        $f=NewPlayer
        Assert ((Field $f 'languageSelector').SelectedIndex -eq $index) "Language selection not restored: $code"
        Assert ((Field $f 'uiText').Language -eq $code) "Language code not restored: $code"
        Assert ((Field $f 'playlistHeading').RightToLeft.ToString() -eq $(if($code -eq 'ar'){'Yes'}else{'No'})) "Text direction not restored: $code"
    }
    Write-Output 'PASS: all 18 languages, complete UI/help translations, placeholders, save/restart and Arabic text direction.'
    Write-Output 'PASS: normal bounds, maximize/minimize, fullscreen preservation, off-screen recovery.'
    Write-Output 'PASS: immediate language switching, accessible labels, tooltips, status arguments, language persistence and fallback.'
    Write-Output 'PASS: LocalAppData path, legacy migration, existing-settings precedence.'
    Write-Output 'PASS: creation, delayed save, final flush, restart restoration, engine volume, invalid values, corrupted file.'
} finally {
    if($f) { $f.Dispose() }
}



