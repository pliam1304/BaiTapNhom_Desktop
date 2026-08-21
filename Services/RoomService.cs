using EduPath.WinForms.Data;   // InMemoryStore lưu danh sách Rooms dùng chung toàn app
using EduPath.WinForms.Models; // Model Room

namespace EduPath.WinForms.Services
{
    /// <summary>
    /// Nghiệp vụ cho màn "Phòng & Lịch học" (admin) — phần quản lý DANH MỤC phòng.
    /// Việc kiểm tra xung đột lịch giữa các phòng đã có sẵn ở ScheduleConflictService,
    /// service này chỉ lo CRUD danh mục phòng (thêm/tra cứu/bật-tắt sử dụng).
    /// </summary>
    public class RoomService
    {
        // Tham chiếu tới "CSDL" trong RAM
        private readonly InMemoryStore _store = InMemoryStore.Instance;

        /// <summary>Toàn bộ danh sách phòng học để hiển thị lên grid.</summary>
        public IReadOnlyList<Room> GetAll() => _store.Rooms;

        /// <summary>Tìm phòng theo mã (vd A201).</summary>
        public Room? GetById(string roomId) =>
            _store.Rooms.FirstOrDefault(r => r.RoomId == roomId); // So khớp đúng mã phòng

        /// <summary>Thêm phòng học mới sau khi kiểm tra hợp lệ.</summary>
        public (bool ok, string? error) Add(Room room)
        {
            // Mã phòng bắt buộc phải có
            if (string.IsNullOrWhiteSpace(room.RoomId))
                return (false, "Mã phòng không được để trống.");

            // Không cho trùng mã phòng đã tồn tại
            if (_store.Rooms.Any(r => r.RoomId == room.RoomId))
                return (false, $"Phòng '{room.RoomId}' đã tồn tại.");

            // Sức chứa phải là số dương thì mới xếp lớp được
            if (room.Capacity <= 0)
                return (false, "Sức chứa phòng phải lớn hơn 0.");

            // Hợp lệ -> thêm vào danh sách
            _store.Rooms.Add(room);
            return (true, null); // Thành công
        }

        /// <summary>
        /// Ngừng sử dụng một phòng — chặn nếu phòng đang được gán cho lớp học phần còn mở,
        /// để tránh trường hợp lớp đang học mà phòng "biến mất" khỏi hệ thống.
        /// </summary>
        public (bool ok, string? error) SetAvailability(string roomId, bool isAvailable)
        {
            var room = GetById(roomId);                     // Tìm phòng cần đổi trạng thái
            if (room is null) return (false, "Không tìm thấy phòng học."); // Không có -> lỗi

            if (!isAvailable) // Chỉ cần kiểm tra ràng buộc khi đang NGỪNG sử dụng phòng
            {
                // Kiểm tra phòng có đang gắn với lớp học phần nào còn mở đăng ký không
                bool inUse = _store.Sections.Any(s => s.RoomId == roomId && s.IsOpen);
                if (inUse)
                    return (false, "Không thể ngừng sử dụng: phòng đang gán cho lớp học phần đang mở."); // Chặn
            }

            room.IsAvailable = isAvailable; // Cập nhật trạng thái sử dụng
            return (true, null);            // Thành công
        }
    }
}
