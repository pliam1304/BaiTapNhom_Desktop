using EduPath.WinForms.Common;

namespace EduPath.WinForms.Forms.Student
{
    /// <summary>Lắp ráp MainShellForm cho vai trò Sinh viên: đăng ký từng view vào shell rồi mở dashboard.</summary>
    public static class StudentShellBuilder
    {
        public static MainShellForm Build()
        {
            var shell = new MainShellForm();

            shell.RegisterView("student-dashboard", "Tổng quan", () => new StudentDashboardView(shell));
            shell.RegisterView("open-sections", "Học phần mở đăng ký", () => new OpenSectionsView(shell));
            shell.RegisterView("enrolled", "Học phần đã đăng ký", () => new EnrolledSectionsView());
            shell.RegisterView("timetable", "Lịch học", () => new TimetableView());
            shell.RegisterView("history", "Lịch sử đăng ký", () => new HistoryView());

            shell.NavigateTo("student-dashboard");
            return shell;
        }
    }
}
