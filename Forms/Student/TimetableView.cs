using EduPath.WinForms.Common;
using EduPath.WinForms.Services;

namespace EduPath.WinForms.Forms.Student
{
    /// <summary>
    /// Màn 07 · Lịch học cá nhân. Khác mockup (dữ liệu cứng trong HTML), ở đây lưới lịch được
    /// dựng động từ các Section sinh viên đang đăng ký, đặt đúng ô Thứ/Giờ tương ứng.
    /// </summary>
    public class TimetableView : UserControl
    {
        private static readonly string[] DayLabels = { "Thứ 2", "Thứ 3", "Thứ 4", "Thứ 5", "Thứ 6" };
        private static readonly (TimeSpan start, TimeSpan end)[] Slots =
        {
            (new TimeSpan(7,0,0), new TimeSpan(9,0,0)),
            (new TimeSpan(9,0,0), new TimeSpan(11,0,0)),
            (new TimeSpan(13,0,0), new TimeSpan(15,0,0)),
            (new TimeSpan(15,0,0), new TimeSpan(17,0,0)),
        };

        public TimetableView()
        {
            Dock = DockStyle.Fill;
            BackColor = UiTheme.Background;
            Padding = new Padding(23, 20, 23, 20);

            var title = new Label { Text = "Lịch học cá nhân", Font = UiTheme.FontHeading, AutoSize = true, Location = new Point(23, 20) };
            var sub = new Label { Text = "Tuần 01 · 17/08–23/08/2026", ForeColor = UiTheme.TextMuted, AutoSize = true, Location = new Point(23, 48) };

            var table = new TableLayoutPanel
            {
                Location = new Point(23, 80),
                Size = new Size(900, 380),
                ColumnCount = 6,
                RowCount = Slots.Length + 1,
                BackColor = Color.White,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
            for (int i = 0; i < 5; i++) table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            for (int i = 0; i < Slots.Length; i++) table.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));

            table.Controls.Add(new Label { Text = "", Dock = DockStyle.Fill }, 0, 0);
            for (int d = 0; d < DayLabels.Length; d++)
                table.Controls.Add(new Label { Text = DayLabels[d], Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 9F, FontStyle.Bold) }, d + 1, 0);

            var enrollmentService = new EnrollmentService();
            var student = SessionContext.CurrentStudent!;
            var mySections = enrollmentService.GetActiveEnrollments(student.StudentId)
                .Select(e => enrollmentService.GetSection(e.SectionId)!)
                .ToList();

            for (int r = 0; r < Slots.Length; r++)
            {
                var (start, end) = Slots[r];
                table.Controls.Add(new Label { Text = $"{start:hh\\:mm}\n{end:hh\\:mm}", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 7.5F) }, 0, r + 1);

                for (int d = 0; d < DayLabels.Length; d++)
                {
                    int dow = d + 2; // Thứ 2 = 2
                    var match = mySections.FirstOrDefault(s => s.DayOfWeek == dow && s.StartTime < end && start < s.EndTime);
                    Control cell;
                    if (match != null)
                    {
                        var course = enrollmentService.GetCourse(match.CourseCode);
                        var lecturer = enrollmentService.GetLecturer(match.LecturerId);
                        cell = new Label
                        {
                            Text = $"{match.SectionId}\n{match.RoomId} · {lecturer?.FullName}",
                            Dock = DockStyle.Fill,
                            BackColor = ColorTranslator.FromHtml("#dbeafe"),
                            ForeColor = UiTheme.NavyLight,
                            Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                            Padding = new Padding(4),
                            TextAlign = ContentAlignment.TopLeft
                        };
                    }
                    else
                    {
                        cell = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
                    }
                    table.Controls.Add(cell, d + 1, r + 1);
                }
            }

            Controls.AddRange(new Control[] { title, sub, table });
        }
    }
}
