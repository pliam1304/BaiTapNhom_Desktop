# EduPath — bản Avalonia (bố cục & animation mới)

Đây là bản viết lại giao diện của `QLyDangKyHocPhan` (WinForms) sang **Avalonia UI**
theo mô hình MVVM, với bố cục và hiệu ứng hoàn toàn khác bản gốc — phần **nghiệp vụ
(Models/Services/InMemoryStore) được giữ nguyên logic**, chỉ đổi namespace.

## Điểm khác biệt so với bản WinForms gốc

| | WinForms gốc | Avalonia (bản mới) |
|---|---|---|
| Điều hướng | Sidebar dọc bên trái | Thanh pill ngang phía trên, có nền vàng trượt theo mục chọn |
| Chuyển trang | Ẩn/hiện Form đột ngột | `TransitioningContentControl` — CrossFade khi đăng nhập, PageSlide khi đổi tab |
| Dashboard | Bảng tĩnh | Thẻ KPI xuất hiện so le (fade + trượt lên, delay tăng dần từng thẻ) |
| Đăng nhập | Form đơn giản, canh giữa | Bố cục 2 cột: panel thương hiệu navy bên trái + thẻ đăng nhập bên phải, chọn vai trò dạng segmented control |
| Modal tạo lớp | `FrmCreateSection` (cửa sổ riêng) | Form mở rộng ngay trong trang (inline), không cần cửa sổ phụ |
| Style | Code-behind set màu thủ công (`UiTheme.cs`) | `App.axaml` style hệ thống dùng Class (`.card`, `.primary`, `.badge-ok`...) |

## Cấu trúc

```
Models/        — copy nguyên logic từ bản WinForms (đổi namespace)
Data/          — InMemoryStore (singleton, seed dữ liệu mẫu)
Services/      — AuthService, EnrollmentService, SectionService, CourseService,
                 ScheduleConflictService, RegistrationPeriodService, SessionContext
ViewModels/    — MVVM: MainWindowViewModel điều phối Login ⇄ StudentShell ⇄ AdminShell
Views/         — .axaml + code-behind tương ứng từng ViewModel
App.axaml      — bảng màu (navy/gold), style Card/Button/Badge/NavPill dùng chung
```

## Chạy thử

Cần .NET 8 SDK. Tại thư mục dự án:

```bash
dotnet restore
dotnet run
```

Tài khoản demo (giữ nguyên từ bản gốc):
- Sinh viên: `SV20260018` / `123456`
- Quản trị: `AD0001` / `admin123`

## Đã hoàn thành

- Đăng nhập (2 vai trò, kiểm tra qua `AuthService`)
- **Sinh viên**: Tổng quan (KPI tín chỉ/đợt đăng ký), Đăng ký học phần (tìm kiếm +
  đăng ký có kiểm tra tiên quyết/trùng lịch/sĩ số/vượt tín chỉ qua `EnrollmentService`),
  Đã đăng ký (hủy đăng ký), Thời khóa biểu (theo tuần), Lịch sử đăng ký
- **Quản trị**: Tổng quan hệ thống (KPI), Quản lý lớp học phần (tạo lớp mới có kiểm
  tra trùng phòng/GV qua `ScheduleConflictService`, đóng đăng ký), Danh mục học phần

## Có thể mở rộng thêm (chưa làm trong bản này)

Các màn hình quản trị còn lại của bản gốc (Tài khoản & phân quyền, Quản lý đợt đăng ký,
Quản lý phòng/giảng viên...) chưa được chuyển sang — có thể thêm theo đúng pattern
`ViewModel + View` đã thiết lập (thêm `NavItem` mới trong `AdminShellViewModel` và
một cặp View/ViewModel tương ứng).

> Lưu ý: mã nguồn được viết mà **không có môi trường build .NET/Avalonia để kiểm tra
> trực tiếp** (sandbox không có mạng để tải NuGet). Cấu trúc, namespace và binding đã
> được rà soát kỹ, nhưng khi `dotnet build` lần đầu có thể cần sửa vài lỗi nhỏ (ví dụ
> kiểu dữ liệu binding của `NumericUpDown`). Nếu gặp lỗi, gửi lại thông báo lỗi để tôi
> sửa tiếp.
