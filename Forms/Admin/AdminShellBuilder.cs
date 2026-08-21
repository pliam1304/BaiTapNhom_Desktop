using EduPath.WinForms.Common;

namespace EduPath.WinForms.Forms.Admin
{
    /// <summary>
    /// Lắp ráp MainShellForm cho vai trò Quản trị viên. Chỉ đăng ký sẵn Dashboard + Quản lý lớp
    /// học phần (đại diện đủ luồng CRUD + kiểm tra xung đột lịch). Các màn còn lại (Học phần,
    /// Sinh viên, Giảng viên, Đợt đăng ký, Phòng học, Tài khoản...) dùng đúng pattern của
    /// SectionsAdminView: DataGridView nạp từ Service tương ứng + nút thao tác gọi Service.
    /// </summary>
    public static class AdminShellBuilder
    {
        public static MainShellForm Build()
        {
            var shell = new MainShellForm();

            // Đăng ký từng view theo key điều hướng (khớp đúng navKey khai báo trong SideMenu.cs)
            shell.RegisterView("admin-dashboard", "Dashboard quản trị", () => new AdminDashboardView(shell));
            shell.RegisterView("sections-admin", "Quản lý lớp học phần", () => new SectionsAdminView());

            // 6 màn quản trị còn lại — đã dựng đầy đủ theo đúng mẫu SectionsAdminView (Grid + Service)
            shell.RegisterView("courses", "Quản lý học phần", () => new CoursesAdminView());
            shell.RegisterView("students", "Quản lý sinh viên", () => new StudentsAdminView());
            shell.RegisterView("lecturers", "Quản lý giảng viên", () => new LecturersAdminView());
            shell.RegisterView("periods", "Học kỳ & Đợt đăng ký", () => new PeriodsAdminView());
            shell.RegisterView("rooms", "Phòng & Lịch học", () => new RoomsAdminView());
            shell.RegisterView("accounts", "Tài khoản & Phân quyền", () => new AccountsAdminView());

            shell.NavigateTo("admin-dashboard");
            return shell;
        }
    }
}
