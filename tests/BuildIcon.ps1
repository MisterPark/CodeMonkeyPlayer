$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
Add-Type @"
using System;
using System.Runtime.InteropServices;
public static class IconHandles {
    [DllImport("user32.dll")] public static extern bool DestroyIcon(IntPtr icon);
}
"@
$root = Split-Path -Parent $PSScriptRoot
$asset = Join-Path $root 'CodeMonkeyPlayer/Assets'
$source = [Drawing.Bitmap]::FromFile((Join-Path $asset 'CodeMonkeyPlayer.png'))
$frames = @()
try {
    foreach ($size in @(16,20,24,32,40,48,64,128,256)) {
        $bitmap = [Drawing.Bitmap]::new($size,$size,[Drawing.Imaging.PixelFormat]::Format32bppArgb)
        $graphics = [Drawing.Graphics]::FromImage($bitmap)
        $graphics.InterpolationMode = [Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
        $graphics.PixelOffsetMode = [Drawing.Drawing2D.PixelOffsetMode]::HighQuality
        $graphics.DrawImage($source,[Drawing.Rectangle]::new(0,0,$size,$size))
        $graphics.Dispose()
        $stream = [IO.MemoryStream]::new()
        $dib = [IO.BinaryWriter]::new($stream)
        $maskStride = [int]([Math]::Ceiling($size / 32.0) * 4)
        $dib.Write([uint32]40)
        $dib.Write([int32]$size)
        $dib.Write([int32]($size * 2))
        $dib.Write([uint16]1)
        $dib.Write([uint16]32)
        $dib.Write([uint32]0)
        $dib.Write([uint32]($size * $size * 4))
        1..4 | ForEach-Object { $dib.Write([uint32]0) }
        for ($y=$size-1; $y -ge 0; $y--) {
            for ($x=0; $x -lt $size; $x++) {
                $pixel=$bitmap.GetPixel($x,$y)
                $dib.Write([byte]$pixel.B)
                $dib.Write([byte]$pixel.G)
                $dib.Write([byte]$pixel.R)
                $dib.Write([byte]$pixel.A)
            }
        }
        for ($y=$size-1; $y -ge 0; $y--) {
            $mask=[byte[]]::new($maskStride)
            for ($x=0; $x -lt $size; $x++) {
                if ($bitmap.GetPixel($x,$y).A -eq 0) {
                    $index=[int][Math]::Floor($x/8)
                    $mask[$index]=$mask[$index] -bor (128 -shr ($x % 8))
                }
            }
            $dib.Write($mask)
        }
        $frames += [pscustomobject]@{ Size=$size; Payload=$stream.ToArray() }
        $dib.Dispose()
        $bitmap.Dispose()
    }
    $output = [IO.File]::Create((Join-Path $asset 'CodeMonkeyPlayer.ico'))
    $writer = [IO.BinaryWriter]::new($output)
    try {
        $writer.Write([uint16]0)
        $writer.Write([uint16]1)
        $writer.Write([uint16]$frames.Count)
        $offset = 6 + 16 * $frames.Count
        foreach ($frame in $frames) {
            $encodedSize = if ($frame.Size -eq 256) { 0 } else { $frame.Size }
            $writer.Write([byte]$encodedSize)
            $writer.Write([byte]$encodedSize)
            $writer.Write([byte]0)
            $writer.Write([byte]0)
            $writer.Write([uint16]1)
            $writer.Write([uint16]32)
            $writer.Write([uint32]$frame.Payload.Length)
            $writer.Write([uint32]$offset)
            $offset += $frame.Payload.Length
        }
        foreach ($frame in $frames) { $writer.Write([byte[]]$frame.Payload) }
    } finally { $writer.Dispose() }
} finally { $source.Dispose() }
Write-Output 'Created 9-size ICO: 16,20,24,32,40,48,64,128,256.'

