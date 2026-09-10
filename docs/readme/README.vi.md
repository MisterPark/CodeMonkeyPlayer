# CodeMonkey Player

<!-- languages:start -->
[한국어](../../README.md) · [English](README.en.md) · [日本語](README.ja.md) · [简体中文](README.zh-Hans.md) · [繁體中文](README.zh-Hant.md) · [Español](README.es.md) · [Français](README.fr.md) · [Deutsch](README.de.md) · [Português (Brasil)](README.pt-BR.md) · [Русский](README.ru.md) · [العربية](README.ar.md) · [हिन्दी](README.hi.md) · [Italiano](README.it.md) · [Bahasa Indonesia](README.id.md) · **Tiếng Việt** · [ไทย](README.th.md) · [Türkçe](README.tr.md) · [Polski](README.pl.md)
<!-- languages:end -->

Trình phát đa phương tiện cho Windows x64, xây dựng bằng WinForms và .NET Framework 4.8, tích hợp bộ máy libmpv.

## Tải xuống và cài đặt

Tải **CodeMonkeyPlayer-Setup.exe** từ [bản phát hành mới nhất](https://github.com/MisterPark/CodeMonkeyPlayer/releases/latest). Chọn ngôn ngữ, thư mục, lối tắt trên màn hình nền và đăng ký trong “Mở bằng” cho video. Cần Windows x64 và .NET Framework 4.8; không cần codec WMP bổ sung. Chạy trực tiếp MSI sẽ mở trình hướng dẫn tiếng Anh. Cài đặt cần quyền quản trị.

## Tính năng

- Kéo thả tệp vào video hoặc danh sách phát. Nhấp để phát/tạm dừng, nhấp đúp để chuyển toàn màn hình. Các nút điều khiển ẩn sẽ hiện lại khi đưa con trỏ xuống cạnh dưới.
- Nhấp thanh tiến trình để chuyển vị trí, chỉnh âm lượng, tắt tiếng, chuyển từng khung hình và lặp danh sách. Lùi một khung hình có thể chậm hơn do cần giải mã lại.
- Mở tệp khác sẽ thêm vào phiên đang chạy, không tạo mục trùng và phát ngay. Sau khi dừng hoặc phát xong, nút Phát/Space phát mục được chọn từ đầu; khi tạm dừng thì tiếp tục video hiện tại.
- Trình phát và trình cài đặt hỗ trợ 18 ngôn ngữ. Lựa chọn trong menu dưới cùng được lưu. Hộp thoại hệ thống dùng ngôn ngữ Windows.

Chấp nhận MP4, AVI, WMV, MKV, MOV, MPEG, WebM và các định dạng âm thanh phổ biến. Khả năng giải mã phụ thuộc vào libmpv/FFmpeg tích hợp. Ảnh thu nhỏ của Explorer phụ thuộc vào hỗ trợ của Windows và có thể vẫn chỉ là biểu tượng. Danh sách phát không được lưu khi thoát.

## Cài đặt

`%LocalAppData%\CodeMonkeyPlayer\CodeMonkeyPlayer.ini` lưu âm lượng (0–100), tắt tiếng, lặp, ngôn ngữ, vị trí, kích thước và trạng thái phóng to cửa sổ. Mặc định: âm lượng 70, tắt tiếng và lặp đều tắt. Ngôn ngữ cá nhân đã lưu được ưu tiên hơn ngôn ngữ chọn lúc cài đặt. Cập nhật và gỡ cài đặt giữ lại thiết lập cá nhân. Không tự động khôi phục toàn màn hình.

## Phím tắt

- `Ctrl+O`: mở tệp. `Space`: phát/tạm dừng. `,` / `.`: khung hình trước/sau rồi tạm dừng.
- `←` / `→`: lùi/tiến 5 giây. `↑` / `↓`: tăng/giảm âm lượng 5 đơn vị. `M`: tắt tiếng.
- `PageUp` / `PageDown`: tệp trước/sau. `F11`: chuyển toàn màn hình. `Esc`: thoát toàn màn hình. `Delete`: xóa mục chọn khi danh sách phát có tiêu điểm.

## Biên dịch và kiểm tra

Cần công cụ phát triển ứng dụng desktop .NET của Visual Studio và gói nhắm mục tiêu .NET Framework 4.8. Trình cài đặt WiX 6 còn cần .NET SDK 6 trở lên. Chạy các lệnh dưới đây tại thư mục gốc kho mã bằng Developer PowerShell. Khôi phục các gói phụ thuộc cần Internet. Tệp cài đặt được tạo trong `CodeMonkeyPlayer.Setup/bin/Release/`.

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File CodeMonkeyPlayer/Native/Restore-Mpv.ps1
MSBuild CodeMonkeyPlayer/CodeMonkeyPlayer.csproj /p:Configuration=Release
powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File tests/Smoke.ps1 -Configuration Release
powershell.exe -NoProfile -ExecutionPolicy Bypass -File CodeMonkeyPlayer.Setup/Build-Installer.ps1
powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File tests/VerifySetupLauncher.ps1 -Configuration Release
powershell.exe -NoProfile -ExecutionPolicy Bypass -File tests/VerifyInstaller.ps1
```

## Thành phần bên thứ ba

libmpv tích hợp được biên dịch với GPL được bật. Xem nguồn và điều kiện phân phối lại trong [thông báo bên thứ ba](../../CodeMonkeyPlayer/Native/THIRD-PARTY-NOTICES.txt) và [giấy phép mpv](../../CodeMonkeyPlayer/Native/COPYING-mpv.txt). EXE và MSI chưa được ký mã.
