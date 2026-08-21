using EduPath.WinForms.Common;    // UiTheme
using EduPath.WinForms.Models;    // Model RegistrationPeriod
using EduPath.WinForms.Services;  // RegistrationPeriodService.Create

namespace EduPath.WinForms.Forms.Admin
{
    /// <summary>Modal "+ Tạo đợt đăng ký" — gọi RegistrationPeriodService.Create.</summary>
    public class FrmCreatePeriod : Form
    {
        private readonly TextBox _txtName = new() { Width = 260 };  // Tên đợt (vd "Đợt bổ sung HK1")
        private readonly TextBox _txtTerm = new() { Width = 260 };  // Học kỳ áp dụng (vd "HK1 2026-2027")
        private readonly DateTimePicker _dtStart = new() { Width = 260, Format = DateTimePickerFormat.Short, Value = DateTime.Today };       // Ngày bắt đầu
        private readonly DateTimePicker _dtEnd = new() { Width = 260, Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddDays(7) }; // Ngày kết thúc
        private readonly NumericUpDown _numMin = new() { Width = 260, Minimum = 1, Maximum = 40, Value = 12 };  // Tín chỉ tối thiểu
        private readonly NumericUpDown _numMax = new() { Width = 260, Minimum = 1, Maximum = 40, Value = 24 };  // Tín chỉ tối đa
        private readonly Label _lblError = new() { ForeColor = Color.Firebrick, AutoSize = true, MaximumSize = new Size(300, 0) };
        private readonly RegistrationPeriodService _periodService = new(); // Service xử lý nghiệp vụ

        // "new" để che có chủ đích thuộc tính Created sẵn có của Control (tránh warning CS0108)
        public new bool Created { get; private set; } // Cờ báo tạo thành công cho view cha

        public FrmCreatePeriod()
        {
            Text = "Tạo đợt đăng ký";
            Width = 340; Height = 430;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false; MinimizeBox = false;
            Font = UiTheme.FontBase;
            Padding = new Padding(15);

            var layout = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 2, AutoSize = true };
            AddRow(layout, "Tên đợt:", _txtName);
            AddRow(layout, "Học kỳ:", _txtTerm);
            AddRow(layout, "Ngày bắt đầu:", _dtStart);
            AddRow(layout, "Ngày kết thúc:", _dtEnd);
            AddRow(layout, "TC tối thiểu:", _numMin);
            AddRow(layout, "TC tối đa:", _numMax);

            var btnSave = UiTheme.MakeYellowButton("Tạo đợt đăng ký");
            btnSave.Click += BtnSave_Click;
            _lblError.Location = new Point(15, 320); // Dưới layout 6 hàng
            btnSave.Location = new Point(15, 360);

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
            // Dựng đối tượng RegistrationPeriod từ dữ liệu nhập
            var period = new RegistrationPeriod
            {
                Name = _txtName.Text.Trim(),
                Term = _txtTerm.Text.Trim(),
                StartDate = _dtStart.Value.Date, // Chỉ lấy phần ngày, bỏ giờ:phút:giây
                EndDate = _dtEnd.Value.Date,
                MinCredits = (int)_numMin.Value,
                MaxCredits = (int)_numMax.Value
                // IsOpen KHÔNG set ở đây — Service.Create tự gán = false để đảm bảo phải bấm "Mở đợt" riêng
            };

            var (ok, error) = _periodService.Create(period); // Toàn bộ validate nằm trong Service
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
