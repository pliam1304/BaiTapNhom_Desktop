using EduPath.WinForms.Forms;
using EduPath.WinForms.Models;
using EduPath.WinForms.Services;

namespace EduPath.WinForms.Common
{
    /// <summary>
    /// Khung sườn dùng chung cho mọi màn hình sau đăng nhập (tương đương `.e-shell` trong mockup):
    /// SideMenu bên trái + header trên cùng + vùng nội dung chính đổi động theo điều hướng.
    /// Mỗi "màn hình" (open-sections, timetable, courses...) là một UserControl độc lập được
    /// đăng ký vào _viewFactories — thêm màn hình mới chỉ cần thêm một dòng ở Program.cs/RegisterView,
    /// không phải sửa MainShellForm.
    /// </summary>
    public class MainShellForm : Form
    {
        private readonly Panel _headerPanel = new();
        private readonly Label _crumbLabel = new();
        private readonly Label _userLabel = new();
        private readonly Panel _contentHost = new() { Dock = DockStyle.Fill, BackColor = UiTheme.Background, AutoScroll = true };
        private readonly SideMenu _sideMenu;

        private readonly Dictionary<string, Func<Control>> _viewFactories = new();
        private readonly Dictionary<string, string> _titles = new();

        public MainShellForm()
        {
            Text = "EDUPATH — Quản lý đăng ký học phần";
            Width = 1180;
            Height = 720;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = UiTheme.Background;
            Font = UiTheme.FontBase;

            var role = SessionContext.CurrentAccount!.Role;
            _sideMenu = new SideMenu(role) { Dock = DockStyle.Left };
            _sideMenu.NavigateRequested += OnNavigate;

            BuildHeader();

            var mainArea = new Panel { Dock = DockStyle.Fill };
            mainArea.Controls.Add(_contentHost);
            mainArea.Controls.Add(_headerPanel);

            Controls.Add(mainArea);
            Controls.Add(_sideMenu);
        }

        private void BuildHeader()
        {
            _headerPanel.Dock = DockStyle.Top;
            _headerPanel.Height = 58;
            _headerPanel.BackColor = Color.White;
            _headerPanel.Padding = new Padding(22, 0, 22, 0);

            _crumbLabel.Text = "";
            _crumbLabel.ForeColor = UiTheme.TextMuted;
            _crumbLabel.AutoSize = true;
            _crumbLabel.Location = new Point(22, 20);
            _headerPanel.Controls.Add(_crumbLabel);

            var role = SessionContext.CurrentAccount!.Role;
            var displayName = role == Role.Admin ? "AD0001 · Phòng Đào tạo" : $"{SessionContext.CurrentStudent?.StudentId} · {SessionContext.CurrentStudent?.FullName}";
            var roleLabel = role == Role.Admin ? "Quản trị viên" : "Sinh viên";
            _userLabel.Text = $"{displayName}   ●  {roleLabel}";
            _userLabel.ForeColor = UiTheme.TextDark;
            _userLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            _userLabel.AutoSize = true;
            _headerPanel.Resize += (_, _) => _userLabel.Location = new Point(_headerPanel.Width - _userLabel.Width - 22, 20);
            _headerPanel.Controls.Add(_userLabel);
        }

        /// <summary>Đăng ký một màn hình (nội dung `.e-content`) theo key điều hướng.</summary>
        public void RegisterView(string key, string title, Func<Control> factory)
        {
            _viewFactories[key] = factory;
            _titles[key] = title;
        }

        public void NavigateTo(string key) => OnNavigate(key);

        private void OnNavigate(string key)
        {
            if (key == "logout")
            {
                SessionContext.SignOut();
                var login = new FrmLogin();
                login.Show();
                Close();
                return;
            }

            if (!_viewFactories.TryGetValue(key, out var factory))
                return;

            _crumbLabel.Text = (SessionContext.IsAdmin ? "Quản trị hệ thống" : "Đăng ký học phần") + " / " + _titles[key];

            _contentHost.Controls.Clear();
            var view = factory();
            view.Dock = DockStyle.Fill;
            _contentHost.Controls.Add(view);
        }
    }
}
