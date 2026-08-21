using EduPath.WinForms.Common;    // GridHelper, UiTheme dùng chung cho mọi màn quản trị
using EduPath.WinForms.Services;  // CourseService chứa toàn bộ nghiệp vụ học phần

namespace EduPath.WinForms.Forms.Admin
{
    /// <summary>
    /// Màn "Quản lý học phần" — dựng đúng theo mẫu SectionsAdminView:
    /// (1) DataGridView nạp từ CourseService, (2) nút "+ Thêm học phần" mở Form modal,
    /// (3) nút "Vô hiệu hóa" thao tác trên dòng đang chọn, lỗi nghiệp vụ hiển thị ngay tại chỗ.
    /// </summary>
    public class CoursesAdminView : UserControl
    {
        // Service xử lý nghiệp vụ — View KHÔNG tự chứa logic, chỉ gọi Service rồi hiển thị kết quả
        private readonly CourseService _courseService = new();
        // Grid dùng chung style toàn app (viền, màu header...) lấy từ GridHelper
        private readonly DataGridView _grid = GridHelper.MakeGrid();
        // Label hiển thị lỗi nghiệp vụ (vd "vô hiệu hóa thất bại vì đang có lớp mở")
        private readonly Label _lblMessage = new() { ForeColor = Color.Firebrick, AutoSize = true, Location = new Point(23, 545) };

        public CoursesAdminView()
        {
            Dock = DockStyle.Fill;                 // Chiếm toàn bộ vùng nội dung của shell
            BackColor = UiTheme.Background;        // Màu nền chuẩn theo theme
            Padding = new Padding(23, 20, 23, 20);  // Khoảng đệm giống các view khác

            // Tiêu đề màn hình
            var title = new Label { Text = "Quản lý học phần", Font = UiTheme.FontHeading, AutoSize = true, Location = new Point(23, 20) };

            // Nút mở form tạo học phần mới
            var btnCreate = UiTheme.MakeYellowButton("+ Thêm học phần");
            btnCreate.Location = new Point(650, 22); // Đặt góc phải trên, cùng vị trí như SectionsAdminView
            btnCreate.Click += (_, _) =>
            {
                using var dlg = new FrmCreateCourse(); // Mở modal tạo học phần (using -> tự Dispose sau khi đóng)
                dlg.ShowDialog();                       // Hiện modal, chặn tương tác cho tới khi đóng
                if (dlg.Created) LoadData();             // Nếu tạo thành công thì nạp lại grid
            };

            // Nút vô hiệu hóa học phần đang chọn trên grid
            var btnDeactivate = UiTheme.MakeOutlineButton("Vô hiệu hóa học phần đã chọn");
            btnDeactivate.Location = new Point(870, 22); // Đặt cạnh nút Thêm
            btnDeactivate.Click += BtnDeactivate_Click;

            // Panel bọc quanh grid để có viền (BorderStyle) giống các view khác
            var gridHost = new Panel { Location = new Point(23, 60), Size = new Size(900, 480), BorderStyle = BorderStyle.FixedSingle };
            gridHost.Controls.Add(_grid); // Grid Dock = Fill nên sẽ tự lấp đầy panel này

            // Gắn toàn bộ control con vào view
            Controls.AddRange(new Control[] { title, btnCreate, btnDeactivate, gridHost, _lblMessage });
            LoadData(); // Nạp dữ liệu ngay khi view được khởi tạo
        }

        /// <summary>Nạp lại toàn bộ danh sách học phần từ Service vào grid.</summary>
        private void LoadData()
        {
            // Chuyển từng Course thành 1 object ẩn danh (anonymous type) để đặt tên cột tiếng Việt dễ hơn
            var rows = _courseService.GetAll().Select(c => new
            {
                c.CourseCode,                                  // Mã học phần
                TenHocPhan = c.CourseName,                      // Tên học phần
                c.Credits,                                      // Số tín chỉ
                c.Faculty,                                      // Khoa phụ trách
                TienQuyet = c.PrerequisiteCode ?? "(không có)", // Học phần tiên quyết, hiển thị "(không có)" nếu null
                TrangThai = c.IsActive ? "Đang mở" : "Đã vô hiệu hóa" // Trạng thái hoạt động
            }).ToList();

            _grid.DataSource = rows; // Gán nguồn dữ liệu -> DataGridView tự sinh cột theo property

            // Đổi header sang tiếng Việt cho từng cột (chỉ chạy khi grid đã có cột, tránh lỗi lần đầu)
            if (_grid.Columns.Count > 0)
            {
                _grid.Columns["CourseCode"].HeaderText = "Mã học phần";
                _grid.Columns["TenHocPhan"].HeaderText = "Tên học phần";
                _grid.Columns["Credits"].HeaderText = "Số TC";
                _grid.Columns["Faculty"].HeaderText = "Khoa";
                _grid.Columns["TienQuyet"].HeaderText = "Tiên quyết";
                _grid.Columns["TrangThai"].HeaderText = "Trạng thái";
            }
        }

        /// <summary>Xử lý khi bấm nút "Vô hiệu hóa học phần đã chọn".</summary>
        private void BtnDeactivate_Click(object? sender, EventArgs e)
        {
            _lblMessage.ForeColor = Color.Firebrick; // Mặc định màu lỗi (đỏ), đổi sang xanh nếu thành công

            // Kiểm tra người dùng đã chọn dòng nào trên grid chưa
            if (_grid.CurrentRow is null)
            {
                _lblMessage.Text = "Vui lòng chọn một học phần trong danh sách trước.";
                return; // Không có dòng nào chọn -> dừng lại, không gọi Service
            }

            // Lấy mã học phần từ ô "CourseCode" của dòng đang chọn
            var courseCode = _grid.CurrentRow.Cells["CourseCode"].Value?.ToString();
            if (string.IsNullOrEmpty(courseCode)) return; // Phòng thủ: nếu vì lý do gì đó ô rỗng thì bỏ qua

            // Gọi Service thực hiện nghiệp vụ vô hiệu hóa (đã có sẵn kiểm tra "đang có lớp mở" bên trong)
            var (ok, error) = _courseService.Deactivate(courseCode);
            if (!ok)
            {
                _lblMessage.Text = error; // Hiển thị lý do thất bại ngay tại chỗ, không tự ý xử lý ở UI
                return;
            }

            _lblMessage.ForeColor = UiTheme.BadgeGreenText;               // Đổi màu thông báo sang xanh (thành công)
            _lblMessage.Text = $"Đã vô hiệu hóa học phần {courseCode}.";  // Thông báo kết quả
            LoadData();                                                   // Nạp lại grid để cập nhật trạng thái mới
        }
    }
}
