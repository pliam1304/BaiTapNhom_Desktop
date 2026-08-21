using EduPath.WinForms.Common;    // UiTheme
using EduPath.WinForms.Models;    // Model Room
using EduPath.WinForms.Services;  // RoomService xử lý nghiệp vụ

namespace EduPath.WinForms.Forms.Admin
{
    /// <summary>Modal "+ Thêm phòng" — gọi RoomService.Add.</summary>
    public class FrmCreateRoom : Form
    {
        private readonly TextBox _txtId = new() { Width = 260 };        // Mã phòng
        private readonly TextBox _txtBuilding = new() { Width = 260 };  // Tòa nhà
        private readonly NumericUpDown _numCapacity = new() { Width = 260, Minimum = 1, Maximum = 500, Value = 60 }; // Sức chứa
        private readonly ComboBox _cboType = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 260 }; // Loại phòng
        private readonly Label _lblError = new() { ForeColor = Color.Firebrick, AutoSize = true, MaximumSize = new Size(300, 0) };
        private readonly RoomService _roomService = new(); // Service xử lý nghiệp vụ

        // "new" để che có chủ đích thuộc tính Created sẵn có của Control (tránh warning CS0108)
        public new bool Created { get; private set; } // Cờ báo tạo thành công cho view cha

        public FrmCreateRoom()
        {
            Text = "Thêm phòng học";
            Width = 340; Height = 330;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false; MinimizeBox = false;
            Font = UiTheme.FontBase;
            Padding = new Padding(15);

            // Danh sách loại phòng cố định — khớp với giá trị mặc định trong Models/Room.cs
            _cboType.DataSource = new[] { "Lý thuyết", "Thực hành" };

            var layout = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 2, AutoSize = true };
            AddRow(layout, "Mã phòng:", _txtId);
            AddRow(layout, "Tòa nhà:", _txtBuilding);
            AddRow(layout, "Sức chứa:", _numCapacity);
            AddRow(layout, "Loại phòng:", _cboType);

            var btnSave = UiTheme.MakeYellowButton("Thêm phòng");
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
            // Dựng đối tượng Room từ dữ liệu nhập
            var room = new Room
            {
                RoomId = _txtId.Text.Trim(),
                Building = _txtBuilding.Text.Trim(),
                Capacity = (int)_numCapacity.Value,
                RoomType = _cboType.SelectedItem?.ToString() ?? "Lý thuyết", // Phòng thủ nếu chưa chọn gì
                IsAvailable = true // Phòng mới mặc định đang sử dụng được
            };

            var (ok, error) = _roomService.Add(room); // Toàn bộ validate nằm trong Service
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
