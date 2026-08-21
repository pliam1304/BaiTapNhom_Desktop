using EduPath.WinForms.Data;
using EduPath.WinForms.Models;

namespace EduPath.WinForms.Services
{
    /// <summary>
    /// Quy tắc lưu lịch học (đúng ghi chú trong mockup màn 16): không được trùng PHÒNG,
    /// GIẢNG VIÊN hoặc SINH VIÊN (qua lớp học phần) trong cùng khung giờ.
    /// Dùng chung cho màn 11 (Lớp học phần) và 16 (Lịch học) khi tạo/sửa lịch.
    /// </summary>
    public class ScheduleConflictService
    {
        private readonly InMemoryStore _store = InMemoryStore.Instance;

        /// <summary>Kiểm tra một Section (mới hoặc đang sửa) có xung đột với các Section khác không.</summary>
        public List<string> FindConflicts(Section candidate)
        {
            var conflicts = new List<string>();

            var others = _store.Sections.Where(s => s.SectionId != candidate.SectionId);

            foreach (var other in others)
            {
                if (!candidate.TimeOverlaps(other)) continue;

                if (other.RoomId == candidate.RoomId)
                    conflicts.Add($"Trùng phòng {candidate.RoomId} với lớp {other.SectionId} ({other.DayLabel} {other.TimeLabel}).");

                if (other.LecturerId == candidate.LecturerId)
                    conflicts.Add($"Trùng giảng viên với lớp {other.SectionId} ({other.DayLabel} {other.TimeLabel}).");
            }

            return conflicts;
        }

        public bool IsValid(Section candidate) => FindConflicts(candidate).Count == 0;
    }
}
