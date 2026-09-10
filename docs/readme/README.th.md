# CodeMonkey Player

<!-- languages:start -->
[한국어](../../README.md) · [English](README.en.md) · [日本語](README.ja.md) · [简体中文](README.zh-Hans.md) · [繁體中文](README.zh-Hant.md) · [Español](README.es.md) · [Français](README.fr.md) · [Deutsch](README.de.md) · [Português (Brasil)](README.pt-BR.md) · [Русский](README.ru.md) · [العربية](README.ar.md) · [हिन्दी](README.hi.md) · [Italiano](README.it.md) · [Bahasa Indonesia](README.id.md) · [Tiếng Việt](README.vi.md) · **ไทย** · [Türkçe](README.tr.md) · [Polski](README.pl.md)
<!-- languages:end -->

โปรแกรมเล่นสื่อสำหรับ Windows x64 พัฒนาด้วย WinForms และ .NET Framework 4.8 พร้อมเอนจิน libmpv ในตัว

## ดาวน์โหลดและติดตั้ง

ดาวน์โหลด **CodeMonkeyPlayer-Setup.exe** จาก[รุ่นล่าสุด](https://github.com/MisterPark/CodeMonkeyPlayer/releases/latest) เลือกภาษา โฟลเดอร์ ทางลัดบนเดสก์ท็อป และการลงทะเบียนใน “เปิดด้วย” สำหรับวิดีโอได้ก่อนติดตั้ง ต้องใช้ Windows x64 และ .NET Framework 4.8 โดยไม่ต้องติดตั้งตัวแปลงสัญญาณ WMP เพิ่ม หากเรียกใช้ MSI โดยตรงจะเปิดตัวช่วยภาษาอังกฤษ การติดตั้งต้องใช้สิทธิ์ผู้ดูแลระบบ

## คุณสมบัติ

- ลากไฟล์ลงบนวิดีโอหรือรายการเล่น คลิกเพื่อเล่น/หยุดชั่วคราว ดับเบิลคลิกเพื่อสลับเต็มหน้าจอ ตัวควบคุมที่ซ่อนจะปรากฏเมื่อเลื่อนตัวชี้ไปขอบล่าง
- คลิกแถบความคืบหน้าเพื่อเปลี่ยนตำแหน่ง ปรับเสียง ปิดเสียง เลื่อนทีละเฟรม และเล่นรายการซ้ำ การย้อนหนึ่งเฟรมอาจช้ากว่าเพราะต้องถอดรหัสใหม่
- เมื่อเปิดไฟล์อื่น จะเพิ่มลงในโปรแกรมที่กำลังทำงานโดยไม่ซ้ำและเล่นทันที หลังหยุดหรือเล่นจบ ปุ่มเล่น/Space จะเล่นรายการที่เลือกตั้งแต่ต้น หากหยุดชั่วคราวจะเล่นวิดีโอปัจจุบันต่อ
- โปรแกรมและตัวติดตั้งรองรับ 18 ภาษา และบันทึกภาษาที่เลือกจากเมนูด้านล่าง กล่องโต้ตอบของระบบใช้ภาษาของ Windows

เพิ่มไฟล์ MP4, AVI, WMV, MKV, MOV, MPEG, WebM และรูปแบบเสียงทั่วไปได้ การถอดรหัสขึ้นอยู่กับ libmpv/FFmpeg ในตัว ภาพขนาดย่อใน Explorer ขึ้นอยู่กับการรองรับของ Windows จึงอาจยังแสดงเป็นไอคอน รายการเล่นจะไม่ถูกบันทึกเมื่อออกจากโปรแกรม

## การตั้งค่า

`%LocalAppData%\CodeMonkeyPlayer\CodeMonkeyPlayer.ini` เก็บระดับเสียง (0–100) การปิดเสียง การเล่นซ้ำ ภาษา ตำแหน่ง ขนาด และสถานะขยายหน้าต่าง ค่าเริ่มต้นคือระดับเสียง 70 โดยปิดการปิดเสียงและการเล่นซ้ำ ภาษาส่วนตัวที่บันทึกไว้มีลำดับความสำคัญเหนือภาษาที่เลือกตอนติดตั้ง การอัปเดตและถอนการติดตั้งจะเก็บการตั้งค่าส่วนตัวไว้ และไม่คืนค่าเต็มหน้าจอโดยอัตโนมัติ

## แป้นพิมพ์ลัด

- `Ctrl+O`: เปิดไฟล์ `Space`: เล่น/หยุดชั่วคราว `,` / `.`: เฟรมก่อนหน้า/ถัดไปแล้วหยุดชั่วคราว
- `←` / `→`: ย้อน/เดินหน้า 5 วินาที `↑` / `↓`: เพิ่ม/ลดเสียงครั้งละ 5 `M`: ปิดเสียง
- `PageUp` / `PageDown`: ไฟล์ก่อนหน้า/ถัดไป `F11`: สลับเต็มหน้าจอ `Esc`: ออกจากเต็มหน้าจอ `Delete`: ลบรายการที่เลือกเมื่อโฟกัสอยู่ที่รายการเล่น

## การบิลด์และตรวจสอบ

ต้องใช้เครื่องมือพัฒนาเดสก์ท็อป .NET ของ Visual Studio และ targeting pack ของ .NET Framework 4.8 ตัวติดตั้ง WiX 6 ต้องใช้ .NET SDK 6 ขึ้นไปด้วย เรียกใช้คำสั่งต่อไปนี้จากโฟลเดอร์รากของที่เก็บโค้ดใน Developer PowerShell การกู้คืนแพ็กเกจต้องใช้อินเทอร์เน็ต ไฟล์ติดตั้งจะอยู่ใน `CodeMonkeyPlayer.Setup/bin/Release/`

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File CodeMonkeyPlayer/Native/Restore-Mpv.ps1
MSBuild CodeMonkeyPlayer/CodeMonkeyPlayer.csproj /p:Configuration=Release
powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File tests/Smoke.ps1 -Configuration Release
powershell.exe -NoProfile -ExecutionPolicy Bypass -File CodeMonkeyPlayer.Setup/Build-Installer.ps1
powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File tests/VerifySetupLauncher.ps1 -Configuration Release
powershell.exe -NoProfile -ExecutionPolicy Bypass -File tests/VerifyInstaller.ps1
```

## ส่วนประกอบของบุคคลที่สาม

libmpv ในตัวเป็นรุ่นที่เปิดใช้ GPL ดูแหล่งที่มาและเงื่อนไขการแจกจ่ายต่อใน[ประกาศของบุคคลที่สาม](../../CodeMonkeyPlayer/Native/THIRD-PARTY-NOTICES.txt) และ[สัญญาอนุญาต mpv](../../CodeMonkeyPlayer/Native/COPYING-mpv.txt) ไฟล์ EXE และ MSI ไม่มีลายเซ็นรับรองโค้ด
