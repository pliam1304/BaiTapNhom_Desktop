using EduPath.Avalonia.Data;
using EduPath.Avalonia.Models;

namespace EduPath.Avalonia.Services
{
    public class EnrollmentCheckResult
    {
        public bool CanRegister { get; init; }
        public List<string> Reasons { get; init; } = new();
        public static EnrollmentCheckResult Pass() => new() { CanRegister = true };
        public static EnrollmentCheckResult Fail(params string[] reasons) =>
            new() { CanRegister = false, Reasons = reasons.ToList() };
    }

    /// <summary>
    /// Toàn bộ nghiệp vụ đăng ký/hủy học phần cho sinh viên (màn 02-08). Đây là phần logic mà
    /// bản mockup HTML gốc chỉ hiển thị dòng chữ ghi chú ("Hệ thống sẽ kiểm tra tiên quyết,
    /// trùng lịch và sĩ số...") chứ chưa thật sự kiểm tra — service này hiện thực hóa nó.
    /// </summary>
    public class EnrollmentService
    {
        private readonly InMemoryStore _store = InMemoryStore.Instance;

        public IReadOnlyList<Section> GetOpenSections(string term) =>
            _store.Sections.Where(s => s.Term == term && s.IsOpen).ToList();

        public IEnumerable<Enrollment> GetActiveEnrollments(string studentId) =>
            _store.Enrollments.Where(e => e.StudentId == studentId && e.Status == EnrollmentStatus.Enrolled);

        public int GetTotalRegisteredCredits(string studentId)
        {
            var sectionIds = GetActiveEnrollments(studentId).Select(e => e.SectionId).ToHashSet();
            return _store.Sections
                .Where(s => sectionIds.Contains(s.SectionId))
                .Join(_store.Courses, s => s.CourseCode, c => c.CourseCode, (s, c) => c.Credits)
                .Sum();
        }

        /// <summary>
        /// Kiểm tra đầy đủ điều kiện trước khi đăng ký (được gọi trước khi hiện modal "Xác nhận đăng ký" - màn 05).
        /// Thứ tự kiểm tra: đợt đăng ký mở -> đã đăng ký chưa -> lớp còn mở -> còn chỗ -> tiên quyết -> trùng lịch -> vượt tín chỉ tối đa.
        /// </summary>
        public EnrollmentCheckResult CanRegister(Student student, Section section)
        {
            var reasons = new List<string>();

            var period = _store.Periods.FirstOrDefault(p => p.Term == section.Term);
            if (period is null || !period.IsCurrentlyOpen(DateTime.Now))
                reasons.Add("Đợt đăng ký học phần hiện không mở.");

            bool alreadyEnrolled = GetActiveEnrollments(student.StudentId).Any(e => e.SectionId == section.SectionId);
            if (alreadyEnrolled)
                reasons.Add("Bạn đã đăng ký lớp học phần này.");

            if (!section.IsOpen)
                reasons.Add("Lớp học phần hiện không mở đăng ký.");

            if (section.Remaining <= 0)
                reasons.Add("Lớp học phần đã đầy sĩ số.");

            var course = _store.Courses.FirstOrDefault(c => c.CourseCode == section.CourseCode);
            if (course?.PrerequisiteCode != null && !student.CompletedCourseCodes.Contains(course.PrerequisiteCode))
                reasons.Add($"Chưa hoàn thành học phần tiên quyết '{course.PrerequisiteCode}'.");

            var registeredSectionIds = GetActiveEnrollments(student.StudentId).Select(e => e.SectionId).ToHashSet();
            var registeredSections = _store.Sections.Where(s => registeredSectionIds.Contains(s.SectionId));
            var clashing = registeredSections.FirstOrDefault(s => s.TimeOverlaps(section));
            if (clashing != null)
                reasons.Add($"Trùng lịch với lớp {clashing.SectionId} ({clashing.DayLabel} {clashing.TimeLabel}).");

            if (course != null)
            {
                int currentCredits = GetTotalRegisteredCredits(student.StudentId);
                if (currentCredits + course.Credits > student.MaxCreditsPerTerm)
                    reasons.Add($"Vượt quá số tín chỉ tối đa cho phép ({student.MaxCreditsPerTerm} TC).");
            }

            return reasons.Count == 0 ? EnrollmentCheckResult.Pass() : EnrollmentCheckResult.Fail(reasons.ToArray());
        }

        /// <summary>Thực hiện đăng ký sau khi người dùng xác nhận ở modal (màn 05). Gọi lại CanRegister để tránh race-condition.</summary>
        public (bool ok, string? error) Register(Student student, Section section)
        {
            var check = CanRegister(student, section);
            if (!check.CanRegister)
                return (false, string.Join(" ", check.Reasons));

            section.Enrolled++;
            _store.Enrollments.Add(new Enrollment
            {
                StudentId = student.StudentId,
                SectionId = section.SectionId,
                RegisteredAt = DateTime.Now,
                Status = EnrollmentStatus.Enrolled
            });
            return (true, null);
        }

        /// <summary>Hủy đăng ký (dùng ở màn 06 · Học phần đã đăng ký).</summary>
        public (bool ok, string? error) Cancel(string studentId, string sectionId)
        {
            var enrollment = _store.Enrollments.FirstOrDefault(e =>
                e.StudentId == studentId && e.SectionId == sectionId && e.Status == EnrollmentStatus.Enrolled);

            if (enrollment is null)
                return (false, "Không tìm thấy lượt đăng ký để hủy.");

            var period = _store.Periods.FirstOrDefault(p => p.Term ==
                _store.Sections.First(s => s.SectionId == sectionId).Term);
            if (period != null && !period.IsCurrentlyOpen(DateTime.Now))
                return (false, "Đợt đăng ký đã đóng, không thể hủy học phần.");

            enrollment.Status = EnrollmentStatus.Cancelled;
            var section = _store.Sections.FirstOrDefault(s => s.SectionId == sectionId);
            if (section != null && section.Enrolled > 0) section.Enrolled--;

            return (true, null);
        }

        /// <summary>Toàn bộ lịch sử đăng ký (kể cả đã hủy) cho màn 08.</summary>
        public IReadOnlyList<Enrollment> GetHistory(string studentId) =>
            _store.Enrollments.Where(e => e.StudentId == studentId)
                .OrderByDescending(e => e.RegisteredAt).ToList();

        public Course? GetCourse(string code) => _store.Courses.FirstOrDefault(c => c.CourseCode == code);
        public Lecturer? GetLecturer(string id) => _store.Lecturers.FirstOrDefault(l => l.LecturerId == id);
        public Section? GetSection(string id) => _store.Sections.FirstOrDefault(s => s.SectionId == id);
    }
}
