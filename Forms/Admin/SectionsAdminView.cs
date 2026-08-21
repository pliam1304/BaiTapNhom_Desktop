using EduPath.WinForms.Common;
using EduPath.WinForms.Services;

namespace EduPath.WinForms.Forms.Admin
{
    /// <summary>
    /// Màn 11 · Quản lý lớp học phần. Đây là view MẪU cho mọi màn quản trị dạng danh sách khác:
    /// (1) DataGridView nạp dữ liệu từ Service, (2) nút "+ Tạo..." mở Form modal gọi Service.Create,
    /// (3) sau khi Service trả về lỗi nghiệp vụ (vd. xung đột lịch) thì hiển thị ngay, không tự xử lý ở UI.
    /// </summary>
    public class SectionsAdminView : UserControl
    {
        private readonly SectionService _sectionService = new();
        private readonly EnrollmentService _enrollmentService = new();
        private readonly DataGridView _grid = GridHelper.MakeGrid();

        public SectionsAdminView()
        {
            Dock = DockStyle.Fill;
            BackColor = UiTheme.Background;
            Padding = new Padding(23, 20, 23, 20);

            var title = new Label { Text = "Quản lý lớp học phần", Font = UiTheme.FontHeading, AutoSize = true, Location = new Point(23, 20) };

            var btnCreate = UiTheme.MakeYellowButton("+ Tạo lớp học phần");
            btnCreate.Location = new Point(750, 22);
            btnCreate.Click += (_, _) =>
            {
                using var dlg = new FrmCreateSection();
                dlg.ShowDialog();
                if (dlg.Created) LoadData();
            };

            var gridHost = new Panel { Location = new Point(23, 60), Size = new Size(900, 500), BorderStyle = BorderStyle.FixedSingle };
            gridHost.Controls.Add(_grid);

            Controls.AddRange(new Control[] { title, btnCreate, gridHost });
            LoadData();
        }

        private void LoadData()
        {
            var rows = _sectionService.GetAll().Select(s =>
            {
                var lecturer = _enrollmentService.GetLecturer(s.LecturerId);
                string status = !s.IsOpen ? "Đã đóng" : s.Remaining <= 0 ? "Đã đầy" : s.Remaining <= 5 ? "Gần đầy" : "Đang mở";
                return new
                {
                    s.SectionId,
                    HocPhan = s.CourseCode,
                    GiangVien = lecturer?.FullName,
                    s.Term,
                    s.RoomId,
                    Lich = $"{s.DayLabel} {s.TimeLabel}",
                    SiSo = $"{s.Enrolled}/{s.Capacity}",
                    TrangThai = status
                };
            }).ToList();

            _grid.DataSource = rows;
            if (_grid.Columns.Count > 0)
            {
                _grid.Columns["SectionId"].HeaderText = "Mã lớp";
                _grid.Columns["HocPhan"].HeaderText = "Học phần";
                _grid.Columns["GiangVien"].HeaderText = "GV";
                _grid.Columns["Term"].HeaderText = "Học kỳ";
                _grid.Columns["RoomId"].HeaderText = "Phòng";
                _grid.Columns["Lich"].HeaderText = "Lịch";
                _grid.Columns["SiSo"].HeaderText = "Sĩ số";
                _grid.Columns["TrangThai"].HeaderText = "Trạng thái";
            }
        }
    }
}
