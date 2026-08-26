namespace EduPath.Avalonia.Models
{
    /// <summary>
    /// Đợt đăng ký học phần.
    /// </summary>
    public class RegistrationPeriod
    {
        public string Name { get; set; } = string.Empty;

        public string Term { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        /// <summary>
        /// Tổng tín chỉ tối thiểu.
        /// </summary>
        public int MinCredits { get; set; } = 12;

        /// <summary>
        /// Tổng tín chỉ tối đa.
        /// </summary>
        public int MaxCredits { get; set; } = 24;

        /// <summary>
        /// Số tín chỉ tự chọn tối thiểu.
        /// </summary>
        public int MinElectiveCredits { get; set; } = 3;

        /// <summary>
        /// Số tín chỉ tự chọn tối đa.
        /// </summary>
        public int MaxElectiveCredits { get; set; } = 9;

        /// <summary>
        /// Trạng thái do Admin/Giảng viên quản lý.
        /// </summary>
        public bool IsOpen { get; set; }

        /// <summary>
        /// Kiểm tra đợt đăng ký có thực sự đang mở.
        /// Phải đồng thời:
        /// - Admin mở IsOpen
        /// - Ngày hiện tại nằm trong StartDate và EndDate
        /// </summary>
        public bool IsCurrentlyOpen(DateTime now)
        {
            return IsOpen
                   && now.Date >= StartDate.Date
                   && now.Date <= EndDate.Date;
        }
    }
}