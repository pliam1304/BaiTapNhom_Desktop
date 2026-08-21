using EduPath.WinForms.Models;

namespace EduPath.WinForms.Common
{
    /// <summary>
    /// Thanh điều hướng trái (navy sidebar) dùng chung cho mọi Form sau đăng nhập, thay thế hàm
    /// JS `side()` trong mockup gốc. Menu item khác nhau theo Role. Raise sự kiện NavigateRequested
    /// để MainShellForm điều hướng — SideMenu không tự biết Form đích, giữ tách bạch UI khỏi luồng điều hướng.
    /// </summary>
    public class SideMenu : Panel
    {
        public event Action<string>? NavigateRequested;

        public SideMenu(Role role)
        {
            Dock = DockStyle.Left;
            Width = 210;
            BackColor = UiTheme.Navy;
            Padding = new Padding(11, 18, 11, 18);

            var layout = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.TopDown,
                AutoSize = true,
                WrapContents = false,
                BackColor = UiTheme.Navy
            };
            Controls.Add(layout);

            layout.Controls.Add(MakeBrand());
            layout.Controls.Add(MakeGroupLabel("DASHBOARD"));
            layout.Controls.Add(MakeNavItem("Tổng quan", role == Role.Admin ? "admin-dashboard" : "student-dashboard"));

            if (role == Role.Admin)
            {
                layout.Controls.Add(MakeGroupLabel("QUẢN LÝ ĐÀO TẠO"));
                layout.Controls.Add(MakeNavItem("Học phần", "courses"));
                layout.Controls.Add(MakeNavItem("Lớp học phần", "sections-admin"));
                layout.Controls.Add(MakeNavItem("Sinh viên", "students"));
                layout.Controls.Add(MakeNavItem("Giảng viên", "lecturers"));
                layout.Controls.Add(MakeNavItem("Học kỳ & Đợt đăng ký", "periods"));
                layout.Controls.Add(MakeNavItem("Phòng & Lịch học", "rooms"));
                layout.Controls.Add(MakeGroupLabel("HỆ THỐNG"));
                layout.Controls.Add(MakeNavItem("Tài khoản & Phân quyền", "accounts"));
            }
            else
            {
                layout.Controls.Add(MakeGroupLabel("ĐĂNG KÝ HỌC PHẦN"));
                layout.Controls.Add(MakeNavItem("Học phần mở đăng ký", "open-sections"));
                layout.Controls.Add(MakeNavItem("Học phần đã đăng ký", "enrolled"));
                layout.Controls.Add(MakeNavItem("Lịch học", "timetable"));
                layout.Controls.Add(MakeNavItem("Lịch sử đăng ký", "history"));
                layout.Controls.Add(MakeGroupLabel("HỆ THỐNG"));
            }

            layout.Controls.Add(MakeNavItem("Đăng xuất", "logout"));
        }

        private Label MakeBrand() => new()
        {
            Text = "EDUPATH",
            ForeColor = Color.White,
            Font = UiTheme.FontBrand,
            AutoSize = true,
            Margin = new Padding(9, 0, 0, 24)
        };

        private Label MakeGroupLabel(string text) => new()
        {
            Text = text,
            ForeColor = ColorTranslator.FromHtml("#91a9c2"),
            Font = new Font("Segoe UI", 7.5F),
            AutoSize = true,
            Margin = new Padding(9, 12, 0, 6)
        };

        private Label MakeNavItem(string text, string navKey)
        {
            var lbl = new Label
            {
                Text = text,
                ForeColor = ColorTranslator.FromHtml("#dce8f6"),
                Font = UiTheme.FontBase,
                AutoSize = false,
                Width = 185,
                Padding = new Padding(9, 6, 9, 6),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 1, 0, 1)
            };
            lbl.Click += (_, _) => NavigateRequested?.Invoke(navKey);
            lbl.MouseEnter += (_, _) => lbl.BackColor = UiTheme.NavyLight;
            lbl.MouseLeave += (_, _) => lbl.BackColor = UiTheme.Navy;
            return lbl;
        }
    }
}
