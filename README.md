# EDUPATH — WinForms (.NET 8, dữ liệu in-memory)

Bản dựng lại từ mockup HTML `edupath-winforms-mockups.html` thành ứng dụng WinForms C# thật,
tách nghiệp vụ khỏi giao diện theo 3 tầng.

## Chạy thử
```
cd EduPath.WinForms
dotnet run
```
(Cần Windows + `dotnet` SDK 8, workload `Windows Desktop`. Không chạy được trên Linux/macOS vì WinForms chỉ hỗ trợ Windows.)

Tài khoản demo:
- Sinh viên: `SV20260018` / `123456`
- Quản trị viên: `AD0001` / `admin123`

## Kiến trúc — vì sao tách như vậy

```
Models/     Đối tượng dữ liệu thuần (POCO) — không chứa logic nghiệp vụ, không chứa UI.
Data/       InMemoryStore — "cơ sở dữ liệu" giả lập bằng List<T>, Singleton dùng chung toàn app.
Services/   TOÀN BỘ logic nghiệp vụ: kiểm tra tiên quyết, trùng lịch, sĩ số, tín chỉ tối đa,
            xung đột phòng/giảng viên, quy tắc đợt đăng ký... Form KHÔNG được tự viết if/else
            nghiệp vụ — chỉ được gọi Service và hiển thị kết quả.
Common/     Hạ tầng UI dùng chung: MainShellForm (khung sidebar+header+content), SideMenu,
            UiTheme (màu/font đúng theo mockup), GridHelper (bảng/card đồng bộ style).
Forms/      Các màn hình thật, chia theo luồng Student/ và Admin/. Mỗi màn = 1 UserControl
            (nội dung `.e-content`) được MainShellForm nạp động khi điều hướng — giống hệt
            cách mockup JS gốc dùng object `content{}` + `setScreen()`, nhưng bằng C# thật.
```

Khi cần đổi từ in-memory sang SQL Server/SQLite: chỉ sửa `Data/InMemoryStore.cs` thành
`DbContext` của EF Core — toàn bộ `Services/` và `Forms/` không cần sửa vì chỉ thao tác qua
các phương thức Service, không đụng trực tiếp vào List.

## Đã dựng đầy đủ logic (không phải mockup tĩnh)

| Màn hình (theo mockup) | File | Nghiệp vụ thật |
|---|---|---|
| 01 Đăng nhập | `Forms/FrmLogin.cs` + `Services/AuthService.cs` | Kiểm tra tài khoản/mật khẩu/vai trò/khóa tài khoản |
| 02 Dashboard SV | `Forms/Student/StudentDashboardView.cs` | Số liệu tính thật từ `EnrollmentService` |
| 03+04+05 Đăng ký học phần | `OpenSectionsView.cs` + `FrmConfirmRegistration.cs` | `EnrollmentService.CanRegister/Register`: **tiên quyết, trùng lịch, sĩ số, tín chỉ tối đa, đợt đăng ký đang mở** |
| 06 Đã đăng ký | `EnrolledSectionsView.cs` | `EnrollmentService.Cancel` (chặn hủy khi đợt đã đóng) |
| 07 Lịch học | `TimetableView.cs` | Lưới dựng động từ Section đang đăng ký (không hard-code) |
| 08 Lịch sử | `HistoryView.cs` | Gồm cả lượt đã hủy |
| 09 Dashboard Admin | `AdminDashboardView.cs` | KPI "Cần xử lý" tính thật (lớp gần đầy, xung đột lịch, SV vượt TC) |
| 11 Lớp học phần | `SectionsAdminView.cs` + `FrmCreateSection.cs` | `SectionService.Create` gọi `ScheduleConflictService` — **chặn trùng phòng/GV theo đúng quy tắc ghi trong mockup màn 16** |

## Chưa dựng chi tiết (để lại `PlaceholderView` theo đúng 1 pattern)

Màn 10, 12, 13, 14, 15, 17 (Học phần, Sinh viên, Giảng viên, Đợt đăng ký, Phòng học, Tài khoản)
dùng đúng khuôn của `SectionsAdminView.cs`:

1. Tạo `XxxService.cs` trong `Services/` (nếu chưa có) chứa `GetAll()` + `Create()/Update()` với
   validate nghiệp vụ (xem `CourseService.cs`, `RegistrationPeriodService.cs` làm mẫu).
2. Tạo `XxxView.cs` trong `Forms/Admin/`: `GridHelper.MakeGrid()` + nạp `DataSource` từ Service.
3. Nút "+ Thêm..." mở 1 Form modal nhỏ (mẫu: `FrmCreateSection.cs`) gọi `Service.Create()`,
   hiển thị `error` nếu Service từ chối.
4. Thay dòng `RegisterView("xxx", ..., () => new PlaceholderView(...))` trong
   `AdminShellBuilder.cs` bằng `() => new XxxView()`.

Toàn bộ 7 file `Services/*.cs` hiện có đã chứa sẵn hầu hết logic cần thiết
(`RegistrationPeriodService` đã có `Open/Close`, chỉ thiếu Form quản trị).
