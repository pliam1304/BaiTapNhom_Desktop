using EduPath.WinForms.Common;    // GridHelper, UiTheme dùng chung
using EduPath.WinForms.Services;  // StudentAdminService xử lý nghiệp vụ hồ sơ sinh viên

namespace EduPath.WinForms.Forms.Admin
{
    /// <summary>Màn "Quản lý sinh viên" — cùng mẫu SectionsAdminView: grid + nút thêm + nút khóa/mở.</summary>
    public class StudentsAdminView : UserControl
    {
        private readonly StudentAdminService _studentService = new(); // Service nghiệp vụ hồ sơ sinh viên
        private readonly DataGridView _grid = GridHelper.MakeGrid();   // Grid style đồng bộ toàn app
        private readonly Label _lblMessage = new() { ForeColor = Color.Firebrick, AutoSize = true, Location = new Point(23, 545) };

        public StudentsAdminView()
        {
            Dock = DockStyle.Fill;
            BackColor = UiTheme.Background;
            Padding = new Padding(23, 20, 23, 20);

            var title = new Label { Text = "Quản lý sinh viên", Font = UiTheme.FontHeading, AutoSize = true, Location = new Point(23, 20) };

            // Nút mở modal tạo hồ sơ sinh viên mới
            var btnCreate = UiTheme.MakeYellowButton("+ Thêm sinh viên");
            btnCreate.Location = new Point(650, 22);
            btnCreate.Click += (_, _) =>
            {
                using var dlg = new FrmCreateStudent(); // Modal tạo mới
                dlg.ShowDialog();
                if (dlg.Created) LoadData(); // Chỉ nạp lại grid nếu tạo thành công
            };

            // Nút khóa/mở học sinh viên đang chọn — dùng chung 1 nút, tự đảo trạng thái theo IsActive hiện tại
            var btnToggle = UiTheme.MakeOutlineButton("Khóa/Mở sinh viên đã chọn");
            btnToggle.Location = new Point(870, 22);
            btnToggle.Click += BtnToggle_Click;

            var gridHost = new Panel { Location = new Point(23, 60), Size = new Size(900, 480), BorderStyle = BorderStyle.FixedSingle };
            gridHost.Controls.Add(_grid);

            Controls.AddRange(new Control[] { title, btnCreate, btnToggle, gridHost, _lblMessage });
            LoadData();
        }

        private void LoadData()
        {
            // Map từng Student -> object ẩn danh để đặt tên cột tiếng Việt
            var rows = _studentService.GetAll().Select(s => new
            {
                s.StudentId,                                       // Mã sinh viên
                HoTen = s.FullName,                                 // Họ tên
                s.Email,                                            // Email liên hệ
                s.Faculty,                                          // Khoa
                Khoa_Hoc = s.IntakeYear,                            // Khóa nhập học (đặt tên khác Faculty để không trùng)
                Lop = s.ClassCode,                                  // Lớp hành chính
                SoTC = $"{s.MinCreditsPerTerm}-{s.MaxCreditsPerTerm}", // Khoảng tín chỉ tối thiểu-tối đa mỗi kỳ
                TrangThai = s.IsActive ? "Đang học" : "Đã khóa"     // Trạng thái hồ sơ
            }).ToList();

            _grid.DataSource = rows;
            if (_grid.Columns.Count > 0)
            {
                _grid.Columns["StudentId"].HeaderText = "MSSV";
                _grid.Columns["HoTen"].HeaderText = "Họ tên";
                _grid.Columns["Email"].HeaderText = "Email";
                _grid.Columns["Faculty"].HeaderText = "Khoa";
                _grid.Columns["Khoa_Hoc"].HeaderText = "Khóa";
                _grid.Columns["Lop"].HeaderText = "Lớp";
                _grid.Columns["SoTC"].HeaderText = "TC tối thiểu-tối đa";
                _grid.Columns["TrangThai"].HeaderText = "Trạng thái";
            }
        }

        /// <summary>Xử lý khóa/mở sinh viên đang chọn — đọc trạng thái hiện tại để quyết định gọi Activate hay Deactivate.</summary>
        private void BtnToggle_Click(object? sender, EventArgs e)
        {
            _lblMessage.ForeColor = Color.Firebrick; // Mặc định màu báo lỗi

            if (_grid.CurrentRow is null) // Chưa chọn dòng nào
            {
                _lblMessage.Text = "Vui lòng chọn một sinh viên trong danh sách trước.";
                return;
            }

            var studentId = _grid.CurrentRow.Cells["StudentId"].Value?.ToString(); // Lấy MSSV của dòng chọn
            if (string.IsNullOrEmpty(studentId)) return;

            var student = _studentService.GetById(studentId); // Tra lại đối tượng gốc để biết IsActive hiện tại
            if (student is null) return;

            // Nếu đang hoạt động -> khóa; nếu đang khóa -> mở lại
            var (ok, error) = student.IsActive
                ? _studentService.Deactivate(studentId)
                : _studentService.Activate(studentId);

            if (!ok)
            {
                _lblMessage.Text = error; // Hiển thị lỗi nghiệp vụ nếu có
                return;
            }

            _lblMessage.ForeColor = UiTheme.BadgeGreenText;
            _lblMessage.Text = $"Đã cập nhật trạng thái sinh viên {studentId}.";
            LoadData(); // Nạp lại grid để phản ánh trạng thái mới
        }
    }
}
