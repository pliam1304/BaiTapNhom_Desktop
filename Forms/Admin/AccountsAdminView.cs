using EduPath.WinForms.Common;    // GridHelper, UiTheme
using EduPath.WinForms.Services;  // AccountService xử lý nghiệp vụ tài khoản

namespace EduPath.WinForms.Forms.Admin
{
    /// <summary>Màn "Tài khoản & Phân quyền" — cùng mẫu SectionsAdminView, thêm nút khóa/mở tài khoản.</summary>
    public class AccountsAdminView : UserControl
    {
        private readonly AccountService _accountService = new();   // Service nghiệp vụ tài khoản
        private readonly DataGridView _grid = GridHelper.MakeGrid(); // Grid style đồng bộ
        private readonly Label _lblMessage = new() { ForeColor = Color.Firebrick, AutoSize = true, Location = new Point(23, 545) };

        public AccountsAdminView()
        {
            Dock = DockStyle.Fill;
            BackColor = UiTheme.Background;
            Padding = new Padding(23, 20, 23, 20);

            var title = new Label { Text = "Tài khoản & Phân quyền", Font = UiTheme.FontHeading, AutoSize = true, Location = new Point(23, 20) };

            var btnCreate = UiTheme.MakeYellowButton("+ Tạo tài khoản");
            btnCreate.Location = new Point(650, 22);
            btnCreate.Click += (_, _) =>
            {
                using var dlg = new FrmCreateAccount(); // Modal tạo tài khoản mới (Admin/Lecturer/Student)
                dlg.ShowDialog();
                if (dlg.Created) LoadData();
            };

            var btnToggle = UiTheme.MakeOutlineButton("Khóa/Mở tài khoản đã chọn");
            btnToggle.Location = new Point(870, 22);
            btnToggle.Click += BtnToggle_Click;

            var gridHost = new Panel { Location = new Point(23, 60), Size = new Size(900, 480), BorderStyle = BorderStyle.FixedSingle };
            gridHost.Controls.Add(_grid);

            Controls.AddRange(new Control[] { title, btnCreate, btnToggle, gridHost, _lblMessage });
            LoadData();
        }

        private void LoadData()
        {
            var rows = _accountService.GetAll().Select(a => new
            {
                a.Username,                                          // Tên đăng nhập
                VaiTro = a.Role.ToString(),                          // Vai trò (Student/Lecturer/Admin)
                LienKet = a.LinkedId ?? "(không có)",                // MSSV/Mã GV liên kết, "(không có)" nếu là Admin
                DangNhapCuoi = a.LastLoginAt?.ToString("dd/MM/yyyy HH:mm") ?? "Chưa đăng nhập", // Lần đăng nhập gần nhất
                TrangThai = a.IsActive ? "Đang hoạt động" : "Đã khóa" // Trạng thái tài khoản
            }).ToList();

            _grid.DataSource = rows;
            if (_grid.Columns.Count > 0)
            {
                _grid.Columns["Username"].HeaderText = "Tên đăng nhập";
                _grid.Columns["VaiTro"].HeaderText = "Vai trò";
                _grid.Columns["LienKet"].HeaderText = "Liên kết";
                _grid.Columns["DangNhapCuoi"].HeaderText = "Đăng nhập cuối";
                _grid.Columns["TrangThai"].HeaderText = "Trạng thái";
            }
        }

        private void BtnToggle_Click(object? sender, EventArgs e)
        {
            _lblMessage.ForeColor = Color.Firebrick;

            if (_grid.CurrentRow is null)
            {
                _lblMessage.Text = "Vui lòng chọn một tài khoản trong danh sách trước.";
                return;
            }

            var username = _grid.CurrentRow.Cells["Username"].Value?.ToString(); // Username của dòng chọn
            if (string.IsNullOrEmpty(username)) return;

            var account = _accountService.GetByUsername(username); // Lấy trạng thái hiện tại để đảo chiều
            if (account is null) return;

            var (ok, error) = account.IsActive
                ? _accountService.Lock(username)   // Đang hoạt động -> khóa
                : _accountService.Unlock(username); // Đang khóa -> mở lại

            if (!ok)
            {
                _lblMessage.Text = error;
                return;
            }

            _lblMessage.ForeColor = UiTheme.BadgeGreenText;
            _lblMessage.Text = $"Đã cập nhật trạng thái tài khoản {username}.";
            LoadData();
        }
    }
}
