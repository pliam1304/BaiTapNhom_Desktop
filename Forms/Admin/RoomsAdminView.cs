using EduPath.WinForms.Common;    // GridHelper, UiTheme
using EduPath.WinForms.Services;  // RoomService xử lý nghiệp vụ danh mục phòng

namespace EduPath.WinForms.Forms.Admin
{
    /// <summary>
    /// Màn "Phòng & Lịch học" — phần quản lý danh mục phòng theo mẫu SectionsAdminView.
    /// Việc xem "Lịch học" chi tiết của từng phòng đã có ở màn Lớp học phần (cột Lịch/Phòng);
    /// view này tập trung vào CRUD phòng, đúng đúng trách nhiệm của RoomService.
    /// </summary>
    public class RoomsAdminView : UserControl
    {
        private readonly RoomService _roomService = new();         // Service nghiệp vụ phòng học
        private readonly DataGridView _grid = GridHelper.MakeGrid(); // Grid style đồng bộ
        private readonly Label _lblMessage = new() { ForeColor = Color.Firebrick, AutoSize = true, Location = new Point(23, 545) };

        public RoomsAdminView()
        {
            Dock = DockStyle.Fill;
            BackColor = UiTheme.Background;
            Padding = new Padding(23, 20, 23, 20);

            var title = new Label { Text = "Phòng & Lịch học", Font = UiTheme.FontHeading, AutoSize = true, Location = new Point(23, 20) };

            var btnCreate = UiTheme.MakeYellowButton("+ Thêm phòng");
            btnCreate.Location = new Point(650, 22);
            btnCreate.Click += (_, _) =>
            {
                using var dlg = new FrmCreateRoom(); // Modal tạo phòng mới
                dlg.ShowDialog();
                if (dlg.Created) LoadData();
            };

            var btnToggle = UiTheme.MakeOutlineButton("Bật/Ngừng sử dụng phòng đã chọn");
            btnToggle.Location = new Point(870, 22);
            btnToggle.Click += BtnToggle_Click;

            var gridHost = new Panel { Location = new Point(23, 60), Size = new Size(900, 480), BorderStyle = BorderStyle.FixedSingle };
            gridHost.Controls.Add(_grid);

            Controls.AddRange(new Control[] { title, btnCreate, btnToggle, gridHost, _lblMessage });
            LoadData();
        }

        private void LoadData()
        {
            var rows = _roomService.GetAll().Select(r => new
            {
                r.RoomId,                                              // Mã phòng
                r.Building,                                            // Tòa nhà
                r.Capacity,                                            // Sức chứa
                LoaiPhong = r.RoomType,                                // Loại phòng (Lý thuyết/Thực hành)
                TrangThai = r.IsAvailable ? "Đang sử dụng" : "Ngừng sử dụng" // Trạng thái sử dụng
            }).ToList();

            _grid.DataSource = rows;
            if (_grid.Columns.Count > 0)
            {
                _grid.Columns["RoomId"].HeaderText = "Mã phòng";
                _grid.Columns["Building"].HeaderText = "Tòa nhà";
                _grid.Columns["Capacity"].HeaderText = "Sức chứa";
                _grid.Columns["LoaiPhong"].HeaderText = "Loại phòng";
                _grid.Columns["TrangThai"].HeaderText = "Trạng thái";
            }
        }

        private void BtnToggle_Click(object? sender, EventArgs e)
        {
            _lblMessage.ForeColor = Color.Firebrick;

            if (_grid.CurrentRow is null)
            {
                _lblMessage.Text = "Vui lòng chọn một phòng trong danh sách trước.";
                return;
            }

            var roomId = _grid.CurrentRow.Cells["RoomId"].Value?.ToString(); // Mã phòng của dòng chọn
            if (string.IsNullOrEmpty(roomId)) return;

            var room = _roomService.GetById(roomId); // Lấy trạng thái hiện tại để quyết định đảo chiều
            if (room is null) return;

            // Đảo trạng thái: đang dùng -> ngừng dùng (có kiểm tra ràng buộc lớp đang mở), ngược lại thì bật lại
            var (ok, error) = _roomService.SetAvailability(roomId, !room.IsAvailable);
            if (!ok)
            {
                _lblMessage.Text = error;
                return;
            }

            _lblMessage.ForeColor = UiTheme.BadgeGreenText;
            _lblMessage.Text = $"Đã cập nhật trạng thái phòng {roomId}.";
            LoadData();
        }
    }
}
