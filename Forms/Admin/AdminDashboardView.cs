using EduPath.WinForms.Common;
using EduPath.WinForms.Data;
using EduPath.WinForms.Services;

namespace EduPath.WinForms.Forms.Admin
{
    /// <summary>Màn 09 · Dashboard quản trị — số "Cần xử lý" tính thật từ dữ liệu (không hard-code).</summary>
    public class AdminDashboardView : UserControl
    {
        public AdminDashboardView(MainShellForm shell)
        {
            Dock = DockStyle.Fill;
            BackColor = UiTheme.Background;
            Padding = new Padding(23, 20, 23, 20);

            var store = InMemoryStore.Instance;
            var period = new RegistrationPeriodService().GetCurrent();

            var title = new Label { Text = "Dashboard quản trị", Font = UiTheme.FontHeading, AutoSize = true, Location = new Point(23, 20) };

            var kpiRow = new FlowLayoutPanel { Location = new Point(23, 60), AutoSize = true, FlowDirection = FlowDirection.LeftToRight };
            var c1 = GridHelper.MakeKpiCard("ĐỢT ĐĂNG KÝ", period?.IsCurrentlyOpen(DateTime.Now) == true ? "Đang mở" : "Đã đóng", UiTheme.BadgeGreenText);
            var c2 = GridHelper.MakeKpiCard("LỚP ĐANG MỞ", store.Sections.Count(s => s.IsOpen).ToString());
            var c3 = GridHelper.MakeKpiCard("LƯỢT ĐĂNG KÝ", store.Enrollments.Count.ToString(), UiTheme.Gold);
            c1.Margin = new Padding(0, 0, 12, 0); c2.Margin = new Padding(0, 0, 12, 0);
            kpiRow.Controls.AddRange(new Control[] { c1, c2, c3 });

            int nearFullCount = store.Sections.Count(s => s.IsOpen && s.Remaining > 0 && s.Remaining <= 5);
            var conflictService = new ScheduleConflictService();
            int conflictCount = store.Sections.Count(s => conflictService.FindConflicts(s).Count > 0) / 2; // mỗi cặp bị đếm 2 lần
            int overLimitCount = store.Students.Count(st => new EnrollmentService().GetTotalRegisteredCredits(st.StudentId) > st.MaxCreditsPerTerm);

            var card = GridHelper.MakeCard();
            card.Location = new Point(23, 165);
            card.Size = new Size(760, 130);
            var cardTitle = new Label { Text = "Cần xử lý", Font = new Font("Segoe UI", 11F, FontStyle.Bold), AutoSize = true, Location = new Point(14, 12) };
            var note = new Label
            {
                Text = $"{nearFullCount:00} lớp gần đầy · {conflictCount:00} lịch học có xung đột phòng/giảng viên · {overLimitCount:00} sinh viên vượt số tín chỉ tối đa.",
                BackColor = ColorTranslator.FromHtml("#fff7df"),
                Padding = new Padding(9),
                MaximumSize = new Size(730, 0),
                AutoSize = true,
                Location = new Point(14, 40)
            };
            var btn = UiTheme.MakeYellowButton("Quản lý đợt đăng ký");
            btn.Location = new Point(14, 85);
            btn.Click += (_, _) => shell.NavigateTo("periods");
            card.Controls.AddRange(new Control[] { cardTitle, note, btn });

            Controls.AddRange(new Control[] { title, kpiRow, card });
        }
    }
}
