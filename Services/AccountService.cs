using EduPath.WinForms.Data;   // InMemoryStore lưu danh sách Accounts dùng chung toàn app
using EduPath.WinForms.Models; // Model Account, Role

namespace EduPath.WinForms.Services
{
    /// <summary>
    /// Nghiệp vụ cho màn "Tài khoản & Phân quyền" (admin). Khác với AuthService (chỉ lo đăng nhập),
    /// service này lo phần quản trị: tạo tài khoản độc lập (vd tài khoản Admin/Lecturer mới),
    /// khóa/mở, đổi mật khẩu — đúng như mô tả trong Models/Account.cs.
    /// </summary>
    public class AccountService
    {
        // Tham chiếu duy nhất tới kho dữ liệu trong RAM
        private readonly InMemoryStore _store = InMemoryStore.Instance;

        /// <summary>Toàn bộ danh sách tài khoản để hiển thị lên grid.</summary>
        public IReadOnlyList<Account> GetAll() => _store.Accounts;

        /// <summary>Tìm tài khoản theo username.</summary>
        public Account? GetByUsername(string username) =>
            _store.Accounts.FirstOrDefault(a => a.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
            // Dùng OrdinalIgnoreCase để không phân biệt hoa/thường, khớp cách làm trong AuthService.Login

        /// <summary>
        /// Tạo tài khoản mới độc lập (dùng cho Admin/Lecturer — Student thì nên tạo qua
        /// StudentAdminService.Add để đảm bảo luôn có hồ sơ Student đi kèm).
        /// </summary>
        public (bool ok, string? error) Add(Account account)
        {
            // Username bắt buộc phải có
            if (string.IsNullOrWhiteSpace(account.Username))
                return (false, "Tên đăng nhập không được để trống.");

            // Không cho trùng username đã tồn tại (không phân biệt hoa thường)
            if (GetByUsername(account.Username) != null)
                return (false, $"Tài khoản '{account.Username}' đã tồn tại.");

            // Mật khẩu khởi tạo bắt buộc phải có
            if (string.IsNullOrWhiteSpace(account.PasswordHash))
                return (false, "Vui lòng nhập mật khẩu khởi tạo.");

            // Nếu vai trò là Student, LinkedId phải trỏ tới một Student có thật (tránh tài khoản mồ côi)
            if (account.Role == Role.Student &&
                (account.LinkedId is null || _store.Students.All(s => s.StudentId != account.LinkedId)))
                return (false, "Tài khoản vai trò Sinh viên phải liên kết với một sinh viên đã tồn tại.");

            // Nếu vai trò là Lecturer, LinkedId phải trỏ tới một Lecturer có thật
            if (account.Role == Role.Lecturer &&
                (account.LinkedId is null || _store.Lecturers.All(l => l.LecturerId != account.LinkedId)))
                return (false, "Tài khoản vai trò Giảng viên phải liên kết với một giảng viên đã tồn tại.");

            // Mọi kiểm tra hợp lệ -> thêm tài khoản vào danh sách
            _store.Accounts.Add(account);
            return (true, null); // Thành công
        }

        /// <summary>Khóa tài khoản — chặn đăng nhập nhưng vẫn giữ lịch sử.</summary>
        public (bool ok, string? error) Lock(string username)
        {
            var account = GetByUsername(username);              // Tìm tài khoản cần khóa
            if (account is null) return (false, "Không tìm thấy tài khoản."); // Không có -> lỗi

            account.IsActive = false; // Đánh dấu ngừng hoạt động (AuthService.Login đã kiểm tra cờ này)
            return (true, null);      // Thành công
        }

        /// <summary>Mở khóa lại tài khoản.</summary>
        public (bool ok, string? error) Unlock(string username)
        {
            var account = GetByUsername(username);              // Tìm tài khoản cần mở khóa
            if (account is null) return (false, "Không tìm thấy tài khoản."); // Không có -> lỗi

            account.IsActive = true; // Cho phép đăng nhập trở lại
            return (true, null);     // Thành công
        }

        /// <summary>Đặt lại mật khẩu cho một tài khoản (vd khi sinh viên quên mật khẩu).</summary>
        public (bool ok, string? error) ResetPassword(string username, string newPassword)
        {
            var account = GetByUsername(username);              // Tìm tài khoản cần đổi mật khẩu
            if (account is null) return (false, "Không tìm thấy tài khoản."); // Không có -> lỗi

            if (string.IsNullOrWhiteSpace(newPassword))         // Mật khẩu mới không được để trống
                return (false, "Mật khẩu mới không được để trống.");

            account.PasswordHash = newPassword; // Demo: gán plain text (thực tế phải hash trước khi lưu)
            return (true, null);                // Thành công
        }
    }
}
