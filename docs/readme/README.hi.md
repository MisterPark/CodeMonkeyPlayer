# CodeMonkey Player

<!-- languages:start -->
[한국어](../../README.md) · [English](README.en.md) · [日本語](README.ja.md) · [简体中文](README.zh-Hans.md) · [繁體中文](README.zh-Hant.md) · [Español](README.es.md) · [Français](README.fr.md) · [Deutsch](README.de.md) · [Português (Brasil)](README.pt-BR.md) · [Русский](README.ru.md) · [العربية](README.ar.md) · **हिन्दी** · [Italiano](README.it.md) · [Bahasa Indonesia](README.id.md) · [Tiếng Việt](README.vi.md) · [ไทย](README.th.md) · [Türkçe](README.tr.md) · [Polski](README.pl.md)
<!-- languages:end -->

Windows x64 के लिए WinForms और .NET Framework 4.8 पर बना मीडिया प्लेयर, जिसमें libmpv इंजन शामिल है।

## डाउनलोड और इंस्टॉल

[नवीनतम रिलीज़](https://github.com/MisterPark/CodeMonkeyPlayer/releases/latest) से **CodeMonkeyPlayer-Setup.exe** डाउनलोड करें। इंस्टॉल करने से पहले भाषा, फ़ोल्डर, डेस्कटॉप शॉर्टकट और वीडियो के लिए “Open with” में पंजीकरण चुन सकते हैं। Windows x64 और .NET Framework 4.8 आवश्यक हैं; अलग WMP कोडेक की ज़रूरत नहीं है। MSI सीधे चलाने पर अंग्रेज़ी विज़ार्ड खुलता है। इंस्टॉल करने के लिए व्यवस्थापक अधिकार चाहिए।

## सुविधाएँ

- फ़ाइलों को वीडियो या प्लेलिस्ट पर छोड़ें। एक क्लिक से चलाएँ/विराम दें, दो क्लिक से पूर्ण स्क्रीन बदलें। पूर्ण स्क्रीन में नियंत्रण छिप जाते हैं और माउस नीचे ले जाने पर दिखते हैं।
- प्रगति पट्टी पर क्लिक करके स्थान बदलें, आवाज़ समायोजित या म्यूट करें, फ़्रेम आगे-पीछे करें और सूची दोहराएँ। पिछले फ़्रेम पर जाने में दोबारा डिकोडिंग के कारण अधिक समय लग सकता है।
- दूसरी फ़ाइल खोलने पर वह मौजूदा प्रोग्राम की सूची में बिना दोहराव जुड़ती है और तुरंत चलती है। रोकने या वीडियो समाप्त होने के बाद Play/Space चयनित आइटम को शुरू से चलाता है; विराम के दौरान वर्तमान वीडियो आगे चलता है।
- प्लेयर और इंस्टॉलर 18 भाषाएँ देते हैं। नीचे के मेनू में चुनी भाषा सहेजी जाती है। सिस्टम संवाद Windows की भाषा का उपयोग करते हैं।

MP4, AVI, WMV, MKV, MOV, MPEG, WebM और सामान्य ऑडियो प्रारूप जोड़े जा सकते हैं। डिकोडिंग शामिल libmpv/FFmpeg पर निर्भर है। Explorer के थंबनेल Windows के समर्थन पर निर्भर हैं और उनकी जगह आइकन रह सकता है। बाहर निकलने पर प्लेलिस्ट नहीं सहेजी जाती।

## सेटिंग

`%LocalAppData%\CodeMonkeyPlayer\CodeMonkeyPlayer.ini` में आवाज़ (0–100), म्यूट, दोहराव, भाषा, विंडो की जगह, आकार और अधिकतम आकार की स्थिति सहेजी जाती है। डिफ़ॉल्ट आवाज़ 70 है; म्यूट और दोहराव बंद हैं। पहले से सहेजी व्यक्तिगत भाषा को इंस्टॉल के समय चुनी भाषा से प्राथमिकता मिलती है। अपडेट और अनइंस्टॉल व्यक्तिगत सेटिंग रखते हैं। पूर्ण स्क्रीन अपने-आप बहाल नहीं होती।

## शॉर्टकट

- `Ctrl+O`: फ़ाइलें खोलें। `Space`: चलाएँ/विराम दें। `,` / `.`: पिछला/अगला फ़्रेम और विराम।
- `←` / `→`: 5 सेकंड पीछे/आगे। `↑` / `↓`: आवाज़ 5 बढ़ाएँ/घटाएँ। `M`: म्यूट।
- `PageUp` / `PageDown`: पिछली/अगली फ़ाइल। `F11`: पूर्ण स्क्रीन बदलें। `Esc`: पूर्ण स्क्रीन से बाहर। `Delete`: प्लेलिस्ट पर फ़ोकस होने पर चयन हटाएँ।

## बिल्ड और जाँच

Visual Studio के .NET डेस्कटॉप विकास उपकरण और .NET Framework 4.8 लक्ष्यीकरण पैक चाहिए। WiX 6 इंस्टॉलर के लिए .NET SDK 6 या नया संस्करण भी आवश्यक है। रिपॉज़िटरी के मूल फ़ोल्डर में Developer PowerShell से ये कमांड चलाएँ। निर्भरताएँ बहाल करने के लिए इंटरनेट चाहिए। इंस्टॉलर `CodeMonkeyPlayer.Setup/bin/Release/` में बनते हैं।

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File CodeMonkeyPlayer/Native/Restore-Mpv.ps1
MSBuild CodeMonkeyPlayer/CodeMonkeyPlayer.csproj /p:Configuration=Release
powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File tests/Smoke.ps1 -Configuration Release
powershell.exe -NoProfile -ExecutionPolicy Bypass -File CodeMonkeyPlayer.Setup/Build-Installer.ps1
powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File tests/VerifySetupLauncher.ps1 -Configuration Release
powershell.exe -NoProfile -ExecutionPolicy Bypass -File tests/VerifyInstaller.ps1
```

## तृतीय-पक्ष घटक

शामिल libmpv, GPL सक्षम करके बनाया गया है। स्रोत और पुनर्वितरण की शर्तों के लिए [तृतीय-पक्ष सूचनाएँ](../../CodeMonkeyPlayer/Native/THIRD-PARTY-NOTICES.txt) और [mpv लाइसेंस](../../CodeMonkeyPlayer/Native/COPYING-mpv.txt) देखें। EXE और MSI पर कोड हस्ताक्षर नहीं हैं।
