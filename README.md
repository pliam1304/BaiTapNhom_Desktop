# EduPath — Hệ thống Đăng ký Học phần (bản Avalonia UI)

EduPath là ứng dụng desktop mô phỏng hệ thống đăng ký học phần của trường đại học,
viết bằng **C# / .NET 8** với giao diện **Avalonia UI** theo mô hình **MVVM**.
Đây là bản viết lại giao diện của project WinForms gốc `QLyDangKyHocPhan` — phần
**nghiệp vụ (Models/Services/dữ liệu mẫu) được giữ nguyên logic**, chỉ thay đổi
tầng giao diện và cách tổ chức điều hướng.

Ứng dụng chạy hoàn toàn **offline**, dữ liệu lưu trong bộ nhớ (RAM) qua một
singleton `InMemoryStore`, không cần cài đặt cơ sở dữ liệu.

---

## 1. Mục lục

- [Yêu cầu hệ thống](#2-yêu-cầu-hệ-thống)
- [Cài đặt & chạy thử](#3-cài-đặt--chạy-thử)
- [Tài khoản demo](#4-tài-khoản-demo)
- [Chức năng theo vai trò](#5-chức-năng-theo-vai-trò)
- [Kiến trúc & cấu trúc thư mục](#6-kiến-trúc--cấu-trúc-thư-mục)
- [Luồng nghiệp vụ chính](#7-luồng-nghiệp-vụ-chính)
- [Dữ liệu mẫu có sẵn](#8-dữ-liệu-mẫu-có-sẵn)
- [Build bản phát hành (release)](#9-build-bản-phát-hành-release)
- [Các lỗi thường gặp khi chạy lần đầu](#10-các-lỗi-thường-gặp-khi-chạy-lần-đầu)
- [Hướng phát triển tiếp](#11-hướng-phát-triển-tiếp)

---

## 2. Yêu cầu hệ thống

| Thành phần | Phiên bản |
|---|---|
| .NET SDK | **8.0** trở lên ([tải tại đây](https://dotnet.microsoft.com/download/dotnet/8.0)) |
| Hệ điều hành | Windows, macOS hoặc Linux (Avalonia đa nền tảng) |
| IDE (không bắt buộc) | Visual Studio 2022, JetBrains Rider, hoặc VS Code + C# Dev Kit |

Kiểm tra đã cài .NET chưa bằng lệnh:

```bash
dotnet --version
```

Kết quả cần hiện `8.x.x` (hoặc cao hơn).

---

## 3. Cài đặt & chạy thử

### Cách 1 — Chạy từ mã nguồn (khuyến nghị khi phát triển)

```bash
# 1. Clone hoặc giải nén project, sau đó vào thư mục gốc (chứa file .csproj)
cd EduPath.Avalonia

# 2. Khôi phục các gói NuGet (Avalonia, Avalonia.Desktop, Avalonia.Themes.Fluent, ...)
dotnet restore

# 3. Build project
dotnet build

# 4. Chạy ứng dụng
dotnet run
```

Nếu muốn build & chạy nhanh trong một lệnh:

```bash
dotnet run --project EduPath.Avalonia.csproj
```

### Cách 2 — Chạy bản build sẵn (Windows)

Repo có kèm file **`EduPath-Desktop-Windows-x64.zip`** — bản đã build sẵn cho Windows x64.
Chỉ cần:

1. Giải nén file zip này.
2. Chạy file `.exe` bên trong (không cần cài .NET SDK, vì đây là bản self-contained/publish).

> Nếu Windows Defender/SmartScreen cảnh báo "Unknown publisher", chọn **More info → Run anyway**
> (đây là ứng dụng bài tập, chưa có chữ ký số).

---

## 4. Tài khoản demo

Ứng dụng có sẵn 4 tài khoản sinh viên và 1 tài khoản quản trị, tất cả được seed sẵn
trong `Data/InMemoryStore.cs`:

| Vai trò | Tên đăng nhập | Mật khẩu | Ghi chú |
|---|---|---|---|
| Sinh viên | `SV2411869` | `123456` | Huỳnh Phúc Lâm — CNTT01, khóa 2026, đã hoàn thành CS101 |
| Sinh viên | `SV2411870` | `123456` | Nguyễn Minh Anh — CNTT01, khóa 2026, đã hoàn thành CS101/MA104/EN101 |
| Sinh viên | `SV2411871` | `123456` | Trần Quốc Bảo — CNTT02, khóa 2025, đã hoàn thành 8 học phần |
| Sinh viên | `SV2411872` | `123456` | Lê Hoàng Nam — CNTT03, khóa 2025, đã hoàn thành 5 học phần |
| Quản trị | `AD0001` | `admin123` | Toàn quyền quản trị hệ thống |

> **Lưu ý**: Ở màn hình đăng nhập cần chọn đúng **vai trò** (Sinh viên / Quản trị)
> tương ứng với tài khoản, nếu chọn sai vai trò hệ thống sẽ báo lỗi dù đúng mật khẩu
> (kiểm tra trong `AuthService.Login`).

Mỗi sinh viên demo đều có sẵn 2 lớp học phần đã đăng ký từ trước (xem mục 8) để có
dữ liệu minh họa ngay khi đăng nhập vào Tổng quan / Thời khóa biểu / Lịch sử.

---

## 5. Chức năng theo vai trò

### 5.1. Sinh viên

| Trang | Mô tả |
|---|---|
| **Tổng quan** | Thẻ KPI: số tín chỉ đã đăng ký, số lớp đang học, trạng thái đợt đăng ký hiện hành |
| **Đăng ký học phần** | Tìm kiếm/lọc danh sách lớp học phần đang mở của học kỳ hiện hành; đăng ký có kiểm tra đầy đủ điều kiện (xem mục 7.2) |
| **Đã đăng ký** | Danh sách lớp đang học, có thể hủy đăng ký (nếu đợt đăng ký còn mở) |
| **Thời khóa biểu** | Lịch học theo tuần, hiển thị theo thứ/khung giờ |
| **Lịch sử** | Toàn bộ lượt đăng ký (kể cả đã hủy), sắp xếp mới nhất trước |

### 5.2. Quản trị viên

| Trang | Mô tả |
|---|---|
| **Tổng quan** | KPI hệ thống: tổng số lớp, tổng sinh viên, tình trạng đợt đăng ký... |
| **Lớp học phần** | Xem danh sách lớp; tạo lớp mới ngay trong trang (form inline, không cần cửa sổ phụ), có kiểm tra trùng phòng/trùng giảng viên qua `ScheduleConflictService`; đóng đăng ký một lớp |
| **Học phần** | Danh mục học phần: mã, tên, số tín chỉ, khoa, học phần tiên quyết; thêm học phần mới; vô hiệu hóa học phần (chỉ khi không còn lớp nào đang mở) |

> Các màn quản trị khác của bản gốc (Tài khoản & phân quyền, Quản lý đợt đăng ký,
> Quản lý phòng/giảng viên...) **chưa được xây dựng giao diện** trong bản này — phần
> service (`RegistrationPeriodService`) đã có sẵn logic, chỉ thiếu View/ViewModel
> tương ứng (xem mục 11).

---

## 6. Kiến trúc & cấu trúc thư mục

Ứng dụng theo mô hình **MVVM** (Model – View – ViewModel), điều hướng thủ công qua
`MainWindowViewModel` (không dùng framework điều hướng ngoài).

```
EduPath.Avalonia/
├── App.axaml, App.axaml.cs      # Style toàn cục (bảng màu navy/gold), khởi tạo MainWindow
├── Program.cs                    # Entry point, cấu hình AppBuilder
├── app.manifest                  # Manifest ứng dụng Windows (DPI-aware...)
│
├── Models/                       # Các thực thể dữ liệu thuần (POCO), không phụ thuộc UI
│   ├── Account.cs                 # Tài khoản đăng nhập (Username, PasswordHash, Role, LinkedId)
│   ├── Student.cs                 # Hồ sơ sinh viên, học phần đã hoàn thành, giới hạn tín chỉ
│   ├── Lecturer.cs                 # Giảng viên
│   ├── Course.cs                   # Học phần (môn học) — mã, tên, tín chỉ, tiên quyết
│   ├── Section.cs                  # Lớp học phần cụ thể (1 Course mở trong 1 học kỳ, có GV/phòng/lịch)
│   ├── Room.cs                     # Phòng học
│   ├── RegistrationPeriod.cs       # Đợt đăng ký (thời gian mở/đóng, giới hạn tín chỉ)
│   ├── Enrollment.cs                # Một lượt đăng ký (Enrolled/Cancelled/Pending)
│   └── Role.cs                      # enum: Student / Lecturer / Admin
│
├── Data/
│   └── InMemoryStore.cs            # "CSDL" giả lập bằng List<T>, Singleton, seed dữ liệu mẫu
│                                     # (khi cần chuyển sang SQL Server/SQLite thật, chỉ cần
│                                     #  thay các List này bằng DbSet<T> của EF Core)
│
├── Services/                       # Toàn bộ nghiệp vụ (business logic), tách khỏi UI để dễ test
│   ├── AuthService.cs               # Đăng nhập, kiểm tra tài khoản/vai trò/khóa tài khoản
│   ├── EnrollmentService.cs         # Đăng ký/hủy học phần — kiểm tra tiên quyết, trùng lịch,
│   │                                 # sĩ số, vượt tín chỉ tối đa, đợt đăng ký còn mở
│   ├── SectionService.cs            # Tạo lớp học phần mới, đóng đăng ký
│   ├── CourseService.cs             # CRUD học phần
│   ├── ScheduleConflictService.cs   # Kiểm tra trùng phòng/trùng giảng viên theo khung giờ
│   ├── RegistrationPeriodService.cs # Mở/đóng đợt đăng ký
│   └── SessionContext.cs            # Lưu tài khoản đang đăng nhập trong suốt phiên làm việc
│
├── ViewModels/                     # Logic trình bày + binding cho từng View
│   ├── ViewModelBase.cs             # Lớp cơ sở implement INotifyPropertyChanged
│   ├── RelayCommand.cs              # ICommand dùng chung cho binding Button/MenuItem...
│   ├── MainWindowViewModel.cs       # Điều phối Login ⇄ StudentShell ⇄ AdminShell
│   ├── LoginViewModel.cs            # Màn đăng nhập, chọn vai trò
│   ├── StudentShellViewModel.cs     # Khung điều hướng của Sinh viên (nav pill + cache trang)
│   ├── AdminShellViewModel.cs       # Khung điều hướng của Quản trị
│   ├── StudentDashboardViewModel.cs / AdminDashboardViewModel.cs
│   ├── OpenSectionsViewModel.cs     # Đăng ký học phần
│   ├── EnrolledSectionsViewModel.cs # Đã đăng ký / hủy
│   ├── TimetableViewModel.cs        # Thời khóa biểu
│   ├── HistoryViewModel.cs          # Lịch sử đăng ký
│   ├── SectionsAdminViewModel.cs / CoursesAdminViewModel.cs
│   └── SectionRow.cs                # DTO hiển thị 1 dòng lớp học phần trong danh sách
│
└── Views/                          # Giao diện .axaml + code-behind tối thiểu tương ứng từng ViewModel
    ├── MainWindow.axaml(.cs)
    ├── LoginView.axaml(.cs)
    ├── StudentShellView.axaml(.cs) / AdminShellView.axaml(.cs)
    ├── StudentDashboardView.axaml(.cs) / AdminDashboardView.axaml(.cs)
    ├── OpenSectionsView.axaml(.cs)
    ├── EnrolledSectionsView.axaml(.cs)
    ├── TimetableView.axaml(.cs)
    ├── HistoryView.axaml(.cs)
    ├── SectionsAdminView.axaml(.cs) / CoursesAdminView.axaml(.cs)
    └── ClassBinding.cs              # Helper hỗ trợ binding CSS-class trong AXAML
```

### Nguyên tắc thiết kế đáng chú ý

- **Tách biệt hoàn toàn nghiệp vụ khỏi UI**: mọi kiểm tra (trùng lịch, tiên quyết, sĩ số...)
  nằm trong `Services/`, ViewModel chỉ gọi service và hiển thị kết quả — dễ viết unit test
  và dễ thay UI khác (WPF, Web...) mà không đụng vào logic.
- **`InMemoryStore` là Singleton**: toàn bộ Services dùng chung một instance dữ liệu,
  mô phỏng một CSDL dùng chung. Khi tắt ứng dụng, dữ liệu **không được lưu lại** —
  mọi thay đổi (đăng ký, tạo lớp...) chỉ tồn tại trong phiên chạy hiện tại.
- **Điều hướng dạng "shell + nav pill + page cache"**: `StudentShellViewModel` /
  `AdminShellViewModel` giữ một `Dictionary<string, object>` cache các trang đã tạo,
  để khi quay lại một tab, trạng thái filter/scroll không bị mất; gọi `InvalidateAll()`
  sau khi đăng ký/hủy để buộc trang tạo lại với dữ liệu mới.
- **Animation chuyển trang**: dùng `TransitioningContentControl` của Avalonia —
  CrossFade khi đăng nhập, PageSlide khi đổi tab điều hướng.

---

## 7. Luồng nghiệp vụ chính

### 7.1. Đăng nhập (`AuthService.Login`)

Thứ tự kiểm tra:
1. Tên đăng nhập/mật khẩu không được để trống.
2. Tài khoản phải tồn tại (so khớp không phân biệt hoa/thường).
3. Tài khoản phải đang **hoạt động** (`IsActive = true`).
4. Mật khẩu phải khớp.
5. **Vai trò chọn ở màn đăng nhập phải khớp** với vai trò của tài khoản.

### 7.2. Đăng ký học phần (`EnrollmentService.CanRegister` → `Register`)

Khi sinh viên bấm đăng ký một lớp, hệ thống kiểm tra **theo đúng thứ tự**:

1. Đợt đăng ký của học kỳ đó có đang **mở** không (`RegistrationPeriod.IsCurrentlyOpen`).
2. Sinh viên đã đăng ký lớp này chưa (tránh đăng ký trùng).
3. Lớp học phần có đang **mở đăng ký** không (`Section.IsOpen`).
4. Lớp còn **chỗ trống** không (`Remaining = Capacity - Enrolled`).
5. Sinh viên đã hoàn thành **học phần tiên quyết** chưa (nếu học phần yêu cầu).
6. Lớp có **trùng lịch** (cùng thứ, khung giờ giao nhau) với lớp đã đăng ký khác không.
7. Đăng ký lớp này có làm **vượt số tín chỉ tối đa** cho phép trong học kỳ không
   (`Student.MaxCreditsPerTerm`, mặc định 24).

Nếu tất cả điều kiện đều đạt, `Register()` sẽ tăng `Section.Enrolled` và tạo một bản ghi
`Enrollment` mới với trạng thái `Enrolled`.

### 7.3. Hủy đăng ký (`EnrollmentService.Cancel`)

- Chỉ hủy được nếu đợt đăng ký của học kỳ đó vẫn đang mở.
- Khi hủy: chuyển `Enrollment.Status` sang `Cancelled` (không xóa bản ghi — vẫn giữ
  trong lịch sử) và giảm `Section.Enrolled`.

### 7.4. Tạo lớp học phần mới (`SectionService.Create`)

Kiểm tra: mã lớp chưa tồn tại → học phần/phòng/giảng viên phải tồn tại → giờ bắt đầu
phải trước giờ kết thúc → **không trùng phòng hoặc trùng giảng viên** với lớp khác
cùng khung giờ (`ScheduleConflictService.FindConflicts`).

### 7.5. Thêm/vô hiệu hóa học phần (`CourseService`)

- Thêm mới: mã học phần không được trùng; nếu có học phần tiên quyết thì tiên quyết
  đó phải đã tồn tại trong hệ thống.
- Vô hiệu hóa: chỉ được phép nếu học phần đó **không còn lớp nào đang mở đăng ký**.

---

## 8. Dữ liệu mẫu có sẵn

Toàn bộ dữ liệu được seed sẵn trong `Data/InMemoryStore.cs` khi ứng dụng khởi động:

| Loại dữ liệu | Số lượng | Ví dụ |
|---|---|---|
| Phòng học | 10 | A201, B203, A305 (phòng máy), C201... |
| Giảng viên | 10 | GV0008 – Trần Minh Khoa (CNTT)... |
| Học phần | 15 | CS101 → CS201 → CS208 → CS350 (chuỗi tiên quyết), MA104, EN101... |
| Đợt đăng ký | 2 | "Đợt đăng ký HK1 2026-2027" (20–30/8/2026, đang **mở**), "Đợt bổ sung HK1" (đang đóng) |
| Lớp học phần | 20 | Trải trong học kỳ **HK1 2026-2027**, đủ các thứ 2–7, nhiều khung giờ khác nhau |
| Sinh viên | 4 | Xem bảng tài khoản demo ở mục 4 |
| Lượt đăng ký có sẵn | 8 | Mỗi sinh viên có sẵn 2 lớp đã đăng ký |

Học kỳ hiện hành đang mở đăng ký là **"HK1 2026-2027"**, giới hạn tín chỉ mỗi sinh
viên: tối thiểu 12, tối đa 24 (có thể khác theo từng `Student.MinCreditsPerTerm` /
`MaxCreditsPerTerm` nếu chỉnh sửa trong `InMemoryStore.cs`).

---

## 9. Build bản phát hành (release)

Để tự build bản chạy độc lập (không cần .NET SDK ở máy người dùng), ví dụ cho Windows x64:

```bash
dotnet publish EduPath.Avalonia.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

Các RID (runtime identifier) phổ biến khác:

| Nền tảng | RID |
|---|---|
| Windows 64-bit | `win-x64` |
| macOS Apple Silicon | `osx-arm64` |
| macOS Intel | `osx-x64` |
| Linux 64-bit | `linux-x64` |

File kết quả nằm trong `bin/Release/net8.0/<RID>/publish/`.

---

## 10. Các lỗi thường gặp khi chạy lần đầu

- **Lỗi không tìm thấy gói NuGet / mất mạng khi `dotnet restore`**: đảm bảo máy có
  kết nối Internet để tải các gói `Avalonia`, `Avalonia.Desktop`, `Avalonia.Themes.Fluent`,
  `Avalonia.Fonts.Inter`, `Avalonia.Diagnostics`.
- **Lỗi kiểu dữ liệu binding (ví dụ `NumericUpDown`)**: một vài control Avalonia yêu cầu
  kiểu `decimal`/`double` chặt chẽ hơn WinForms — nếu build lỗi ở AXAML, kiểm tra lại
  kiểu property được binding trong ViewModel tương ứng.
- **Đăng nhập báo lỗi dù đúng tài khoản/mật khẩu**: kiểm tra đã chọn đúng **vai trò**
  (Sinh viên/Quản trị) tương ứng với tài khoản chưa (xem mục 4 và 7.1).
- **Mở DevTools (Ctrl+F12)**: chỉ hoạt động ở cấu hình `Debug` (gói `Avalonia.Diagnostics`
  bị loại khỏi bản `Release` theo cấu hình trong `.csproj`).

---

## 11. Hướng phát triển tiếp

Các phần sau **chưa có giao diện** trong bản này nhưng phần service nền tảng đã sẵn
sàng hoặc dễ bổ sung theo đúng pattern `ViewModel + View` hiện có:

- **Tài khoản & phân quyền** (khóa/mở tài khoản, đổi mật khẩu, tạo tài khoản mới).
- **Quản lý đợt đăng ký** — giao diện cho `RegistrationPeriodService` (mở/đóng đợt,
  tạo đợt mới) đã có sẵn logic, chỉ cần thêm View/ViewModel.
- **Quản lý phòng học & giảng viên** (CRUD `Room`, `Lecturer`).
- Thêm `NavItem` mới trong `AdminShellViewModel` (hoặc `StudentShellViewModel`) rồi
  map `key` sang ViewModel/View mới trong hàm `Navigate(key)`.
- Khi cần dữ liệu bền vững (không mất khi tắt ứng dụng): thay các `List<T>` trong
  `InMemoryStore` bằng `DbSet<T>` của Entity Framework Core kết nối SQL Server/SQLite —
  vì toàn bộ `Services/` chỉ thao tác qua các thuộc tính này nên tầng trên gần như
  không cần sửa.

---

## Giấy phép

Đây là dự án bài tập nhóm (`BaiTapNhom_Desktop`), phục vụ mục đích học tập.