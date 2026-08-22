namespace EduPath.Avalonia.Models
{
    /// <summary>
    /// Tài khoản đăng nhập. Một Account có thể gắn với một Student hoặc Lecturer qua LinkedId
    /// (Admin thì LinkedId = null). Tách Account khỏi Student/Lecturer để hỗ trợ màn hình
    /// "Tài khoản & phân quyền" (17) độc lập với hồ sơ nghiệp vụ.
    /// </summary>
    public class Account
    {
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty; // demo: lưu plain, thực tế phải hash
        public Role Role { get; set; }
        public string? LinkedId { get; set; }          // MSSV hoặc Mã GV tương ứng
        public bool IsActive { get; set; } = true;
        public DateTime? LastLoginAt { get; set; }
    }
}
