using EduPath.Avalonia.Models;

namespace EduPath.Avalonia.ViewModels
{
    /// <summary>Gói một Section kèm thông tin hiển thị (tên học phần, GV, phòng) để bind thẳng vào View.</summary>
    public class SectionRow
    {
        public Section Section { get; }
        public Course? Course { get; }
        public Lecturer? Lecturer { get; }

        public SectionRow(Section section, Course? course, Lecturer? lecturer)
        {
            Section = section;
            Course = course;
            Lecturer = lecturer;
        }

        public string SectionId => Section.SectionId;
        public string CourseName => Course?.CourseName ?? Section.CourseCode;
        public string CourseCode => Section.CourseCode;
        public int Credits => Course?.Credits ?? 0;
        public string LecturerName => Lecturer?.FullName ?? Section.LecturerId;
        public string RoomId => Section.RoomId;
        public string Schedule => $"{Section.DayLabel} · {Section.TimeLabel}";
        public string SeatsLabel => $"{Section.Enrolled}/{Section.Capacity}";
        public int Remaining => Section.Remaining;
        public bool IsFull => Section.Remaining <= 0;
        public string StatusBadge => !Section.IsOpen ? "badge-off" : IsFull ? "badge-warn" : "badge-ok";
        public string StatusText => !Section.IsOpen ? "Đã đóng" : IsFull ? "Gần đầy" : "Còn chỗ";
    }
}
