param([string]$MsiPath=(Join-Path $PSScriptRoot '../CodeMonkeyPlayer.Setup/bin/Release/CodeMonkeyPlayer-Setup.msi'))
$ErrorActionPreference='Stop'
$installer=New-Object -ComObject WindowsInstaller.Installer
$database=$installer.OpenDatabase([IO.Path]::GetFullPath($MsiPath),0)
function Rows([string]$query) {
    $view=$database.OpenView($query)
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
    foreach($expected in @('CodeMonkeyPlayer.exe','CodeMonkeyPlayer.exe.config','Interop.WMPLib.dll','AxInterop.WMPLib.dll')) {
        if (!($files | Where-Object { ($_ -split '\|')[-1] -eq $expected })) { throw "Missing payload: $expected" }
    }
    if ($files.Count -ne 4) { throw 'Unexpected payload files, possibly personal settings.' }
    if (@(Rows 'SELECT `Shortcut` FROM `Shortcut`').Count -ne 2) { throw 'Missing shortcuts.' }
    if (@(Rows 'SELECT `Condition` FROM `LaunchCondition`').Count -lt 2) { throw 'Missing prerequisite checks.' }
    if (@(Rows 'SELECT `UpgradeCode` FROM `Upgrade`').Count -eq 0) { throw 'Missing upgrade rules.' }
    Write-Output 'PASS: MSI database, four application files, two shortcuts, prerequisite checks, upgrade rules; no personal settings bundled.'
} finally {
    [Runtime.InteropServices.Marshal]::ReleaseComObject($database) | Out-Null
    [Runtime.InteropServices.Marshal]::ReleaseComObject($installer) | Out-Null
}
