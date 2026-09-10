# CodeMonkey Player

<!-- languages:start -->
[한국어](../../README.md) · [English](README.en.md) · [日本語](README.ja.md) · [简体中文](README.zh-Hans.md) · [繁體中文](README.zh-Hant.md) · [Español](README.es.md) · [Français](README.fr.md) · [Deutsch](README.de.md) · [Português (Brasil)](README.pt-BR.md) · [Русский](README.ru.md) · [العربية](README.ar.md) · [हिन्दी](README.hi.md) · [Italiano](README.it.md) · **Bahasa Indonesia** · [Tiếng Việt](README.vi.md) · [ไทย](README.th.md) · [Türkçe](README.tr.md) · [Polski](README.pl.md)
<!-- languages:end -->

Pemutar media untuk Windows x64, dibuat dengan WinForms dan .NET Framework 4.8, dengan mesin libmpv bawaan.

## Unduh dan instal

Unduh **CodeMonkeyPlayer-Setup.exe** dari [rilis terbaru](https://github.com/MisterPark/CodeMonkeyPlayer/releases/latest). Pilih bahasa, folder, pintasan desktop, dan pendaftaran “Buka dengan” untuk video. Memerlukan Windows x64 dan .NET Framework 4.8, tanpa codec WMP tambahan. Menjalankan MSI secara langsung membuka panduan berbahasa Inggris. Instalasi memerlukan hak administrator.

## Fitur

- Seret berkas ke video atau daftar putar. Klik untuk memutar/menjeda; klik dua kali untuk beralih ke layar penuh. Kontrol tersembunyi muncul kembali saat penunjuk berada di tepi bawah.
- Klik bilah kemajuan untuk berpindah posisi, atur volume, bisukan suara, pindah bingkai dan ulangi daftar. Mundur satu bingkai dapat lebih lambat karena perlu decoding ulang.
- Berkas lain ditambahkan tanpa duplikat ke proses yang sudah berjalan dan langsung diputar. Setelah berhenti atau selesai, Putar/Space memulai item terpilih dari awal; saat dijeda, video saat ini dilanjutkan.
- Pemutar dan penginstal mendukung 18 bahasa. Pilihan di menu bawah disimpan. Dialog sistem mengikuti bahasa Windows.

Menerima MP4, AVI, WMV, MKV, MOV, MPEG, WebM dan format audio umum. Decoding bergantung pada libmpv/FFmpeg bawaan. Thumbnail Explorer bergantung pada dukungan Windows dan mungkin tetap berupa ikon. Daftar putar tidak disimpan saat keluar.

## Pengaturan

`%LocalAppData%\CodeMonkeyPlayer\CodeMonkeyPlayer.ini` menyimpan volume (0–100), bisu, pengulangan, bahasa, posisi, ukuran dan status jendela dimaksimalkan. Nilai awal: volume 70, bisu dan pengulangan nonaktif. Bahasa pribadi yang tersimpan mengalahkan pilihan saat instalasi. Pembaruan dan penghapusan instalasi mempertahankan pengaturan pribadi. Layar penuh tidak dipulihkan otomatis.

## Pintasan

- `Ctrl+O`: buka berkas. `Space`: putar/jeda. `,` / `.`: bingkai sebelumnya/berikutnya, lalu jeda.
- `←` / `→`: mundur/maju 5 detik. `↑` / `↓`: naikkan/turunkan volume sebesar 5. `M`: bisukan suara.
- `PageUp` / `PageDown`: berkas sebelumnya/berikutnya. `F11`: alihkan layar penuh. `Esc`: keluar dari layar penuh. `Delete`: hapus pilihan saat fokus berada pada daftar putar.

## Build dan verifikasi

Instal alat pengembangan desktop .NET Visual Studio dan targeting pack .NET Framework 4.8. Penginstal WiX 6 juga memerlukan .NET SDK 6 atau lebih baru. Jalankan perintah berikut dari akar repositori melalui Developer PowerShell. Pemulihan dependensi memerlukan Internet. Penginstal dihasilkan di `CodeMonkeyPlayer.Setup/bin/Release/`.

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File CodeMonkeyPlayer/Native/Restore-Mpv.ps1
MSBuild CodeMonkeyPlayer/CodeMonkeyPlayer.csproj /p:Configuration=Release
powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File tests/Smoke.ps1 -Configuration Release
powershell.exe -NoProfile -ExecutionPolicy Bypass -File CodeMonkeyPlayer.Setup/Build-Installer.ps1
powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File tests/VerifySetupLauncher.ps1 -Configuration Release
powershell.exe -NoProfile -ExecutionPolicy Bypass -File tests/VerifyInstaller.ps1
```

## Komponen pihak ketiga

libmpv bawaan dibuat dengan GPL diaktifkan. Sumber dan ketentuan distribusi ulang tersedia dalam [pemberitahuan pihak ketiga](../../CodeMonkeyPlayer/Native/THIRD-PARTY-NOTICES.txt) dan [lisensi mpv](../../CodeMonkeyPlayer/Native/COPYING-mpv.txt). EXE dan MSI tidak ditandatangani secara digital.
