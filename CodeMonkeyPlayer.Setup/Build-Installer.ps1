param([ValidateSet('Debug','Release')][string]$Configuration='Release')
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$vswhere=Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio/Installer/vswhere.exe'
if (!(Test-Path -LiteralPath $vswhere)) { throw 'Visual Studio Build Tools with .NET desktop development is required.' }
$msbuild=& $vswhere -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild/**/Bin/MSBuild.exe' | Select-Object -First 1
if (!$msbuild) { throw 'Visual Studio MSBuild was not found.' }
$payload=Join-Path $PSScriptRoot "obj/payload/$Configuration"
& $msbuild (Join-Path $root 'CodeMonkeyPlayer/CodeMonkeyPlayer.csproj') /t:Rebuild "/p:Configuration=$Configuration" "/p:OutputPath=$payload/" /nologo /verbosity:minimal
if ($LASTEXITCODE -ne 0) { throw 'Player build failed.' }
dotnet build (Join-Path $PSScriptRoot 'CodeMonkeyPlayer.Setup.wixproj') -c $Configuration /p:BuildProjectReferences=false "/p:PayloadDir=$payload"
if ($LASTEXITCODE -ne 0) { throw 'Installer build failed.' }
Get-ChildItem (Join-Path $PSScriptRoot "bin/$Configuration") -Filter '*.msi' -Recurse | Select-Object FullName,Length
