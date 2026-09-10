# CodeMonkey Player

<!-- languages:start -->
[한국어](../../README.md) · [English](README.en.md) · [日本語](README.ja.md) · [简体中文](README.zh-Hans.md) · [繁體中文](README.zh-Hant.md) · [Español](README.es.md) · [Français](README.fr.md) · [Deutsch](README.de.md) · [Português (Brasil)](README.pt-BR.md) · [Русский](README.ru.md) · **العربية** · [हिन्दी](README.hi.md) · [Italiano](README.it.md) · [Bahasa Indonesia](README.id.md) · [Tiếng Việt](README.vi.md) · [ไทย](README.th.md) · [Türkçe](README.tr.md) · [Polski](README.pl.md)
<!-- languages:end -->

<div dir="rtl">

مشغّل وسائط لنظام Windows x64، مبني باستخدام WinForms و.NET Framework 4.8 مع محرّك libmpv مدمج.

## التنزيل والتثبيت

نزّل **CodeMonkeyPlayer-Setup.exe** من [أحدث إصدار](https://github.com/MisterPark/CodeMonkeyPlayer/releases/latest). يمكنك اختيار اللغة ومجلد التثبيت واختصار سطح المكتب والتسجيل ضمن «فتح باستخدام» لملفات الفيديو. يتطلب Windows x64 و.NET Framework 4.8، ولا يحتاج إلى برامج ترميز إضافية لـ WMP. تشغيل MSI مباشرة يفتح معالجًا باللغة الإنجليزية. يتطلب التثبيت صلاحيات المسؤول.

## الميزات

- اسحب الملفات إلى منطقة الفيديو أو قائمة التشغيل. انقر للتشغيل أو الإيقاف المؤقت، وانقر مرتين لتبديل ملء الشاشة. تختفي عناصر التحكم في ملء الشاشة وتظهر عند تحريك المؤشر إلى الأسفل.
- انقر على شريط التقدم للانتقال، واضبط الصوت أو اكتمه، وانتقل بين الإطارات وكرّر القائمة. قد يستغرق الرجوع إلى الإطار السابق وقتًا أطول لأنه قد يتطلب إعادة فك الترميز.
- فتح ملف آخر يضيفه دون تكرار إلى نسخة البرنامج الحالية ويشغّله فورًا. بعد الإيقاف أو انتهاء التشغيل، يبدأ زر التشغيل أو `Space` العنصر المحدد من البداية؛ أما أثناء الإيقاف المؤقت فيستأنف الفيديو الحالي.
- يدعم المشغّل والمثبّت 18 لغة. يُحفظ اختيار اللغة من القائمة السفلية. تتبع مربعات حوار النظام لغة Windows.

يمكن إضافة MP4 وAVI وWMV وMKV وMOV وMPEG وWebM وصيغ الصوت الشائعة. يعتمد فك الترميز على libmpv/FFmpeg المدمج. تعتمد الصور المصغرة في مستكشف الملفات على دعم Windows وقد تبقى أيقونة فقط. لا تُحفظ قائمة التشغيل عند الخروج.

## الإعدادات

يُخزّن الملف `%LocalAppData%\CodeMonkeyPlayer\CodeMonkeyPlayer.ini` مستوى الصوت (0–100) والكتم والتكرار واللغة وموضع النافذة وحجمها وحالة تكبيرها. القيم الافتراضية: الصوت 70، والكتم والتكرار معطّلان. تتقدم اللغة الشخصية المحفوظة على اللغة المختارة أثناء التثبيت. تبقى الإعدادات الشخصية بعد التحديث وإلغاء التثبيت. لا يُستعاد ملء الشاشة تلقائيًا.

## اختصارات لوحة المفاتيح

- `Ctrl+O`: فتح الملفات. `Space`: تشغيل/إيقاف مؤقت. `,` و`.`: الإطار السابق والتالي، ثم الإيقاف المؤقت.
- `←` و`→`: الرجوع والتقدم 5 ثوانٍ. `↑` و`↓`: رفع وخفض الصوت بمقدار 5. `M`: كتم الصوت.
- `PageUp` و`PageDown`: الملف السابق والتالي. `F11`: تبديل ملء الشاشة. `Esc`: الخروج منه. `Delete`: حذف العناصر المحددة عندما يكون التركيز على قائمة التشغيل.

## البناء والتحقق

ثبّت أدوات تطوير تطبيقات سطح المكتب .NET في Visual Studio وحزمة استهداف .NET Framework 4.8. يتطلب مثبّت WiX 6 أيضًا .NET SDK 6 أو أحدث. نفّذ الأوامر التالية من جذر المستودع في Developer PowerShell. تتطلب استعادة الاعتماديات اتصالًا بالإنترنت. تُنشأ ملفات التثبيت في `CodeMonkeyPlayer.Setup/bin/Release/`.

<div dir="ltr">

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File CodeMonkeyPlayer/Native/Restore-Mpv.ps1
MSBuild CodeMonkeyPlayer/CodeMonkeyPlayer.csproj /p:Configuration=Release
powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File tests/Smoke.ps1 -Configuration Release
powershell.exe -NoProfile -ExecutionPolicy Bypass -File CodeMonkeyPlayer.Setup/Build-Installer.ps1
powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File tests/VerifySetupLauncher.ps1 -Configuration Release
powershell.exe -NoProfile -ExecutionPolicy Bypass -File tests/VerifyInstaller.ps1
```

</div>

## مكونات خارجية

بُني libmpv المدمج مع تفعيل GPL. راجع مصادره ومتطلبات إعادة التوزيع في [إشعارات المكونات الخارجية](../../CodeMonkeyPlayer/Native/THIRD-PARTY-NOTICES.txt) و[رخصة mpv](../../CodeMonkeyPlayer/Native/COPYING-mpv.txt). ملفا EXE وMSI غير موقّعين رقميًا.

</div>
