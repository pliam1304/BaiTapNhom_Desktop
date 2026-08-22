namespace EduPath.Avalonia.Models
{
    /// <summary>
    /// Đợt đăng ký học phần (màn 14 "Quản lý đợt đăng ký"). EnrollmentService kiểm tra
    /// đợt hiện hành trước khi cho phép đăng ký/hủy.
    /// </summary>
    public class RegistrationPeriod
    {
        public string Name { get; set; } = string.Empty;     // Đợt đăng ký HK1 2026-2027
        public string Term { get; set; } = string.Empty;      // HK1 2026-2027
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int MinCredits { get; set; } = 12;
        public int MaxCredits { get; set; } = 24;
        public bool IsOpen { get; set; }

        public bool IsCurrentlyOpen(DateTime now) =>
            IsOpen && now.Date >= StartDate.Date && now.Date <= EndDate.Date;
    }
}
