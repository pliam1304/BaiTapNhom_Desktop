namespace EduPath.Avalonia.Models
{
    public class Lecturer
    {
        public string LecturerId { get; set; } = string.Empty;  // GV0008
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;   // Khoa/Bộ môn
        public bool IsActive { get; set; } = true;
    }
}
