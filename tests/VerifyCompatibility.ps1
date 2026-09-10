param([Parameter(Mandatory=$true)][string]$MediaPath,[string]$Configuration='Debug')
& (Join-Path $PSScriptRoot 'VerifyMpv.ps1') -MediaPath $MediaPath -Configuration $Configuration