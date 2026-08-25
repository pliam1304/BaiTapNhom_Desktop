*** Cấu trúc của dự án được tổ chức với chức năng như dưới đây ***

BaiTapNhom_Desktop/
├── Data/                  <-- Chứa các lớp xử lý dữ liệu giả lập (in-memory store) hoặc kết nối cơ sở dữ liệu.
├── Models/                <-- Chứa các lớp đối tượng thực thể (Entity/DTO) đại diện cho dữ liệu (Sinh viên, Môn học, Lớp học phần, Tài khoản...).
├── Services/              <-- Chứa các lớp logic nghiệp vụ (xử lý đăng ký học phần, xác thực tài khoản, kiểm tra xung đột lịch học...).
├── ViewModels/            <-- Chứa các lớp xử lý logic giao diện, kết nối giữa View và Model theo mô hình MVVM.
├── Views/                 <-- Chứa các file giao diện người dùng (.axaml) và code-behind tương ứng (.axaml.cs).
    ├── Features/                  <-- Thư mục gốc chứa toàn bộ các màn hình và tính năng được chia nhỏ theo module.
    │   ├── AdminDashboard/        <-- Chứa giao diện, code-behind và ViewModel cho trang Tổng quan của Quản trị viên.
    │   ├── CoursesAdmin/          <-- Chứa các file liên quan đến tính năng Quản lý môn học dành cho Admin.
    │   ├── EnrolledSections/      <-- Chứa giao diện và logic hiển thị danh sách các học phần sinh viên đã đăng ký.
    │   ├── History/               <-- Chứa giao diện và logic tra cứu lịch sử (đăng ký/thao tác hệ thống).
    │   ├── Login/                 <-- Chứa màn hình và logic xử lý đăng nhập của ứng dụng.
    │   ├── OpenSections/          <-- Chứa giao diện và danh sách các lớp học phần đang mở cho phép sinh viên đăng ký.
    │   ├── SectionsAdmin/         <-- Chứa tính năng Quản lý lớp học phần (thêm/sửa/xóa) dành cho Admin.
    │   ├── Shells/                <-- Chứa các khung giao diện chính chứa menu điều hướng (Layout phân quyền cho Admin và Sinh viên).
    │   ├── StudentDashboard/      <-- Chứa giao diện, code-behind và ViewModel cho trang Tổng quan của Sinh viên.
    │   └── Timetable/             <-- Chứa giao diện và logic hiển thị thời khóa biểu của sinh viên.
├── App.axaml              <-- File cấu hình gốc của ứng dụng Avalonia, khai báo tài nguyên (Resource) và styles dùng chung toàn app.
├── App.axaml.cs           <-- Code-behind của App.axaml, khởi chạy ứng dụng.
├── app.manifest           <-- File cấu hình quyền và giao diện hệ thống cho ứng dụng nền tảng.
├── EduPath.Avalonia.csproj<-- File dự án chính (.NET project file), quản lý các thư viện, package và cấu hình biên dịch.
├── Program.cs             <-- Điểm khởi đầu (Entry point) thực thi chương trình của ứng dụng C#.
└── README.md              <-- File tài liệu mô tả thông tin, hướng dẫn sử dụng và cấu trúc của dự án.