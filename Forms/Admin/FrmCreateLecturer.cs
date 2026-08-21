using EduPath.WinForms.Common;    // UiTheme
using EduPath.WinForms.Models;    // Model Lecturer
using EduPath.WinForms.Services;  // LecturerAdminService xử lý nghiệp vụ

namespace EduPath.WinForms.Forms.Admin
{
    /// <summary>Modal "+ Thêm giảng viên" — gọi LecturerAdminService.Add.</summary>
    public class FrmCreateLecturer : Form
    {
        private readonly TextBox _txtId = new() { Width = 260 };       // Mã giảng viên
        private readonly TextBox _txtName = new() { Width = 260 };     // Họ tên
        private readonly TextBox _txtEmail = new() { Width = 260 };    // Email
        private readonly TextBox _txtDept = new() { Width = 260 };     // Khoa/Bộ môn
        private readonly Label _lblError = new() { ForeColor = Color.Firebrick, AutoSize = true, MaximumSize = new Size(300, 0) };
        private readonly LecturerAdminService _lecturerService = new(); // Service xử lý nghiệp vụ

        // "new" để che có chủ đích thuộc tính Created sẵn có của Control (tránh warning CS0108)
        public new bool Created { get; private set; } // Cờ báo tạo thành công cho view cha

        public FrmCreateLecturer()
        {
            Text = "Thêm giảng viên";
            Width = 340; Height = 330;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false; MinimizeBox = false;
            Font = UiTheme.FontBase;
            Padding = new Padding(15);

            var layout = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 2, AutoSize = true };
            AddRow(layout, "Mã giảng viên:", _txtId);
            AddRow(layout, "Họ tên:", _txtName);
            AddRow(layout, "Email:", _txtEmail);
            AddRow(layout, "Khoa/Bộ môn:", _txtDept);

            var btnSave = UiTheme.MakeYellowButton("Thêm giảng viên");
            btnSave.Click += BtnSave_Click;
            _lblError.Location = new Point(15, 220); // Dưới layout 4 hàng
            btnSave.Location = new Point(15, 260);

            Controls.Add(layout);
            Controls.Add(_lblError);
            Controls.Add(btnSave);
        }

        private static void AddRow(TableLayoutPanel layout, string label, Control input)
        {
            layout.RowCount++;
            layout.Controls.Add(new Label { Text = label, AutoSize = true, Margin = new Padding(0, 8, 8, 0) });
            layout.Controls.Add(input);
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            // Dựng đối tượng Lecturer từ dữ liệu nhập
            var lecturer = new Lecturer
            {
                LecturerId = _txtId.Text.Trim(),
                FullName = _txtName.Text.Trim(),
                Email = _txtEmail.Text.Trim(),
                Department = _txtDept.Text.Trim(),
                IsActive = true // Giảng viên mới mặc định đang công tác
            };

            var (ok, error) = _lecturerService.Add(lecturer); // Toàn bộ validate nằm trong Service
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
