using EduPath.WinForms.Common;
using EduPath.WinForms.Services;

namespace EduPath.WinForms.Forms.Student
{
    /// <summary>Màn 02 · Dashboard sinh viên — số liệu lấy thật từ EnrollmentService, không hard-code.</summary>
    public class StudentDashboardView : UserControl
    {
        private readonly EnrollmentService _enrollmentService = new();

        public StudentDashboardView(MainShellForm shell)
        {
            AutoScroll = true;
            Padding = new Padding(23, 20, 23, 20);
            BackColor = UiTheme.Background;

            var student = SessionContext.CurrentStudent!;
            var period = new RegistrationPeriodService().GetCurrent();

            var title = new Label { Text = "Tổng quan đăng ký", Font = UiTheme.FontHeading, AutoSize = true, Location = new Point(23, 20) };
            var sub = new Label { Text = "Học kỳ 1 · Năm học 2026–2027", ForeColor = UiTheme.TextMuted, AutoSize = true, Location = new Point(23, 48) };

            var enrolledCount = _enrollmentService.GetActiveEnrollments(student.StudentId).Count();
            var credits = _enrollmentService.GetTotalRegisteredCredits(student.StudentId);
            var pending = 0; // demo: chưa có quy trình duyệt vượt tín chỉ đang chờ
            var daysLeft = period != null ? Math.Max(0, (period.EndDate.Date - DateTime.Now.Date).Days) : 0;

            var kpiRow = new FlowLayoutPanel { Location = new Point(23, 78), AutoSize = true, FlowDirection = FlowDirection.LeftToRight };
            kpiRow.Controls.Add(WithMargin(GridHelper.MakeKpiCard("LỚP ĐÃ ĐĂNG KÝ", enrolledCount.ToString())));
            kpiRow.Controls.Add(WithMargin(GridHelper.MakeKpiCard("TỔNG TÍN CHỈ", credits.ToString(), UiTheme.Gold)));
            kpiRow.Controls.Add(WithMargin(GridHelper.MakeKpiCard("ĐANG CHỜ XỬ LÝ", pending.ToString())));
            kpiRow.Controls.Add(WithMargin(GridHelper.MakeKpiCard("CÒN LẠI ĐỢT ĐK", $"{daysLeft} ngày", UiTheme.BadgeGreenText)));

            var card = GridHelper.MakeCard();
            card.Location = new Point(23, 185);
            card.Size = new Size(760, 150);

            var cardTitle = new Label { Text = "Đăng ký học phần HK1 – 2026/2027", Font = new Font("Segoe UI", 11F, FontStyle.Bold), AutoSize = true, Location = new Point(14, 12) };
            var cardSub = new Label
            {
                Text = period != null ? $"{period.StartDate:dd/MM/yyyy} – {period.EndDate:dd/MM/yyyy} · Tối đa {student.MaxCreditsPerTerm} tín chỉ" : "Chưa có đợt đăng ký",
                ForeColor = UiTheme.TextMuted, AutoSize = true, Location = new Point(14, 34)
            };
            var badge = UiTheme.MakeBadge(period?.IsCurrentlyOpen(DateTime.Now) == true ? "ĐANG MỞ" : "ĐÃ ĐÓNG",
                period?.IsCurrentlyOpen(DateTime.Now) == true ? "ok" : "off");
            badge.Location = new Point(650, 14);

            var note = new Label
            {
                Text = $"Bạn hiện có {credits}/{student.MaxCreditsPerTerm} tín chỉ. Hệ thống sẽ kiểm tra tiên quyết, trùng lịch và sĩ số trước khi xác nhận.",
                BackColor = ColorTranslator.FromHtml("#fff7df"),
                Padding = new Padding(9),
                MaximumSize = new Size(730, 0),
                AutoSize = true,
                Location = new Point(14, 60)
            };

            var btnRegister = UiTheme.MakeYellowButton("Đăng ký học phần");
            btnRegister.Location = new Point(14, 105);
            btnRegister.Click += (_, _) => shell.NavigateTo("open-sections");

            card.Controls.AddRange(new Control[] { cardTitle, cardSub, badge, note, btnRegister });

            Controls.AddRange(new Control[] { title, sub, kpiRow, card });
        }

        private static Control WithMargin(Control c) { c.Margin = new Padding(0, 0, 12, 12); return c; }
    }
}
