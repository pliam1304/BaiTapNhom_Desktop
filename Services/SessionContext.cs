using EduPath.WinForms.Models;

namespace EduPath.WinForms.Services
{
    /// <summary>
    /// Giữ thông tin người dùng đang đăng nhập trong suốt phiên làm việc (thay cho việc
    /// mỗi Form tự truyền username/role qua constructor). Reset khi đăng xuất.
    /// </summary>
    public static class SessionContext
    {
        public static Account? CurrentAccount { get; private set; }
        public static Student? CurrentStudent { get; private set; }

        public static void SignIn(Account account, Student? student)
        {
            CurrentAccount = account;
            CurrentStudent = student;
        }

        public static void SignOut()
        {
            CurrentAccount = null;
            CurrentStudent = null;
        }

        public static bool IsAdmin => CurrentAccount?.Role == Role.Admin;
        public static bool IsStudent => CurrentAccount?.Role == Role.Student;
    }
}
