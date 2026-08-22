namespace EduPath.Avalonia.Models
{
    public class Student
    {
        public string StudentId { get; set; } = string.Empty;   // SV20260018
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Faculty { get; set; } = string.Empty;      // Khoa/Bộ môn
        public int IntakeYear { get; set; }                      // Khóa
        public string ClassCode { get; set; } = string.Empty;    // Lớp hành chính (CNTT01)
        public bool IsActive { get; set; } = true;                // Đang học / đã nghỉ

        /// <summary>Danh sách mã học phần sinh viên đã hoàn thành — dùng để kiểm tra tiên quyết.</summary>
        public HashSet<string> CompletedCourseCodes { get; set; } = new();

        public int MinCreditsPerTerm { get; set; } = 12;
        public int MaxCreditsPerTerm { get; set; } = 24;
    }
}
