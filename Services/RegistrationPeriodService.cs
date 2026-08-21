using EduPath.WinForms.Data;
using EduPath.WinForms.Models;

namespace EduPath.WinForms.Services
{
    /// <summary>Nghiệp vụ cho màn 14 · Quản lý đợt đăng ký, và cấp thông tin đợt hiện hành cho dashboard SV.</summary>
    public class RegistrationPeriodService
    {
        private readonly InMemoryStore _store = InMemoryStore.Instance;

        public IReadOnlyList<RegistrationPeriod> GetAll() => _store.Periods; // Trả nguyên danh sách cho grid

        public RegistrationPeriod? GetCurrent() =>
            // Ưu tiên đợt đang mở; nếu không có đợt nào mở thì lấy tạm đợt đầu tiên (để dashboard vẫn có dữ liệu hiển thị)
            _store.Periods.FirstOrDefault(p => p.IsOpen) ?? _store.Periods.FirstOrDefault();

        /// <summary>Tạo mới một đợt đăng ký (dùng cho nút "+ Tạo đợt đăng ký" ở màn quản lý).</summary>
        public (bool ok, string? error) Create(RegistrationPeriod period)
        {
            // Tên đợt là khóa chính để tra cứu (Open/Close đều dùng Name) nên bắt buộc phải có
            if (string.IsNullOrWhiteSpace(period.Name))
                return (false, "Tên đợt đăng ký không được để trống.");

            // Không cho phép hai đợt trùng tên vì Open/Close sẽ không phân biệt được
            if (_store.Periods.Any(p => p.Name == period.Name))
                return (false, $"Đợt đăng ký '{period.Name}' đã tồn tại.");

            // Ngày bắt đầu phải trước ngày kết thúc thì khoảng thời gian mới có ý nghĩa
            if (period.StartDate.Date > period.EndDate.Date)
                return (false, "Ngày bắt đầu phải trước ngày kết thúc.");

            // Số tín chỉ tối thiểu phải nhỏ hơn hoặc bằng tối đa
            if (period.MinCredits > period.MaxCredits)
                return (false, "Số tín chỉ tối thiểu không được lớn hơn số tín chỉ tối đa.");

            // Đợt mới tạo mặc định luôn ở trạng thái ĐÓNG, phải dùng nút "Mở đợt" riêng để kích hoạt
            // (tránh vô tình có 2 đợt cùng mở — vi phạm ràng buộc trong hàm Open() bên dưới)
            period.IsOpen = false;

            _store.Periods.Add(period); // Hợp lệ -> thêm vào danh sách
            return (true, null);        // Thành công
        } // Kết thúc method Create

        public (bool ok, string? error) Open(string name)
        {
            var period = _store.Periods.FirstOrDefault(p => p.Name == name);
            if (period is null) return (false, "Không tìm thấy đợt đăng ký.");

            bool anotherOpen = _store.Periods.Any(p => p.Name != name && p.IsOpen);
            if (anotherOpen)
                return (false, "Đã có một đợt đăng ký khác đang mở. Vui lòng đóng đợt đó trước.");

            period.IsOpen = true;
            return (true, null);
        }

        public (bool ok, string? error) Close(string name)
        {
            var period = _store.Periods.FirstOrDefault(p => p.Name == name);
            if (period is null) return (false, "Không tìm thấy đợt đăng ký.");
            period.IsOpen = false;
            return (true, null);
        }
    }
}
