param([string]$MsiPath=(Join-Path $PSScriptRoot '../CodeMonkeyPlayer.Setup/bin/Release/CodeMonkeyPlayer-Setup.msi'))
$ErrorActionPreference='Stop'
$installer=New-Object -ComObject WindowsInstaller.Installer
$database=$installer.OpenDatabase([IO.Path]::GetFullPath($MsiPath),0)
function Rows([string]$query) {
    try { $view=$database.OpenView($query) } catch { throw "Invalid MSI query: $query ($_)" }
    $view.Execute() | Out-Null
    $results=@()
    while ($record=$view.Fetch()) {
        $results+=,$record.StringData(1)
        [Runtime.InteropServices.Marshal]::ReleaseComObject($record) | Out-Null
    }
    $view.Close() | Out-Null
    [Runtime.InteropServices.Marshal]::ReleaseComObject($view) | Out-Null
    return $results
}
try {
    $files=@(Rows 'SELECT `FileName` FROM `File`')
    foreach($expected in @('CodeMonkeyPlayer.exe','CodeMonkeyPlayer.exe.config','libmpv-2.dll','THIRD-PARTY-NOTICES.txt','COPYING-mpv.txt')) {
        if (!($files | Where-Object { ($_ -split '\|')[-1] -eq $expected })) { throw "Missing payload: $expected" }
    }
    if ($files.Count -ne 5) { throw 'Unexpected payload files, possibly personal settings.' }
    if (@(Rows 'SELECT `Shortcut` FROM `Shortcut`').Count -ne 2) { throw 'Missing shortcuts.' }
    if (@(Rows 'SELECT `Condition` FROM `LaunchCondition`').Count -lt 1) { throw 'Missing prerequisite checks.' }
    if (@(Rows 'SELECT `UpgradeCode` FROM `Upgrade`').Count -eq 0) { throw 'Missing upgrade rules.' }
    $languages=@(Rows 'SELECT `Value` FROM `ComboBox` WHERE `Property` = ''APPLANGUAGE''')
    if ($languages.Count -ne 18 -or $languages -notcontains 'ko' -or $languages -notcontains 'zh-Hant') { throw 'Installer languages are incomplete.' }
    foreach($option in @('DESKTOPSHORTCUT','REGISTERVIDEO')) {
        if (@(Rows ('SELECT `Value` FROM `CheckBox` WHERE `Property` = '''+$option+'''') ) -ne '1') { throw "Missing option: $option" }
    }
    if ((Rows 'SELECT `Condition` FROM `Component` WHERE `Component` = ''DesktopShortcut''') -ne 'DESKTOPSHORTCUT = "1"') { throw 'Desktop shortcut is not optional.' }
    if ((Rows 'SELECT `Condition` FROM `Component` WHERE `Component` = ''VideoAssociations''') -ne 'REGISTERVIDEO = "1"') { throw 'Video registration is not optional.' }
    if ((Rows 'SELECT `Argument` FROM `ControlEvent` WHERE `Dialog_` = ''WelcomeDlg'' AND `Control_` = ''Next'' AND `Ordering` = 3') -ne 'PlayerOptionsDlg') { throw 'Options dialog is not reachable.' }
    $registryKeys=@(Rows 'SELECT `Key` FROM `Registry`')
    foreach($extension in @('mp4','m4v','avi','wmv','mkv','mov','mpg','mpeg','webm')) {
        if ($registryKeys -notcontains "SOFTWARE\Classes\.$extension\OpenWithProgids") { throw "Missing Open with registration: $extension" }
        if ($registryKeys -contains "SOFTWARE\Classes\.$extension") { throw "Installer overwrites default association: $extension" }
    }
    if ($registryKeys -match 'UserChoice') { throw 'Installer writes protected user defaults.' }
    if ($registryKeys -notcontains 'SOFTWARE\Classes\CodeMonkeyPlayer.Video\shellex\{e357fccd-a995-4576-b01f-234630154e96}') { throw 'Missing video thumbnail provider for player ProgID.' }
    $commands=@(Rows 'SELECT `Value` FROM `Registry` WHERE `Key` = ''SOFTWARE\Classes\CodeMonkeyPlayer.Video\shell\open\command''')
    if ($commands.Count -ne 1 -or $commands[0] -ne '"[INSTALLFOLDER]CodeMonkeyPlayer.exe" "%1"') { throw 'Video command does not quote executable and filename.' }
    if ((Rows 'SELECT `Value` FROM `Registry` WHERE `Name` = ''Language''') -ne '[APPLANGUAGE]') { throw 'Initial language is not installed.' }
    Write-Output 'PASS: options dialog, 18 languages, conditional shortcut/registration, nine video extensions, quoted open command, default associations preserved.'
    Write-Output 'PASS: MSI database, five application files including mpv and license notices, two shortcuts, prerequisite checks, upgrade rules; no personal settings bundled.'
} finally {
    [Runtime.InteropServices.Marshal]::ReleaseComObject($database) | Out-Null
    [Runtime.InteropServices.Marshal]::ReleaseComObject($installer) | Out-Null
}
