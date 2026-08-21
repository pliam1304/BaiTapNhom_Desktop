using EduPath.WinForms.Common;    // UiTheme cho style form/button đồng bộ
using EduPath.WinForms.Data;      // InMemoryStore để lấy danh sách học phần hiện có (làm dropdown tiên quyết)
using EduPath.WinForms.Models;    // Model Course
using EduPath.WinForms.Services;  // CourseService xử lý nghiệp vụ thêm học phần

namespace EduPath.WinForms.Forms.Admin
{
    /// <summary>Modal "+ Thêm học phần" — gọi CourseService.Add, hiển thị lỗi nghiệp vụ nếu có.</summary>
    public class FrmCreateCourse : Form
    {
        // ---- Các control nhập liệu, khai báo sẵn kích thước đồng bộ giống FrmCreateSection ----
        private readonly TextBox _txtCode = new() { Width = 260 };          // Mã học phần
        private readonly TextBox _txtName = new() { Width = 260 };          // Tên học phần
        private readonly NumericUpDown _numCredits = new() { Width = 260, Minimum = 1, Maximum = 10, Value = 3 }; // Số tín chỉ
        private readonly TextBox _txtFaculty = new() { Width = 260 };       // Khoa phụ trách
        private readonly ComboBox _cboPrerequisite = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 260 }; // Học phần tiên quyết (tùy chọn)
        private readonly Label _lblError = new() { ForeColor = Color.Firebrick, AutoSize = true, MaximumSize = new Size(300, 0) };
        private readonly CourseService _courseService = new(); // Service xử lý nghiệp vụ thêm học phần

        /// <summary>Cờ báo cho view cha biết đã tạo thành công hay chưa, để quyết định có LoadData lại không.</summary>
        // "new" để che có chủ đích thuộc tính Created sẵn có của Control (tránh warning CS0108)
        public new bool Created { get; private set; }

        public FrmCreateCourse()
        {
            // ---- Thiết lập chung cho cửa sổ modal ----
            Text = "Thêm học phần";                      // Tiêu đề cửa sổ
            Width = 340; Height = 360;                    // Kích thước cố định
            StartPosition = FormStartPosition.CenterParent; // Canh giữa so với cửa sổ cha
            FormBorderStyle = FormBorderStyle.FixedDialog;  // Không cho resize
            MaximizeBox = false; MinimizeBox = false;       // Ẩn nút phóng to/thu nhỏ
            Font = UiTheme.FontBase;                        // Font chữ đồng bộ toàn app
            Padding = new Padding(15);                      // Khoảng đệm quanh nội dung

            // Nạp danh sách học phần hiện có vào dropdown "Tiên quyết" — thêm dòng "(không có)" ở đầu
            var store = InMemoryStore.Instance;
            var options = new List<string> { "(không có)" }; // Lựa chọn mặc định: học phần không có tiên quyết
            options.AddRange(store.Courses.Select(c => c.CourseCode)); // Thêm toàn bộ mã học phần đang có
            _cboPrerequisite.DataSource = options;
            _cboPrerequisite.SelectedIndex = 0; // Mặc định chọn "(không có)"

            // ---- Layout dạng bảng 2 cột: nhãn - ô nhập, giống FrmCreateSection ----
            var layout = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 2, AutoSize = true };
            AddRow(layout, "Mã học phần:", _txtCode);
            AddRow(layout, "Tên học phần:", _txtName);
            AddRow(layout, "Số tín chỉ:", _numCredits);
            AddRow(layout, "Khoa:", _txtFaculty);
            AddRow(layout, "Tiên quyết:", _cboPrerequisite);

            // ---- Nút lưu + label lỗi ----
            var btnSave = UiTheme.MakeYellowButton("Thêm học phần");
            btnSave.Click += BtnSave_Click;      // Gắn xử lý khi bấm nút Lưu
            _lblError.Location = new Point(15, 250); // Vị trí hiển thị lỗi
            btnSave.Location = new Point(15, 290);   // Vị trí nút Lưu

            // Gắn toàn bộ control vào form
            Controls.Add(layout);
            Controls.Add(_lblError);
            Controls.Add(btnSave);
        }

        /// <summary>Hàm tiện ích thêm 1 hàng (nhãn + ô nhập) vào TableLayoutPanel — copy nguyên mẫu từ FrmCreateSection.</summary>
        private static void AddRow(TableLayoutPanel layout, string label, Control input)
        {
            layout.RowCount++; // Tăng số hàng lên 1
            layout.Controls.Add(new Label { Text = label, AutoSize = true, Margin = new Padding(0, 8, 8, 0) }); // Cột nhãn
            layout.Controls.Add(input); // Cột ô nhập
        }

        /// <summary>Xử lý khi bấm nút "Thêm học phần": validate cơ bản rồi gọi Service.</summary>
        private void BtnSave_Click(object? sender, EventArgs e)
        {
            // Xác định học phần tiên quyết: nếu chọn "(không có)" thì gán null
            string? prerequisite = _cboPrerequisite.SelectedItem?.ToString();
            if (prerequisite == "(không có)") prerequisite = null;

            // Dựng đối tượng Course từ dữ liệu người dùng nhập
            var course = new Course
            {
                CourseCode = _txtCode.Text.Trim(),        // Cắt khoảng trắng thừa đầu/cuối
                CourseName = _txtName.Text.Trim(),
                Credits = (int)_numCredits.Value,
                Faculty = _txtFaculty.Text.Trim(),
                PrerequisiteCode = prerequisite,
                IsActive = true                            // Học phần mới tạo mặc định đang hoạt động
            };

            // Gọi Service — mọi validate nghiệp vụ (trùng mã, tiên quyết không tồn tại...) nằm ở đây,
            // Form chỉ hiển thị kết quả, không tự phán đoán đúng/sai.
            var (ok, error) = _courseService.Add(course);
            if (!ok)
            {
                _lblError.Text = error; // Hiển thị lý do lỗi ngay dưới form
                return;                  // Không đóng form để người dùng sửa lại
            }

            Created = true; // Báo cho view cha biết đã tạo thành công
            Close();         // Đóng modal
        }
    }
}
