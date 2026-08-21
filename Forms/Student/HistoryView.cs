using EduPath.WinForms.Common;
using EduPath.WinForms.Models;
using EduPath.WinForms.Services;

namespace EduPath.WinForms.Forms.Student
{
    /// <summary>Màn 08 · Lịch sử đăng ký — bao gồm cả lượt đã hủy, khác màn 06 (chỉ hiện đang hoạt động).</summary>
    public class HistoryView : UserControl
    {
        public HistoryView()
        {
            Dock = DockStyle.Fill;
            BackColor = UiTheme.Background;
            Padding = new Padding(23, 20, 23, 20);

            var title = new Label { Text = "Lịch sử đăng ký", Font = UiTheme.FontHeading, AutoSize = true, Location = new Point(23, 20) };

            var grid = GridHelper.MakeGrid();
            var gridHost = new Panel { Location = new Point(23, 60), Size = new Size(900, 420), BorderStyle = BorderStyle.FixedSingle };
            gridHost.Controls.Add(grid);

            var enrollmentService = new EnrollmentService();
            var student = SessionContext.CurrentStudent!;
            var rows = enrollmentService.GetHistory(student.StudentId).Select(h =>
            {
                var section = enrollmentService.GetSection(h.SectionId);
                var course = enrollmentService.GetCourse(section?.CourseCode ?? "");
                return new
                {
                    HocKy = section?.Term,
                    course?.CourseCode,
                    TenHocPhan = course?.CourseName,
                    LopHP = h.SectionId,
                    ThoiGian = h.RegisteredAt.ToString("dd/MM/yyyy HH:mm"),
                    TrangThai = h.Status == EnrollmentStatus.Enrolled ? "Đã đăng ký" : h.Status == EnrollmentStatus.Cancelled ? "Đã hủy" : "Đang chờ"
                };
            }).ToList();

            grid.DataSource = rows;
            if (grid.Columns.Count > 0)
            {
                grid.Columns["HocKy"].HeaderText = "Học kỳ";
                grid.Columns["CourseCode"].HeaderText = "Mã HP";
                grid.Columns["TenHocPhan"].HeaderText = "Tên học phần";
                grid.Columns["LopHP"].HeaderText = "Lớp học phần";
                grid.Columns["ThoiGian"].HeaderText = "Thời gian";
                grid.Columns["TrangThai"].HeaderText = "Trạng thái";
            }

            Controls.AddRange(new Control[] { title, gridHost });
        }
    }
}
