using EduPath.WinForms.Common;    // UiTheme
using EduPath.WinForms.Data;      // InMemoryStore để nạp danh sách Student/Lecturer cho dropdown liên kết
using EduPath.WinForms.Models;    // Model Account, Role
using EduPath.WinForms.Services;  // AccountService xử lý nghiệp vụ

namespace EduPath.WinForms.Forms.Admin
{
    /// <summary>
    /// Modal "+ Tạo tài khoản" — dùng cho Admin/Lecturer/Student độc lập với hồ sơ nghiệp vụ.
    /// Lưu ý: nếu chỉ cần tạo tài khoản Sinh viên đi kèm hồ sơ mới, nên dùng FrmCreateStudent
    /// (tạo cả Student lẫn Account cùng lúc) thay vì form này.
    /// </summary>
    public class FrmCreateAccount : Form
    {
        private readonly TextBox _txtUsername = new() { Width = 260 };  // Tên đăng nhập
        private readonly TextBox _txtPassword = new() { Width = 260, UseSystemPasswordChar = true }; // Mật khẩu khởi tạo
        private readonly ComboBox _cboRole = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 260 }; // Vai trò
        private readonly ComboBox _cboLinked = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 260 }; // Liên kết Student/Lecturer
        private readonly Label _lblError = new() { ForeColor = Color.Firebrick, AutoSize = true, MaximumSize = new Size(300, 0) };
        private readonly AccountService _accountService = new(); // Service xử lý nghiệp vụ

        // "new" để che có chủ đích thuộc tính Created sẵn có của Control (tránh warning CS0108)
        public new bool Created { get; private set; } // Cờ báo tạo thành công cho view cha

        public FrmCreateAccount()
        {
            Text = "Tạo tài khoản";
            Width = 340; Height = 360;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false; MinimizeBox = false;
            Font = UiTheme.FontBase;
            Padding = new Padding(15);

            // Danh sách vai trò lấy trực tiếp từ enum Role — tránh gõ tay dễ sai chính tả
            _cboRole.DataSource = Enum.GetValues(typeof(Role));
            _cboRole.SelectedIndexChanged += (_, _) => ReloadLinkedOptions(); // Đổi vai trò -> nạp lại danh sách liên kết tương ứng

            var layout = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 2, AutoSize = true };
            AddRow(layout, "Tên đăng nhập:", _txtUsername);
            AddRow(layout, "Mật khẩu khởi tạo:", _txtPassword);
            AddRow(layout, "Vai trò:", _cboRole);
            AddRow(layout, "Liên kết:", _cboLinked);

            var btnSave = UiTheme.MakeYellowButton("Tạo tài khoản");
            btnSave.Click += BtnSave_Click;
            _lblError.Location = new Point(15, 250); // Dưới layout 4 hàng
            btnSave.Location = new Point(15, 290);

            Controls.Add(layout);
            Controls.Add(_lblError);
            Controls.Add(btnSave);

            ReloadLinkedOptions(); // Nạp lần đầu theo vai trò mặc định (Student, do enum khai báo Student trước tiên)
        }

        /// <summary>Nạp lại dropdown "Liên kết" theo vai trò đang chọn — Admin thì không cần liên kết.</summary>
        private void ReloadLinkedOptions()
        {
            var store = InMemoryStore.Instance; // Nguồn dữ liệu Student/Lecturer hiện có
            var role = (Role)_cboRole.SelectedItem!; // Vai trò đang được chọn trên combo

            if (role == Role.Student)
                _cboLinked.DataSource = store.Students.Select(s => s.StudentId).ToList(); // Chọn MSSV để liên kết
            else if (role == Role.Lecturer)
                _cboLinked.DataSource = store.Lecturers.Select(l => l.LecturerId).ToList(); // Chọn mã GV để liên kết
            else
                _cboLinked.DataSource = new List<string> { "(không cần)" }; // Admin không cần liên kết tới hồ sơ nào

            _cboLinked.Enabled = role != Role.Admin; // Khóa combo khi là Admin để tránh chọn nhầm
        }

        private static void AddRow(TableLayoutPanel layout, string label, Control input)
        {
            layout.RowCount++;
            layout.Controls.Add(new Label { Text = label, AutoSize = true, Margin = new Padding(0, 8, 8, 0) });
            layout.Controls.Add(input);
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            var role = (Role)_cboRole.SelectedItem!; // Vai trò được chọn
            // Nếu là Admin thì LinkedId = null; ngược lại lấy giá trị đang chọn trong dropdown liên kết
            string? linkedId = role == Role.Admin ? null : _cboLinked.SelectedItem?.ToString();

            var account = new Account
            {
                Username = _txtUsername.Text.Trim(),
                PasswordHash = _txtPassword.Text, // Demo: lưu plain text theo đúng cách AuthService đang so khớp
                Role = role,
                LinkedId = linkedId,
                IsActive = true // Tài khoản mới mặc định đang hoạt động
            };

            var (ok, error) = _accountService.Add(account); // Toàn bộ validate nằm trong Service
            if (!ok)
            {
                _lblError.Text = error;
                return;
            }

            Created = true;
            Close();
        }
    }
}
