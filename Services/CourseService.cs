using EduPath.WinForms.Data;
using EduPath.WinForms.Models;

namespace EduPath.WinForms.Services
{
    /// <summary>Nghiệp vụ cho màn 10 · Quản lý học phần.</summary>
    public class CourseService
    {
        private readonly InMemoryStore _store = InMemoryStore.Instance;

        public IReadOnlyList<Course> GetAll() => _store.Courses;

        public Course? GetByCode(string code) =>
            _store.Courses.FirstOrDefault(c => c.CourseCode == code);

        public (bool ok, string? error) Add(Course course)
        {
            if (string.IsNullOrWhiteSpace(course.CourseCode))
                return (false, "Mã học phần không được để trống.");
            if (_store.Courses.Any(c => c.CourseCode == course.CourseCode))
                return (false, $"Mã học phần '{course.CourseCode}' đã tồn tại.");
            if (course.PrerequisiteCode != null && GetByCode(course.PrerequisiteCode) is null)
                return (false, $"Học phần tiên quyết '{course.PrerequisiteCode}' không tồn tại.");

            _store.Courses.Add(course);
            return (true, null);
        }

        public (bool ok, string? error) Deactivate(string code)
        {
            var course = GetByCode(code);
            if (course is null) return (false, "Không tìm thấy học phần.");

            bool hasOpenSections = _store.Sections.Any(s => s.CourseCode == code && s.IsOpen);
            if (hasOpenSections)
                return (false, "Không thể vô hiệu hóa: học phần đang có lớp mở đăng ký.");

            course.IsActive = false;
            return (true, null);
        }
    }
}
