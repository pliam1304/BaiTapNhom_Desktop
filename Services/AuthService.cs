using EduPath.Avalonia.Data;
using EduPath.Avalonia.Models;

namespace EduPath.Avalonia.Services
{
    public class AuthResult
    {
        public bool Success { get; init; }
        public string? ErrorMessage { get; init; }
        public Account? Account { get; init; }
        public Student? Student { get; init; }

        public static AuthResult Fail(string message) => new() { Success = false, ErrorMessage = message };
        public static AuthResult Ok(Account acc, Student? st) => new() { Success = true, Account = acc, Student = st };
    }

    /// <summary>
    /// Xử lý màn 01 · Đăng nhập. Tách riêng khỏi Form để có thể unit-test và tái sử dụng
    /// (ví dụ sau này thêm đăng nhập qua SSO/AD thì chỉ sửa ở đây).
    /// </summary>
    public class AuthService
    {
        private readonly InMemoryStore _store = InMemoryStore.Instance;

        // //sửa lại không can Role nữa, vì đã có thể nhận diện role từ account
        // public AuthResult Login(string username, string password, Role expectedRole)
        // {
        //     if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        //         return AuthResult.Fail("Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu.");

        //     var account = _store.Accounts.FirstOrDefault(a =>
        //         a.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

        //     if (account is null)
        //         return AuthResult.Fail("Tài khoản không tồn tại.");

        //     if (!account.IsActive)
        //         return AuthResult.Fail("Tài khoản đã bị khóa. Vui lòng liên hệ Phòng Đào tạo.");

        //     if (account.PasswordHash != password)
        //         return AuthResult.Fail("Mật khẩu không đúng.");

        //     if (account.Role != expectedRole)
        //         return AuthResult.Fail($"Tài khoản này không có vai trò '{expectedRole}'. Vui lòng chọn đúng vai trò đăng nhập.");

        //     account.LastLoginAt = DateTime.Now;

        //     Student? student = account.Role == Role.Student
        //         ? _store.Students.FirstOrDefault(s => s.StudentId == account.LinkedId)
        //         : null;

        //     return AuthResult.Ok(account, student);
        //}
        // Thay vì: public LoginResult Login(string username, string password, Role expectedRole)
        public AuthResult Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return AuthResult.Fail("Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu.");

            var account = _store.Accounts.FirstOrDefault(a =>
                a.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

            if (account is null)
                return AuthResult.Fail("Tài khoản không tồn tại.");

            if (!account.IsActive)
                return AuthResult.Fail("Tài khoản đã bị khóa. Vui lòng liên hệ Phòng Đào tạo.");

            if (account.PasswordHash != password)
                return AuthResult.Fail("Mật khẩu không đúng.");

            // Đã bỏ đoạn kiểm tra `account.Role != expectedRole` vì không cần chọn vai trò trước nữa.
            // Hệ thống sẽ tự động nhận diện quyền Admin hay Student trực tiếp từ tài khoản tìm được.

            account.LastLoginAt = DateTime.Now;

            Student? student = account.Role == Role.Student
                ? _store.Students.FirstOrDefault(s => s.StudentId == account.LinkedId)
                : null;

            return AuthResult.Ok(account, student);
        }
    }
}
