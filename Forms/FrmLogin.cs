using EduPath.WinForms.Common;
using EduPath.WinForms.Forms.Admin;
using EduPath.WinForms.Forms.Student;
using EduPath.WinForms.Models;
using EduPath.WinForms.Services;

namespace EduPath.WinForms.Forms
{
    /// <summary>Màn 01 · Đăng nhập. Gọi AuthService để xác thực, không tự chứa logic nghiệp vụ.</summary>
    public class FrmLogin : Form
    {
        private readonly TextBox _txtUsername = new() { PlaceholderText = "Tên đăng nhập / Mã sinh viên" };
        private readonly TextBox _txtPassword = new() { PlaceholderText = "Mật khẩu", UseSystemPasswordChar = true };
        private readonly ComboBox _cboRole = new() { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly Label _lblError = new() { ForeColor = Color.Firebrick, AutoSize = true, Visible = false };
        private readonly AuthService _authService = new();

        public FrmLogin()
        {
            Text = "EDUPATH — Đăng nhập";
            Width = 900;
            Height = 620;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = UiTheme.Background;
            Font = UiTheme.FontBase;

            var split = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44));
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 56));

            split.Controls.Add(BuildLeftPanel(), 0, 0);
            split.Controls.Add(BuildRightPanel(), 1, 0);
            Controls.Add(split);
        }

        private Control BuildLeftPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = UiTheme.Navy, Padding = new Padding(48, 65, 48, 0) };
            var brand = new Label { Text = "EDUPATH", ForeColor = Color.White, Font = new Font("Segoe UI", 20F, FontStyle.Bold), AutoSize = true, Location = new Point(48, 40) };
            var title = new Label { Text = "Đăng ký học phần\nđại học", ForeColor = Color.White, Font = new Font("Segoe UI", 22F, FontStyle.Bold), AutoSize = true, Location = new Point(48, 90) };
            var desc = new Label
            {
                Text = "Phần mềm quản lý nghiệp vụ dành cho sinh viên, giảng viên và quản trị viên.",
                ForeColor = ColorTranslator.FromHtml("#b8c9dc"),
                Font = UiTheme.FontBase,
                MaximumSize = new Size(320, 0),
                AutoSize = true,
                Location = new Point(48, 175)
            };
            var note = new Label
            {
                Text = "HK1 2026–2027 · Đợt đăng ký đang mở đến 30/08/2026",
                ForeColor = UiTheme.Navy,
                BackColor = ColorTranslator.FromHtml("#fff7df"),
                Font = new Font("Segoe UI", 8.5F),
                AutoSize = true,
                Padding = new Padding(9),
                Location = new Point(48, 240)
            };
            panel.Controls.AddRange(new Control[] { brand, title, desc, note });
            return panel;
        }

        private Control BuildRightPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = UiTheme.Background, Padding = new Padding(70, 90, 70, 0) };

            var lblSub = new Label { Text = "HỆ THỐNG QUẢN LÝ ĐÀO TẠO", ForeColor = UiTheme.TextMuted, Font = new Font("Segoe UI", 8F), AutoSize = true, Location = new Point(0, 0) };
            var lblTitle = new Label { Text = "Đăng nhập", Font = new Font("Segoe UI", 18F, FontStyle.Bold), AutoSize = true, Location = new Point(0, 18) };

            _cboRole.Items.AddRange(new object[] { "Sinh viên", "Quản trị viên" });
            _cboRole.SelectedIndex = 0;

            _txtUsername.Width = 320; _txtUsername.Location = new Point(0, 70);
            _txtPassword.Width = 320; _txtPassword.Location = new Point(0, 105);
            _cboRole.Width = 320; _cboRole.Location = new Point(0, 140);
            _lblError.Location = new Point(0, 172);

            var btnLogin = UiTheme.MakeYellowButton("Đăng nhập");
            btnLogin.Width = 320;
            btnLogin.Location = new Point(0, 200);
            btnLogin.Click += BtnLogin_Click;

            var lblForgot = new Label { Text = "Quên mật khẩu? Liên hệ Phòng Đào tạo.", ForeColor = UiTheme.TextMuted, Font = new Font("Segoe UI", 8F), AutoSize = true, Location = new Point(0, 245) };

            var demoHint = new Label
            {
                Text = "Demo: SV20260018 / 123456 (Sinh viên)  ·  AD0001 / admin123 (Quản trị viên)",
                ForeColor = UiTheme.TextMuted,
                Font = new Font("Segoe UI", 7.5F, FontStyle.Italic),
                AutoSize = true,
                Location = new Point(0, 270)
            };

            panel.Controls.AddRange(new Control[] { lblSub, lblTitle, _txtUsername, _txtPassword, _cboRole, _lblError, btnLogin, lblForgot, demoHint });
            AcceptButton = btnLogin;
            return panel;
        }

        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            var role = _cboRole.SelectedIndex == 1 ? Role.Admin : Role.Student;
            var result = _authService.Login(_txtUsername.Text.Trim(), _txtPassword.Text, role);

            if (!result.Success)
            {
                _lblError.Text = result.ErrorMessage;
                _lblError.Visible = true;
                return;
            }

            SessionContext.SignIn(result.Account!, result.Student);

            Form target = role == Role.Admin ? AdminShellBuilder.Build() : StudentShellBuilder.Build();
            target.Show();
            Hide();
            target.FormClosed += (_, _) => Close();
        }
    }
}
