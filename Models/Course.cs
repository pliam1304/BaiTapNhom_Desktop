namespace EduPath.Avalonia.Models
{
    /// <summary>
    /// Học phần (môn học) — khác với Section (lớp học phần cụ thể theo học kỳ, có GV/phòng/lịch riêng).
    /// </summary>
    public class Course
    {
        public string CourseCode { get; set; } = string.Empty;   // CS201
        public string CourseName { get; set; } = string.Empty;
        public int Credits { get; set; }
        public string Faculty { get; set; } = string.Empty;       // Khoa/Bộ môn
        public string? PrerequisiteCode { get; set; }              // CS101, có thể null
        public bool IsActive { get; set; } = true;
    }
}
