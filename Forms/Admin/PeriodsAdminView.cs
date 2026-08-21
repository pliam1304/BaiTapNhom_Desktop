using EduPath.WinForms.Common;    // GridHelper, UiTheme
using EduPath.WinForms.Services;  // RegistrationPeriodService: đã có sẵn Open/Close, vừa bổ sung Create

namespace EduPath.WinForms.Forms.Admin
{
    /// <summary>
    /// Màn "Học kỳ & Đợt đăng ký" — cùng mẫu SectionsAdminView, thêm 2 nút Mở/Đóng đợt vì
    /// nghiệp vụ đặc thù (chỉ được có 1 đợt mở tại một thời điểm — ràng buộc đã có sẵn trong Service).
    /// </summary>
    public class PeriodsAdminView : UserControl
    {
        private readonly RegistrationPeriodService _periodService = new(); // Service nghiệp vụ đợt đăng ký
        private readonly DataGridView _grid = GridHelper.MakeGrid();        // Grid style đồng bộ
        private readonly Label _lblMessage = new() { ForeColor = Color.Firebrick, AutoSize = true, Location = new Point(23, 545) };

        public PeriodsAdminView()
        {
            Dock = DockStyle.Fill;
            BackColor = UiTheme.Background;
            Padding = new Padding(23, 20, 23, 20);

            var title = new Label { Text = "Học kỳ & Đợt đăng ký", Font = UiTheme.FontHeading, AutoSize = true, Location = new Point(23, 20) };

            // Nút tạo đợt đăng ký mới
            var btnCreate = UiTheme.MakeYellowButton("+ Tạo đợt đăng ký");
            btnCreate.Location = new Point(600, 22);
            btnCreate.Click += (_, _) =>
            {
                using var dlg = new FrmCreatePeriod();
                dlg.ShowDialog();
                if (dlg.Created) LoadData();
            };

            // Nút mở đợt đăng ký đang chọn
            var btnOpen = UiTheme.MakeOutlineButton("Mở đợt đã chọn");
            btnOpen.Location = new Point(780, 22);
            btnOpen.Click += (_, _) => ToggleSelected(open: true);

            // Nút đóng đợt đăng ký đang chọn
            var btnClose = UiTheme.MakeOutlineButton("Đóng đợt đã chọn");
            btnClose.Location = new Point(900, 22);
            btnClose.Click += (_, _) => ToggleSelected(open: false);

            var gridHost = new Panel { Location = new Point(23, 60), Size = new Size(1020, 480), BorderStyle = BorderStyle.FixedSingle };
            gridHost.Controls.Add(_grid);

            Controls.AddRange(new Control[] { title, btnCreate, btnOpen, btnClose, gridHost, _lblMessage });
            LoadData();
        }

        private void LoadData()
        {
            // Map từng RegistrationPeriod -> object ẩn danh để đặt tên cột tiếng Việt
            var rows = _periodService.GetAll().Select(p => new
            {
                p.Name,                                              // Tên đợt (khóa chính để Open/Close)
                p.Term,                                               // Học kỳ áp dụng
                BatDau = p.StartDate.ToString("dd/MM/yyyy"),           // Ngày bắt đầu, định dạng VN
                KetThuc = p.EndDate.ToString("dd/MM/yyyy"),            // Ngày kết thúc
                TinChi = $"{p.MinCredits}-{p.MaxCredits}",             // Khoảng tín chỉ tối thiểu-tối đa
                TrangThai = p.IsOpen ? "Đang mở" : "Đã đóng"           // Trạng thái hiện tại
            }).ToList();

            _grid.DataSource = rows;
            if (_grid.Columns.Count > 0)
            {
                _grid.Columns["Name"].HeaderText = "Tên đợt";
                _grid.Columns["Term"].HeaderText = "Học kỳ";
                _grid.Columns["BatDau"].HeaderText = "Bắt đầu";
                _grid.Columns["KetThuc"].HeaderText = "Kết thúc";
                _grid.Columns["TinChi"].HeaderText = "TC tối thiểu-tối đa";
                _grid.Columns["TrangThai"].HeaderText = "Trạng thái";
            }
        }

        /// <summary>Mở hoặc đóng đợt đăng ký đang được chọn trên grid.</summary>
        private void ToggleSelected(bool open)
        {
            _lblMessage.ForeColor = Color.Firebrick; // Mặc định màu báo lỗi

            if (_grid.CurrentRow is null) // Chưa chọn dòng nào trên grid
            {
                _lblMessage.Text = "Vui lòng chọn một đợt đăng ký trong danh sách trước.";
                return;
            }

            var name = _grid.CurrentRow.Cells["Name"].Value?.ToString(); // Lấy tên đợt của dòng chọn
            if (string.IsNullOrEmpty(name)) return;

            // Gọi Open hoặc Close tùy tham số truyền vào — ràng buộc "chỉ 1 đợt mở" đã nằm sẵn trong Open()
            var (ok, error) = open ? _periodService.Open(name) : _periodService.Close(name);
            if (!ok)
            {
                _lblMessage.Text = error; // Hiển thị lỗi (vd "đã có đợt khác đang mở")
                return;
            }

            _lblMessage.ForeColor = UiTheme.BadgeGreenText;
            _lblMessage.Text = $"Đã {(open ? "mở" : "đóng")} đợt đăng ký '{name}'.";
            LoadData(); // Nạp lại grid để phản ánh trạng thái mới
        }
    }
}
