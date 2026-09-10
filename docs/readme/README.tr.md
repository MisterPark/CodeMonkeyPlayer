# CodeMonkey Player

<!-- languages:start -->
[한국어](../../README.md) · [English](README.en.md) · [日本語](README.ja.md) · [简体中文](README.zh-Hans.md) · [繁體中文](README.zh-Hant.md) · [Español](README.es.md) · [Français](README.fr.md) · [Deutsch](README.de.md) · [Português (Brasil)](README.pt-BR.md) · [Русский](README.ru.md) · [العربية](README.ar.md) · [हिन्दी](README.hi.md) · [Italiano](README.it.md) · [Bahasa Indonesia](README.id.md) · [Tiếng Việt](README.vi.md) · [ไทย](README.th.md) · **Türkçe** · [Polski](README.pl.md)
<!-- languages:end -->

Windows x64 için WinForms ve .NET Framework 4.8 ile geliştirilmiş, yerleşik libmpv motoruna sahip medya oynatıcı.

## İndirme ve kurulum

[En son sürümden](https://github.com/MisterPark/CodeMonkeyPlayer/releases/latest) **CodeMonkeyPlayer-Setup.exe** dosyasını indirin. Kurulumdan önce dil, klasör, masaüstü kısayolu ve videolar için “Birlikte aç” kaydı seçilebilir. Windows x64 ve .NET Framework 4.8 gerekir; ek WMP kodekleri gerekmez. MSI doğrudan çalıştırıldığında İngilizce sihirbaz açılır. Kurulum yönetici yetkisi ister.

## Özellikler

- Dosyaları videoya veya oynatma listesine sürükleyin. Tek tıklama oynatır/duraklatır; çift tıklama tam ekranı değiştirir. Gizlenen kontroller, işaretçi alt kenara getirildiğinde görünür.
- İlerleme çubuğuna tıklayarak konum değiştirin; sesi ayarlayın, sessize alın, kare kare ilerleyin veya geri gidin ve listeyi tekrarlayın. Önceki kareye dönüş yeniden çözümleme gerektirebildiğinden daha yavaş olabilir.
- Başka bir dosya açıldığında çalışan örneğin listesine yinelenmeden eklenir ve hemen oynatılır. Durdurma veya bitiş sonrasında Oynat/Space seçili öğeyi baştan başlatır; duraklatıldığında mevcut video sürdürülür.
- Oynatıcı ve kurulum programı 18 dili destekler. Alt menüde seçilen dil kaydedilir. Sistem iletişim kutuları Windows dilini kullanır.

MP4, AVI, WMV, MKV, MOV, MPEG, WebM ve yaygın ses biçimleri eklenebilir. Çözümleme yerleşik libmpv/FFmpeg desteğine bağlıdır. Dosya Gezgini küçük resimleri Windows desteğine bağlıdır ve simge olarak kalabilir. Çıkışta oynatma listesi kaydedilmez.

## Ayarlar

`%LocalAppData%\CodeMonkeyPlayer\CodeMonkeyPlayer.ini`; ses düzeyi (0–100), sessiz, tekrar, dil, pencere konumu, boyutu ve büyütülme durumunu saklar. Varsayılan ses 70’tir; sessiz ve tekrar kapalıdır. Kayıtlı kişisel dil, kurulumda seçilen dilden önceliklidir. Güncelleme ve kaldırma kişisel ayarları korur. Tam ekran otomatik geri yüklenmez.

## Kısayollar

- `Ctrl+O`: dosya aç. `Space`: oynat/duraklat. `,` / `.`: önceki/sonraki kareye geçip duraklat.
- `←` / `→`: 5 saniye geri/ileri. `↑` / `↓`: sesi 5 artır/azalt. `M`: sessize al.
- `PageUp` / `PageDown`: önceki/sonraki dosya. `F11`: tam ekranı değiştir. `Esc`: tam ekrandan çık. `Delete`: odak listedeyken seçili öğeleri kaldır.

## Derleme ve doğrulama

Visual Studio .NET masaüstü geliştirme araçları ve .NET Framework 4.8 hedefleme paketi gerekir. WiX 6 kurulum programı ayrıca .NET SDK 6 veya yenisini gerektirir. Komutları depo kökünde Developer PowerShell ile çalıştırın. Bağımlılıkları geri yüklemek için İnternet gerekir. Kurulum dosyaları `CodeMonkeyPlayer.Setup/bin/Release/` altında oluşturulur.

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File CodeMonkeyPlayer/Native/Restore-Mpv.ps1
MSBuild CodeMonkeyPlayer/CodeMonkeyPlayer.csproj /p:Configuration=Release
powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File tests/Smoke.ps1 -Configuration Release
powershell.exe -NoProfile -ExecutionPolicy Bypass -File CodeMonkeyPlayer.Setup/Build-Installer.ps1
powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File tests/VerifySetupLauncher.ps1 -Configuration Release
powershell.exe -NoProfile -ExecutionPolicy Bypass -File tests/VerifyInstaller.ps1
```

## Üçüncü taraf bileşenleri

Yerleşik libmpv, GPL etkinleştirilerek derlenmiştir. Kaynaklar ve yeniden dağıtım koşulları için [üçüncü taraf bildirimlerine](../../CodeMonkeyPlayer/Native/THIRD-PARTY-NOTICES.txt) ve [mpv lisansına](../../CodeMonkeyPlayer/Native/COPYING-mpv.txt) bakın. EXE ve MSI kod imzasına sahip değildir.
