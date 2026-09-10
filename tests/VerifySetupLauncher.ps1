param([string]$Configuration='Release')
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Windows.Forms
$root=Split-Path -Parent $PSScriptRoot
$exe=Join-Path $root "CodeMonkeyPlayer.Setup/bin/$Configuration/CodeMonkeyPlayer-Setup.exe"
$msi=Join-Path $root "CodeMonkeyPlayer.Setup/bin/$Configuration/CodeMonkeyPlayer-Setup.msi"
$a=[Reflection.Assembly]::LoadFrom($exe)
$type=$a.GetType('CodeMonkeyPlayerSetup.SetupForm')
$flags=[Reflection.BindingFlags]'Instance,NonPublic'
$static=[Reflection.BindingFlags]'Static,NonPublic'
$f=[Activator]::CreateInstance($type)
function Field($name) { $type.GetField($name,$flags).GetValue($f) }
function Assert($ok,$message) { if(!$ok) { throw $message } }
try {
    $text=$a.GetType('CodeMonkeyPlayerSetup.InstallerText')
    $keys=$text.GetField('Keys',$static).GetValue($null)
    (Field 'desktop').Checked=$false
    (Field 'folder').Text='C:\Test Folder\Player'
    $selector=Field 'language'
    Assert ($selector.Items.Count -eq 18) 'Missing setup languages'
    for($i=0;$i -lt 18;$i++) {
        $selector.SelectedIndex=$i
        $code=$selector.SelectedItem.Code
        foreach($key in $keys) {
            $value=$text.GetMethod('Get').Invoke($null,@($code,$key))
            Assert (![string]::IsNullOrWhiteSpace($value)) "Missing setup translation: $code/$key"
            if($code -ne 'ko') { Assert ($value -notmatch '[가-힣]') "Korean fallback in $code/$key" }
        }
        Assert ((Field 'heading').Text -eq $text.GetMethod('Get').Invoke($null,@($code,'Title'))) "Heading did not update: $code"
        Assert ((Field 'install').Text -eq $text.GetMethod('Get').Invoke($null,@($code,'Install'))) "Install button did not update: $code"
        Assert (!(Field 'desktop').Checked -and (Field 'folder').Text -eq 'C:\Test Folder\Player') 'Language change reset options'
    }
    $args=$type.GetMethod('BuildArguments',$static).Invoke($null,@('C:\Temp\Player.msi','C:\Temp\setup.log','C:\Test Folder\Player','ja',$false,$true))
    Assert ($args.Contains('/qn /norestart') -and $args.Contains('APPLANGUAGE=ja') -and $args.Contains('DESKTOPSHORTCUT=0') -and $args.Contains('REGISTERVIDEO=1')) 'Incorrect MSI options'
    Assert ($args.Contains('INSTALLFOLDER="C:\Test Folder\Player"')) 'Folder is not quoted'
    foreach($bad in @('relative','C:relative','C:\Bad"Folder','\\server\share','C:\')) {
        $rejected=$false
        try { $type.GetMethod('ValidateFolder',$static).Invoke($null,@($bad)) | Out-Null } catch { $rejected=$true }
        Assert $rejected "Invalid folder accepted: $bad"
    }
    $embedded=$a.GetManifestResourceStream('Player.msi')
    $file=[IO.File]::OpenRead($msi)
    $hash=[Security.Cryptography.SHA256]::Create()
    try {
        Assert ([Convert]::ToBase64String($hash.ComputeHash($embedded)) -eq [Convert]::ToBase64String($hash.ComputeHash($file))) 'Embedded MSI differs from verified build'
    } finally { $embedded.Dispose(); $file.Dispose(); $hash.Dispose() }
    # Render the standalone layout without running the installation.
    $selector.SelectedIndex=1
    $f.StartPosition='Manual'
    $f.Location=[Drawing.Point]::new(-30000,-30000)
    $f.ShowInTaskbar=$false
    $f.Show()
    [Windows.Forms.Application]::DoEvents()
    # Keep actions reachable across translations and installation result states.
    for($i=0;$i -lt 18;$i++) {
        $selector.SelectedIndex=$i
        foreach($state in @('Intro','Installing','Done','Failed','InvalidPath','Restart')) {
            $type.GetField('statusKey',$flags).SetValue($f,$state)
            $type.GetField('completed',$flags).SetValue($f,($state -in @('Done','Restart')))
            (Field 'progress').Visible=$state -eq 'Installing'
            (Field 'logLocation').Visible=$state -ne 'Intro'
            $type.GetMethod('ApplyLanguage',$flags).Invoke($f,@($selector.SelectedItem.Code)) | Out-Null
            [Windows.Forms.Application]::DoEvents()
            foreach($name in @('install','close')) {
                $button=Field $name
                $bounds=$f.RectangleToClient($button.RectangleToScreen($button.ClientRectangle))
                Assert ($f.ClientRectangle.Contains($bounds)) "Action clipped: $($selector.SelectedItem.Code)/$state/$name"
            }
        }
    }
    $type.GetField('statusKey',$flags).SetValue($f,'Intro')
    $type.GetField('completed',$flags).SetValue($f,$false)
    (Field 'logLocation').Visible=$false
    (Field 'progress').Visible=$false
    $selector.SelectedIndex=0
    [Windows.Forms.Application]::DoEvents()
    $bitmap=[Drawing.Bitmap]::new($f.Width,$f.Height)
    try {
        $f.DrawToBitmap($bitmap,[Drawing.Rectangle]::new(0,0,$f.Width,$f.Height))
        $bitmap.Save((Join-Path $PSScriptRoot 'setup-launcher-preview.png'))
    } finally { $bitmap.Dispose(); $f.Hide() }
    Write-Output 'PASS: 18 setup languages, immediate switching without lost options, safe MSI arguments, path validation, embedded MSI hash. Installation was not executed.'
} finally { $f.Dispose() }
