using EduPath.WinForms.Common;
using EduPath.WinForms.Services;

namespace EduPath.WinForms.Forms.Student
{
    /// <summary>Màn 06 · Học phần đã đăng ký, có nút Hủy đăng ký gọi EnrollmentService.Cancel.</summary>
    public class EnrolledSectionsView : UserControl
    {
        private readonly EnrollmentService _enrollmentService = new();
        private readonly DataGridView _grid = GridHelper.MakeGrid();
        private readonly FlowLayoutPanel _kpiRow = new() { AutoSize = true, FlowDirection = FlowDirection.LeftToRight };

        public EnrolledSectionsView()
        {
            Dock = DockStyle.Fill;
            BackColor = UiTheme.Background;
            Padding = new Padding(23, 20, 23, 20);

            var title = new Label { Text = "Học phần đã đăng ký", Font = UiTheme.FontHeading, AutoSize = true, Location = new Point(23, 20) };

            var gridHost = new Panel { Location = new Point(23, 60), Size = new Size(900, 350), BorderStyle = BorderStyle.FixedSingle };
            gridHost.Controls.Add(_grid);
            _grid.CellClick += Grid_CellClick;

            _kpiRow.Location = new Point(23, 425);

            Controls.AddRange(new Control[] { title, gridHost, _kpiRow });
            LoadData();
        }

        private void LoadData()
        {
            var student = SessionContext.CurrentStudent!;
            var enrollments = _enrollmentService.GetActiveEnrollments(student.StudentId).ToList();

            var rows = enrollments.Select(e =>
            {
                var section = _enrollmentService.GetSection(e.SectionId)!;
                var course = _enrollmentService.GetCourse(section.CourseCode);
                var lecturer = _enrollmentService.GetLecturer(section.LecturerId);
                return new
                {
                    section.SectionId,
                    HocPhan = course?.CourseName,
                    course?.Credits,
                    GiangVien = lecturer?.FullName,
                    Lich = $"{section.DayLabel} {section.TimeLabel}",
                    section.RoomId,
                    TrangThai = "Đã đăng ký",
                    ThaoTac = "Hủy đăng ký"
                };
            }).ToList();

            _grid.DataSource = rows;
            if (_grid.Columns.Count > 0)
            {
                _grid.Columns["SectionId"].HeaderText = "Mã lớp";
                _grid.Columns["HocPhan"].HeaderText = "Học phần";
                _grid.Columns["Credits"].HeaderText = "TC";
                _grid.Columns["GiangVien"].HeaderText = "Giảng viên";
                _grid.Columns["Lich"].HeaderText = "Lịch";
                _grid.Columns["RoomId"].HeaderText = "Phòng";
                _grid.Columns["TrangThai"].HeaderText = "Trạng thái";
                _grid.Columns["ThaoTac"].HeaderText = "Thao tác";
            }

            int totalCredits = enrollments.Sum(e => _enrollmentService.GetCourse(_enrollmentService.GetSection(e.SectionId)!.CourseCode)?.Credits ?? 0);
            _kpiRow.Controls.Clear();
            var c1 = GridHelper.MakeKpiCard("TỔNG SỐ LỚP", enrollments.Count.ToString());
            var c2 = GridHelper.MakeKpiCard("TỔNG SỐ TÍN CHỈ", totalCredits.ToString(), UiTheme.Gold);
            c1.Margin = new Padding(0, 0, 12, 0);
            _kpiRow.Controls.Add(c1);
            _kpiRow.Controls.Add(c2);
        }

        private void Grid_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || _grid.Columns[e.ColumnIndex].Name != "ThaoTac") return;

            var sectionId = _grid.Rows[e.RowIndex].Cells["SectionId"].Value?.ToString();
            if (sectionId is null) return;

            var confirm = MessageBox.Show($"Bạn có chắc muốn hủy đăng ký lớp {sectionId}?", "Xác nhận hủy",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            var student = SessionContext.CurrentStudent!;
            var (ok, error) = _enrollmentService.Cancel(student.StudentId, sectionId);
            if (!ok)
                MessageBox.Show(error, "Không thể hủy", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            else
                LoadData();
        }
    }
}
