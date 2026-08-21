using EduPath.WinForms.Common;
using EduPath.WinForms.Models;
using EduPath.WinForms.Services;

namespace EduPath.WinForms.Forms.Student
{
    /// <summary>Màn 03 · Học phần mở đăng ký. Bấm "Đăng ký" mở FrmConfirmRegistration (màn 04+05).</summary>
    public class OpenSectionsView : UserControl
    {
        private readonly EnrollmentService _enrollmentService = new();
        private readonly DataGridView _grid = GridHelper.MakeGrid();
        private readonly TextBox _txtSearch = new() { PlaceholderText = "Tìm mã học phần, tên học phần..." };
        private readonly MainShellForm _shell;

        public OpenSectionsView(MainShellForm shell)
        {
            _shell = shell;
            Dock = DockStyle.Fill;
            BackColor = UiTheme.Background;
            Padding = new Padding(23, 20, 23, 20);

            var title = new Label { Text = "Học phần mở đăng ký", Font = UiTheme.FontHeading, AutoSize = true, Location = new Point(23, 20) };

            var filterPanel = new Panel { BackColor = Color.White, Location = new Point(23, 60), Size = new Size(900, 45), BorderStyle = BorderStyle.FixedSingle };
            _txtSearch.Location = new Point(10, 10);
            _txtSearch.Width = 260;
            _txtSearch.TextChanged += (_, _) => LoadData();
            var btnRefresh = UiTheme.MakeOutlineButton("Làm mới");
            btnRefresh.Location = new Point(280, 8);
            btnRefresh.Click += (_, _) => { _txtSearch.Text = ""; LoadData(); };
            filterPanel.Controls.Add(_txtSearch);
            filterPanel.Controls.Add(btnRefresh);

            var gridHost = new Panel { Location = new Point(23, 115), Size = new Size(900, 420), BorderStyle = BorderStyle.FixedSingle };
            gridHost.Controls.Add(_grid);
            _grid.CellClick += Grid_CellClick;

            Controls.AddRange(new Control[] { title, filterPanel, gridHost });
            LoadData();
        }

        private void LoadData()
        {
            var term = "HK1 2026-2027";
            var sections = _enrollmentService.GetOpenSections(term)
                .Where(s => string.IsNullOrWhiteSpace(_txtSearch.Text) ||
                            s.CourseCode.Contains(_txtSearch.Text, StringComparison.OrdinalIgnoreCase) ||
                            (_enrollmentService.GetCourse(s.CourseCode)?.CourseName.Contains(_txtSearch.Text, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();

            var rows = sections.Select(s =>
            {
                var course = _enrollmentService.GetCourse(s.CourseCode);
                var lecturer = _enrollmentService.GetLecturer(s.LecturerId);
                string status = s.Remaining <= 0 ? "Đã đầy" : s.Remaining <= 5 ? "Gần đầy" : "Đang mở";
                return new
                {
                    s.CourseCode,
                    TenHocPhan = course?.CourseName,
                    s.SectionId,
                    course?.Credits,
                    GiangVien = lecturer?.FullName,
                    LichHoc = $"{s.DayLabel} · {s.TimeLabel}",
                    s.RoomId,
                    SiSo = $"{s.Enrolled}/{s.Capacity}",
                    TrangThai = status,
                    ThaoTac = s.Remaining > 0 ? "Đăng ký" : "Không khả dụng"
                };
            }).ToList();

            _grid.DataSource = rows;
            if (_grid.Columns.Count > 0)
            {
                _grid.Columns["CourseCode"].HeaderText = "Mã HP";
                _grid.Columns["TenHocPhan"].HeaderText = "Tên học phần";
                _grid.Columns["SectionId"].HeaderText = "Lớp";
                _grid.Columns["Credits"].HeaderText = "TC";
                _grid.Columns["GiangVien"].HeaderText = "Giảng viên";
                _grid.Columns["LichHoc"].HeaderText = "Lịch học";
                _grid.Columns["RoomId"].HeaderText = "Phòng";
                _grid.Columns["SiSo"].HeaderText = "Sĩ số";
                _grid.Columns["TrangThai"].HeaderText = "Trạng thái";
                _grid.Columns["ThaoTac"].HeaderText = "Thao tác";
            }
        }

        private void Grid_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || _grid.Columns[e.ColumnIndex].Name != "ThaoTac") return;

            var sectionId = _grid.Rows[e.RowIndex].Cells["SectionId"].Value?.ToString();
            var section = _enrollmentService.GetSection(sectionId ?? "");
            if (section is null || section.Remaining <= 0) return;

            using var dlg = new FrmConfirmRegistration(section);
            dlg.ShowDialog();
            if (dlg.RegisteredSuccessfully) LoadData();
        }
    }
}
