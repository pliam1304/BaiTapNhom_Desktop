using EduPath.WinForms.Common;    // UiTheme
using EduPath.WinForms.Models;    // Model Student
using EduPath.WinForms.Services;  // StudentAdminService xử lý nghiệp vụ tạo hồ sơ + tài khoản

namespace EduPath.WinForms.Forms.Admin
{
    /// <summary>Modal "+ Thêm sinh viên" — tạo hồ sơ Student và tự động tạo Account đăng nhập đi kèm.</summary>
    public class FrmCreateStudent : Form
    {
        // ---- Control nhập liệu ----
        private readonly TextBox _txtId = new() { Width = 260 };            // MSSV
        private readonly TextBox _txtName = new() { Width = 260 };          // Họ tên
        private readonly TextBox _txtEmail = new() { Width = 260 };         // Email
        private readonly TextBox _txtFaculty = new() { Width = 260 };       // Khoa
        private readonly NumericUpDown _numIntakeYear = new() { Width = 260, Minimum = 2000, Maximum = 2100, Value = DateTime.Now.Year }; // Khóa nhập học
        private readonly TextBox _txtClassCode = new() { Width = 260 };     // Lớp hành chính
        private readonly TextBox _txtPassword = new() { Width = 260, UseSystemPasswordChar = true }; // Mật khẩu khởi tạo tài khoản
        private readonly Label _lblError = new() { ForeColor = Color.Firebrick, AutoSize = true, MaximumSize = new Size(300, 0) };
        private readonly StudentAdminService _studentService = new(); // Service xử lý nghiệp vụ

        // "new" để che có chủ đích thuộc tính Created sẵn có của Control (tránh warning CS0108)
        public new bool Created { get; private set; } // Cờ báo tạo thành công cho view cha

        public FrmCreateStudent()
        {
            Text = "Thêm sinh viên";
            Width = 340; Height = 480; // Cao hơn FrmCreateCourse vì có nhiều trường hơn
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false; MinimizeBox = false;
            Font = UiTheme.FontBase;
            Padding = new Padding(15);

            // ---- Layout bảng 2 cột, cùng mẫu FrmCreateSection ----
            var layout = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 2, AutoSize = true };
            AddRow(layout, "MSSV:", _txtId);
            AddRow(layout, "Họ tên:", _txtName);
            AddRow(layout, "Email:", _txtEmail);
            AddRow(layout, "Khoa:", _txtFaculty);
            AddRow(layout, "Khóa nhập học:", _numIntakeYear);
            AddRow(layout, "Lớp hành chính:", _txtClassCode);
            AddRow(layout, "Mật khẩu khởi tạo:", _txtPassword);

            var btnSave = UiTheme.MakeYellowButton("Thêm sinh viên");
            btnSave.Click += BtnSave_Click;
            _lblError.Location = new Point(15, 370); // Đặt dưới layout (7 hàng x ~34px + margin)
            btnSave.Location = new Point(15, 410);

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
            // Dựng đối tượng Student từ dữ liệu nhập.
            // Lưu ý: phải ghi rõ EduPath.WinForms.Models.Student (không chỉ "Student") vì project
            // còn có namespace EduPath.WinForms.Forms.Student (thư mục Forms/Student) — nếu chỉ viết
            // "Student", trình biên dịch ưu tiên namespace anh em đó trước class Models.Student,
            // gây lỗi CS0118 "'Student' is a namespace but is used like a type".
            var student = new EduPath.WinForms.Models.Student
            {
                StudentId = _txtId.Text.Trim(),
                FullName = _txtName.Text.Trim(),
                Email = _txtEmail.Text.Trim(),
                Faculty = _txtFaculty.Text.Trim(),
                IntakeYear = (int)_numIntakeYear.Value,
                ClassCode = _txtClassCode.Text.Trim(),
                IsActive = true,                          // Sinh viên mới mặc định đang học
                CompletedCourseCodes = new HashSet<string>(), // Chưa hoàn thành học phần nào khi mới tạo
                MinCreditsPerTerm = 12,                    // Giá trị mặc định giống seed data trong InMemoryStore
                MaxCreditsPerTerm = 24
            };

            // Gọi Service: vừa validate vừa tạo kèm Account — Form không tự viết logic này
            var (ok, error) = _studentService.Add(student, _txtPassword.Text);
            if (!ok)
            {
                _lblError.Text = error; // Hiển thị lỗi ngay tại chỗ
                return;
            }

            Created = true; // Báo thành công cho view cha
            Close();
        }
    }
}
