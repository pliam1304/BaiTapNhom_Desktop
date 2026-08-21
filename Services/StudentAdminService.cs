using EduPath.WinForms.Data;   // Cần InMemoryStore để đọc/ghi "CSDL" giả lập trong RAM
using EduPath.WinForms.Models; // Cần Student, Account, Role để thao tác dữ liệu nghiệp vụ

namespace EduPath.WinForms.Services
{
    /// <summary>
    /// Nghiệp vụ cho màn "Quản lý sinh viên" (admin). Tách riêng khỏi EnrollmentService vì
    /// EnrollmentService chỉ lo đăng ký/hủy học phần, còn service này lo hồ sơ sinh viên
    /// (tạo mới, tra cứu, khóa/mở hồ sơ) — đúng nguyên tắc mỗi Service một trách nhiệm.
    /// </summary>
    public class StudentAdminService
    {
        // Lấy tham chiếu duy nhất tới "CSDL" trong RAM (Singleton) để mọi thao tác dùng chung 1 danh sách
        private readonly InMemoryStore _store = InMemoryStore.Instance;

        /// <summary>Trả về toàn bộ danh sách sinh viên để đổ vào DataGridView.</summary>
        public IReadOnlyList<Student> GetAll() => _store.Students; // Trả nguyên list, UI chỉ đọc (read-only)

        /// <summary>Tìm một sinh viên theo mã số sinh viên (MSSV), trả về null nếu không có.</summary>
        public Student? GetById(string studentId) =>
            _store.Students.FirstOrDefault(s => s.StudentId == studentId); // So khớp đúng MSSV

        /// <summary>
        /// Tạo hồ sơ sinh viên mới, đồng thời tự tạo luôn tài khoản đăng nhập tương ứng
        /// (đúng quan hệ Account 1-1 Student mô tả trong Models/Account.cs).
        /// </summary>
        public (bool ok, string? error) Add(Student student, string initialPassword)
        {
            // Bước 1: kiểm tra MSSV không được để trống
            if (string.IsNullOrWhiteSpace(student.StudentId))
                return (false, "Mã sinh viên không được để trống.");

            // Bước 2: kiểm tra trùng MSSV trong danh sách hiện có
            if (_store.Students.Any(s => s.StudentId == student.StudentId))
                return (false, $"Mã sinh viên '{student.StudentId}' đã tồn tại.");

            // Bước 3: kiểm tra họ tên không được bỏ trống
            if (string.IsNullOrWhiteSpace(student.FullName))
                return (false, "Họ tên sinh viên không được để trống.");

            // Bước 4: kiểm tra mật khẩu khởi tạo hợp lệ (đơn giản hóa cho bản demo)
            if (string.IsNullOrWhiteSpace(initialPassword))
                return (false, "Vui lòng nhập mật khẩu khởi tạo cho tài khoản sinh viên.");

            // Bước 5: kiểm tra username (trùng MSSV) chưa từng được dùng ở bảng Account
            if (_store.Accounts.Any(a => a.Username.Equals(student.StudentId, StringComparison.OrdinalIgnoreCase)))
                return (false, $"Tài khoản '{student.StudentId}' đã tồn tại trong hệ thống.");

            // Mọi kiểm tra hợp lệ -> thêm hồ sơ sinh viên vào danh sách
            _store.Students.Add(student);

            // Tự động tạo tài khoản đăng nhập gắn với sinh viên vừa tạo (LinkedId trỏ về StudentId)
            _store.Accounts.Add(new Account
            {
                Username = student.StudentId,      // Username mặc định = MSSV
                PasswordHash = initialPassword,     // Demo: lưu plain text (thực tế phải hash trước khi lưu)
                Role = Role.Student,                // Gán vai trò Sinh viên
                LinkedId = student.StudentId,       // Liên kết ngược về hồ sơ Student
                IsActive = true                     // Tài khoản mới mặc định đang hoạt động
            });

            return (true, null); // Thành công, không có lỗi
        }

        /// <summary>Khóa/nghỉ học một sinh viên — không xóa hẳn dữ liệu để giữ lịch sử đăng ký.</summary>
        public (bool ok, string? error) Deactivate(string studentId)
        {
            var student = GetById(studentId);           // Tìm sinh viên cần khóa
            if (student is null) return (false, "Không tìm thấy sinh viên."); // Không có -> báo lỗi

            student.IsActive = false; // Đánh dấu ngừng học, KHÔNG xóa record (giữ lịch sử Enrollment)

            // Đồng bộ: khóa luôn tài khoản đăng nhập gắn với sinh viên này
            var account = _store.Accounts.FirstOrDefault(a => a.LinkedId == studentId);
            if (account != null) account.IsActive = false; // Nếu có tài khoản thì khóa luôn

            return (true, null); // Thành công
        }

        /// <summary>Mở lại (kích hoạt lại) một sinh viên đã bị khóa.</summary>
        public (bool ok, string? error) Activate(string studentId)
        {
            var student = GetById(studentId);              // Tìm sinh viên cần mở lại
            if (student is null) return (false, "Không tìm thấy sinh viên."); // Không có -> báo lỗi

            student.IsActive = true; // Đánh dấu đang học lại

            var account = _store.Accounts.FirstOrDefault(a => a.LinkedId == studentId); // Tìm tài khoản liên kết
            if (account != null) account.IsActive = true; // Mở lại tài khoản tương ứng

            return (true, null); // Thành công
        }
    }
}
