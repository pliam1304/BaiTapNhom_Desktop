using EduPath.WinForms.Common;
using EduPath.WinForms.Models;
using EduPath.WinForms.Services;

namespace EduPath.WinForms.Forms.Student
{
    /// <summary>
    /// Gộp màn 04 (Chi tiết lớp học phần) và 05 (Xác nhận đăng ký) thành một modal — đúng luồng
    /// người dùng thật: xem chi tiết rồi bấm xác nhận, thay vì 2 màn tách rời như mockup demo.
    /// </summary>
    public class FrmConfirmRegistration : Form
    {
        private readonly EnrollmentService _enrollmentService = new();
        public bool RegisteredSuccessfully { get; private set; }

        public FrmConfirmRegistration(Section section)
        {
            var course = _enrollmentService.GetCourse(section.CourseCode);
            var lecturer = _enrollmentService.GetLecturer(section.LecturerId);
            var student = SessionContext.CurrentStudent!;

            Text = "Chi tiết lớp học phần";
            Width = 430;
            Height = 430;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false; MinimizeBox = false;
            BackColor = Color.White;
            Font = UiTheme.FontBase;
            Padding = new Padding(22);

            var lblLabel = new Label { Text = "LỚP HỌC PHẦN", ForeColor = UiTheme.TextMuted, Font = new Font("Segoe UI", 8F), AutoSize = true, Location = new Point(22, 18) };
            var lblTitle = new Label { Text = $"{section.SectionId} · {course?.CourseName}", Font = new Font("Segoe UI", 12F, FontStyle.Bold), MaximumSize = new Size(380, 0), AutoSize = true, Location = new Point(22, 36) };
            var badge = UiTheme.MakeBadge($"{(section.IsOpen ? "Đang mở" : "Đã đóng")} · {section.Enrolled}/{section.Capacity}", section.Remaining <= 5 ? "warn" : "ok");
            badge.Location = new Point(22, 70);

            var detailsBox = new Label
            {
                Text = $"Học phần: {section.CourseCode} · {course?.Credits} tín chỉ · Khoa {course?.Faculty}\n" +
                       $"Giảng viên: {lecturer?.FullName}\n" +
                       $"Lịch học: {section.DayLabel}, {section.TimeLabel} · Phòng: {section.RoomId}\n" +
                       $"Tiên quyết: {(course?.PrerequisiteCode ?? "Không có")}",
                BackColor = ColorTranslator.FromHtml("#f6f8fb"),
                Padding = new Padding(10),
                Location = new Point(22, 100),
                Size = new Size(380, 90)
            };

            var check = _enrollmentService.CanRegister(student, section);
            var noteBox = new Label
            {
                Text = check.CanRegister
                    ? $"Kiểm tra nhanh: còn {section.Remaining} chỗ · chưa trùng lịch · đạt điều kiện tiên quyết."
                    : string.Join("\n", check.Reasons),
                BackColor = check.CanRegister ? ColorTranslator.FromHtml("#fff7df") : ColorTranslator.FromHtml("#fdecea"),
                ForeColor = check.CanRegister ? UiTheme.TextDark : Color.Firebrick,
                Padding = new Padding(9),
                Location = new Point(22, 200),
                Size = new Size(380, 70)
            };

            var btnCancel = UiTheme.MakeOutlineButton("Quay lại danh sách");
            btnCancel.Location = new Point(22, 300);
            btnCancel.Click += (_, _) => Close();

            var btnConfirm = UiTheme.MakeYellowButton($"Xác nhận đăng ký lớp {section.SectionId}");
            btnConfirm.Location = new Point(180, 300);
            btnConfirm.Enabled = check.CanRegister;
            btnConfirm.Click += (_, _) =>
            {
                var (ok, error) = _enrollmentService.Register(student, section);
                if (!ok)
                {
                    MessageBox.Show(error, "Không thể đăng ký", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                RegisteredSuccessfully = true;
                MessageBox.Show($"Đăng ký lớp {section.SectionId} thành công.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
            };

            Controls.AddRange(new Control[] { lblLabel, lblTitle, badge, detailsBox, noteBox, btnCancel, btnConfirm });
        }
    }
}
