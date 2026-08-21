namespace EduPath.WinForms.Models
{
    public enum EnrollmentStatus
    {
        Enrolled,     // Đã đăng ký
        Cancelled,    // Đã hủy
        Pending       // Đang chờ xử lý (ví dụ chờ duyệt vượt tín chỉ)
    }

    /// <summary>
    /// Một lượt đăng ký của sinh viên vào một lớp học phần — đây là "sổ lịch sử đăng ký" (màn 08)
    /// đồng thời là nguồn dữ liệu cho "Học phần đã đăng ký" (màn 06) khi Status = Enrolled.
    /// </summary>
    public class Enrollment
    {
        public string StudentId { get; set; } = string.Empty;
        public string SectionId { get; set; } = string.Empty;
        public DateTime RegisteredAt { get; set; }
        public EnrollmentStatus Status { get; set; }
    }
}
