using EduPath.WinForms.Common;
using EduPath.WinForms.Data;
using EduPath.WinForms.Models;
using EduPath.WinForms.Services;

namespace EduPath.WinForms.Forms.Admin
{
    /// <summary>Modal "+ Tạo lớp học phần" — gọi SectionService.Create, hiển thị lỗi xung đột nếu có.</summary>
    public class FrmCreateSection : Form
    {
        private readonly ComboBox _cboCourse = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 260 };
        private readonly ComboBox _cboLecturer = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 260 };
        private readonly ComboBox _cboRoom = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 260 };
        private readonly ComboBox _cboDay = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 260 };
        private readonly TextBox _txtSectionId = new() { Width = 260 };
        private readonly NumericUpDown _numCapacity = new() { Width = 260, Minimum = 1, Maximum = 300, Value = 60 };
        private readonly DateTimePicker _dtStart = new() { Format = DateTimePickerFormat.Time, ShowUpDown = true, Width = 260, Value = DateTime.Today.AddHours(9) };
        private readonly DateTimePicker _dtEnd = new() { Format = DateTimePickerFormat.Time, ShowUpDown = true, Width = 260, Value = DateTime.Today.AddHours(10).AddMinutes(30) };
        private readonly Label _lblError = new() { ForeColor = Color.Firebrick, AutoSize = true, MaximumSize = new Size(300, 0) };
        private readonly SectionService _sectionService = new();

        // Thêm từ khóa "new" vì lớp Control (cha của Form) đã có sẵn thuộc tính "Created" riêng
        // (đúng nghĩa là "đã tạo xong control chưa"), khác hoàn toàn với ý nghĩa của ta ở đây
        // ("đã lưu lớp học phần thành công chưa"). "new" báo cho trình biên dịch biết đây là
        // một thuộc tính MỚI cố ý che (hide) thuộc tính cũ, không phải override nhầm.
        public new bool Created { get; private set; }

        public FrmCreateSection()
        {
            Text = "Tạo lớp học phần";
            Width = 340; Height = 460;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false; MinimizeBox = false;
            Font = UiTheme.FontBase;
            Padding = new Padding(15);

            var store = InMemoryStore.Instance;
            _cboCourse.DataSource = store.Courses.Select(c => $"{c.CourseCode} · {c.CourseName}").ToList();
            _cboLecturer.DataSource = store.Lecturers.Select(l => $"{l.LecturerId} · {l.FullName}").ToList();
            _cboRoom.DataSource = store.Rooms.Select(r => r.RoomId).ToList();
            _cboDay.DataSource = new[] { "Thứ 2", "Thứ 3", "Thứ 4", "Thứ 5", "Thứ 6", "Thứ 7", "Chủ nhật" };

            var layout = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 2, AutoSize = true };
            AddRow(layout, "Mã lớp:", _txtSectionId);
            AddRow(layout, "Học phần:", _cboCourse);
            AddRow(layout, "Giảng viên:", _cboLecturer);
            AddRow(layout, "Phòng:", _cboRoom);
            AddRow(layout, "Thứ:", _cboDay);
            AddRow(layout, "Giờ bắt đầu:", _dtStart);
            AddRow(layout, "Giờ kết thúc:", _dtEnd);
            AddRow(layout, "Sĩ số tối đa:", _numCapacity);

            var btnSave = UiTheme.MakeYellowButton("Tạo lớp học phần");
            btnSave.Click += BtnSave_Click;
            _lblError.Location = new Point(15, 350);
            btnSave.Location = new Point(15, 390);

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
            if (_cboCourse.SelectedItem is null || _cboLecturer.SelectedItem is null || _cboRoom.SelectedItem is null)
            {
                _lblError.Text = "Vui lòng chọn đầy đủ học phần / giảng viên / phòng.";
                return;
            }

            var courseCode = _cboCourse.SelectedItem!.ToString()!.Split(" · ")[0];
            var lecturerId = _cboLecturer.SelectedItem!.ToString()!.Split(" · ")[0];
            var roomId = _cboRoom.SelectedItem!.ToString()!;
            int dayOfWeek = _cboDay.SelectedIndex + 2; // Thứ 2 = index 0 -> 2

            var section = new Section
            {
                SectionId = _txtSectionId.Text.Trim(),
                CourseCode = courseCode,
                Term = "HK1 2026-2027",
                LecturerId = lecturerId,
                RoomId = roomId,
                DayOfWeek = dayOfWeek,
                StartTime = _dtStart.Value.TimeOfDay,
                EndTime = _dtEnd.Value.TimeOfDay,
                Capacity = (int)_numCapacity.Value,
                Enrolled = 0,
                IsOpen = true
            };

            var (ok, error) = _sectionService.Create(section);
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
