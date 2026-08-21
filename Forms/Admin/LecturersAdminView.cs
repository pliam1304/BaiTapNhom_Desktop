using EduPath.WinForms.Common;    // GridHelper, UiTheme
using EduPath.WinForms.Services;  // LecturerAdminService xử lý nghiệp vụ

namespace EduPath.WinForms.Forms.Admin
{
    /// <summary>Màn "Quản lý giảng viên" — cùng mẫu SectionsAdminView.</summary>
    public class LecturersAdminView : UserControl
    {
        private readonly LecturerAdminService _lecturerService = new(); // Service nghiệp vụ giảng viên
        private readonly DataGridView _grid = GridHelper.MakeGrid();     // Grid style đồng bộ
        private readonly Label _lblMessage = new() { ForeColor = Color.Firebrick, AutoSize = true, Location = new Point(23, 545) };

        public LecturersAdminView()
        {
            Dock = DockStyle.Fill;
            BackColor = UiTheme.Background;
            Padding = new Padding(23, 20, 23, 20);

            var title = new Label { Text = "Quản lý giảng viên", Font = UiTheme.FontHeading, AutoSize = true, Location = new Point(23, 20) };

            var btnCreate = UiTheme.MakeYellowButton("+ Thêm giảng viên");
            btnCreate.Location = new Point(650, 22);
            btnCreate.Click += (_, _) =>
            {
                using var dlg = new FrmCreateLecturer(); // Modal tạo giảng viên mới
                dlg.ShowDialog();
                if (dlg.Created) LoadData();
            };

            var btnToggle = UiTheme.MakeOutlineButton("Khóa/Mở giảng viên đã chọn");
            btnToggle.Location = new Point(870, 22);
            btnToggle.Click += BtnToggle_Click;

            var gridHost = new Panel { Location = new Point(23, 60), Size = new Size(900, 480), BorderStyle = BorderStyle.FixedSingle };
            gridHost.Controls.Add(_grid);

            Controls.AddRange(new Control[] { title, btnCreate, btnToggle, gridHost, _lblMessage });
            LoadData();
        }

        private void LoadData()
        {
            var rows = _lecturerService.GetAll().Select(l => new
            {
                l.LecturerId,                                     // Mã giảng viên
                HoTen = l.FullName,                                // Họ tên
                l.Email,                                           // Email
                l.Department,                                      // Khoa/Bộ môn
                TrangThai = l.IsActive ? "Đang công tác" : "Đã khóa" // Trạng thái hoạt động
            }).ToList();

            _grid.DataSource = rows;
            if (_grid.Columns.Count > 0)
            {
                _grid.Columns["LecturerId"].HeaderText = "Mã GV";
                _grid.Columns["HoTen"].HeaderText = "Họ tên";
                _grid.Columns["Email"].HeaderText = "Email";
                _grid.Columns["Department"].HeaderText = "Khoa/Bộ môn";
                _grid.Columns["TrangThai"].HeaderText = "Trạng thái";
            }
        }

        private void BtnToggle_Click(object? sender, EventArgs e)
        {
            _lblMessage.ForeColor = Color.Firebrick;

            if (_grid.CurrentRow is null)
            {
                _lblMessage.Text = "Vui lòng chọn một giảng viên trong danh sách trước.";
                return;
            }

            var lecturerId = _grid.CurrentRow.Cells["LecturerId"].Value?.ToString(); // MSGV của dòng chọn
            if (string.IsNullOrEmpty(lecturerId)) return;

            var lecturer = _lecturerService.GetById(lecturerId); // Lấy trạng thái hiện tại để quyết định đảo chiều
            if (lecturer is null) return;

            var (ok, error) = lecturer.IsActive
                ? _lecturerService.Deactivate(lecturerId)  // Đang hoạt động -> khóa (có kiểm tra đang dạy lớp mở)
                : _lecturerService.Activate(lecturerId);   // Đang khóa -> mở lại

            if (!ok)
            {
                _lblMessage.Text = error;
                return;
            }

            _lblMessage.ForeColor = UiTheme.BadgeGreenText;
            _lblMessage.Text = $"Đã cập nhật trạng thái giảng viên {lecturerId}.";
            LoadData();
        }
    }
}
