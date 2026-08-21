using EduPath.WinForms.Data;
using EduPath.WinForms.Models;

namespace EduPath.WinForms.Services
{
    /// <summary>Nghiệp vụ cho màn 11 · Quản lý lớp học phần (và dùng chung quy tắc lịch với màn 16).</summary>
    public class SectionService
    {
        private readonly InMemoryStore _store = InMemoryStore.Instance;
        private readonly ScheduleConflictService _conflictService = new();

        public IReadOnlyList<Section> GetAll() => _store.Sections;

        public (bool ok, string? error) Create(Section section)
        {
            if (_store.Sections.Any(s => s.SectionId == section.SectionId))
                return (false, $"Mã lớp '{section.SectionId}' đã tồn tại.");

            if (_store.Courses.All(c => c.CourseCode != section.CourseCode))
                return (false, "Học phần không tồn tại.");

            if (_store.Rooms.All(r => r.RoomId != section.RoomId))
                return (false, "Phòng học không tồn tại.");

            if (_store.Lecturers.All(l => l.LecturerId != section.LecturerId))
                return (false, "Giảng viên không tồn tại.");

            if (section.StartTime >= section.EndTime)
                return (false, "Giờ bắt đầu phải trước giờ kết thúc.");

            var conflicts = _conflictService.FindConflicts(section);
            if (conflicts.Count > 0)
                return (false, string.Join(" ", conflicts));

            _store.Sections.Add(section);
            return (true, null);
        }

        public (bool ok, string? error) CloseRegistration(string sectionId)
        {
            var section = _store.Sections.FirstOrDefault(s => s.SectionId == sectionId);
            if (section is null) return (false, "Không tìm thấy lớp học phần.");
            section.IsOpen = false;
            return (true, null);
        }
    }
}
