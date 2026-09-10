$ErrorActionPreference='Stop'
$dll=Join-Path $PSScriptRoot 'libmpv-2.dll'
if(Test-Path -LiteralPath $dll) {
    if((Get-FileHash $dll -Algorithm SHA256).Hash -ne '673E6397920AB64A9C5B3A618F7F16D38854EFE72B58665F1F84E4E873B763A4') { throw 'mpv DLL checksum mismatch' }
    return
}
$cache=Join-Path $PSScriptRoot '../obj/mpv-download'
New-Item -ItemType Directory -Force -Path $cache | Out-Null
$archive=Join-Path $cache 'mpv.7z'
if(!(Test-Path -LiteralPath $archive)) {
    Invoke-WebRequest 'https://github.com/shinchiro/mpv-winbuild-cmake/releases/download/20260903/mpv-dev-x86_64-20260903-git-69e63f425a.7z' -OutFile $archive
}
if((Get-FileHash $archive -Algorithm SHA256).Hash -ne 'fac135c68a35b7639e39d72c0c365104edbaebdea39a0dfdd8c36e8c8e80faef') { throw 'mpv archive checksum mismatch' }
$seven=Join-Path $cache '7zr.exe'
if(!(Test-Path -LiteralPath $seven)) { Invoke-WebRequest 'https://www.7-zip.org/a/7zr.exe' -OutFile $seven }
if((Get-FileHash $seven -Algorithm SHA256).Hash -ne 'AD4C82FADCBDF93C03B4FC440F300509C7D60C5C2F4D183E35D9D70D6957037D') { throw '7-Zip checksum mismatch; review the new release before updating the pin' }
& $seven x $archive ('-o'+(Join-Path $cache 'extracted')) -y
if($LASTEXITCODE -ne 0) { throw 'mpv extraction failed' }
Copy-Item -LiteralPath (Join-Path $cache 'extracted/libmpv-2.dll') -Destination $dll
