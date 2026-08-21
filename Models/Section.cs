namespace EduPath.WinForms.Models
{
    /// <summary>
    /// Lớp học phần: một Course được mở trong một học kỳ cụ thể, gắn GV/phòng/khung giờ/sĩ số.
    /// DayOfWeek dùng số 2-8 kiểu VN (Thứ 2 = 2 ... Chủ nhật = 8) để khớp UI mockup "Thứ 2".
    /// </summary>
    public class Section
    {
        public string SectionId { get; set; } = string.Empty;   // CS201-01
        public string CourseCode { get; set; } = string.Empty;
        public string Term { get; set; } = string.Empty;         // HK1 2026-2027
        public string LecturerId { get; set; } = string.Empty;
        public string RoomId { get; set; } = string.Empty;

        public int DayOfWeek { get; set; }                       // 2..8 (Thứ 2..Chủ nhật)
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public int Capacity { get; set; }
        public int Enrolled { get; set; }
        public bool IsOpen { get; set; } = true;

        public int Remaining => Capacity - Enrolled;

        public string DayLabel => DayOfWeek == 8 ? "Chủ nhật" : $"Thứ {DayOfWeek}";
        public string TimeLabel => $"{StartTime:hh\\:mm}–{EndTime:hh\\:mm}";

        /// <summary>Hai khung giờ có giao nhau về thời gian không (chưa xét thứ/phòng/GV).</summary>
        public bool TimeOverlaps(Section other) =>
            DayOfWeek == other.DayOfWeek &&
            StartTime < other.EndTime && other.StartTime < EndTime;
    }
}
