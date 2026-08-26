namespace EduPath.Avalonia.Models
{
    /// <summary>
    /// Học phần (môn học) — khác với Section (lớp học phần cụ thể theo học kỳ, có GV/phòng/lịch riêng).
    /// </summary>
    public class Course
    {
        public string CourseCode { get; set; } = string.Empty;            
        public string CourseName { get; set; } = string.Empty;
        public int Credits { get; set; }
        public string Faculty { get; set; } = string.Empty;        
        public string? PrerequisiteCode { get; set; }              
        public bool IsActive { get; set; } = true;
        public bool IsRequired { get; set; } = true;

        public string ElectiveGroup { get; set; } = string.Empty;
    }
}
