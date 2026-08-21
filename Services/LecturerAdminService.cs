using EduPath.WinForms.Data;   // InMemoryStore: nơi lưu danh sách Lecturers dùng chung toàn app
using EduPath.WinForms.Models; // Model Lecturer

namespace EduPath.WinForms.Services
{
    /// <summary>Nghiệp vụ cho màn "Quản lý giảng viên" (admin): thêm, tra cứu, khóa/mở hồ sơ giảng viên.</summary>
    public class LecturerAdminService
    {
        // Tham chiếu duy nhất tới kho dữ liệu trong RAM
        private readonly InMemoryStore _store = InMemoryStore.Instance;

        /// <summary>Toàn bộ danh sách giảng viên, dùng để đổ vào DataGridView.</summary>
        public IReadOnlyList<Lecturer> GetAll() => _store.Lecturers;

        /// <summary>Tìm giảng viên theo mã (vd GV0008).</summary>
        public Lecturer? GetById(string lecturerId) =>
            _store.Lecturers.FirstOrDefault(l => l.LecturerId == lecturerId); // So khớp đúng mã GV

        /// <summary>Thêm mới một giảng viên sau khi kiểm tra hợp lệ dữ liệu đầu vào.</summary>
        public (bool ok, string? error) Add(Lecturer lecturer)
        {
            // Mã giảng viên bắt buộc phải có (dùng làm khóa chính trong danh sách)
            if (string.IsNullOrWhiteSpace(lecturer.LecturerId))
                return (false, "Mã giảng viên không được để trống.");

            // Không cho phép trùng mã giảng viên đã tồn tại
            if (_store.Lecturers.Any(l => l.LecturerId == lecturer.LecturerId))
                return (false, $"Mã giảng viên '{lecturer.LecturerId}' đã tồn tại.");

            // Họ tên là thông tin bắt buộc để hiển thị trên các lớp học phần
            if (string.IsNullOrWhiteSpace(lecturer.FullName))
                return (false, "Họ tên giảng viên không được để trống.");

            // Mọi điều kiện hợp lệ -> thêm vào danh sách trong bộ nhớ
            _store.Lecturers.Add(lecturer);
            return (true, null); // Trả về thành công, không lỗi
        }

        /// <summary>
        /// Vô hiệu hóa giảng viên — chặn nếu giảng viên đang đứng lớp mở đăng ký,
        /// tương tự quy tắc CourseService.Deactivate (tránh phá vỡ dữ liệu đang dùng dở).
        /// </summary>
        public (bool ok, string? error) Deactivate(string lecturerId)
        {
            var lecturer = GetById(lecturerId);                    // Tìm giảng viên cần khóa
            if (lecturer is null) return (false, "Không tìm thấy giảng viên."); // Không tồn tại -> lỗi

            // Kiểm tra xem giảng viên có đang phụ trách lớp học phần nào còn mở đăng ký không
            bool teachingOpenSection = _store.Sections.Any(s => s.LecturerId == lecturerId && s.IsOpen);
            if (teachingOpenSection)
                return (false, "Không thể vô hiệu hóa: giảng viên đang phụ trách lớp học phần đang mở."); // Chặn khóa

            lecturer.IsActive = false; // Đủ điều kiện -> đánh dấu ngừng hoạt động
            return (true, null);       // Thành công
        }

        /// <summary>Kích hoạt lại giảng viên đã bị khóa trước đó.</summary>
        public (bool ok, string? error) Activate(string lecturerId)
        {
            var lecturer = GetById(lecturerId);                    // Tìm giảng viên
            if (lecturer is null) return (false, "Không tìm thấy giảng viên."); // Không có -> lỗi

            lecturer.IsActive = true; // Mở lại trạng thái hoạt động
            return (true, null);      // Thành công
        }
    }
}
