using EduPath.Avalonia.Models;
using EduPath.Avalonia.Services;

namespace EduPath.Avalonia.ViewModels
{
    public class HistoryRow
    {
        public Enrollment Enrollment { get; }
        public SectionRow? Section { get; }

        public HistoryRow(Enrollment enrollment, SectionRow? section)
        {
            Enrollment = enrollment;
            Section = section;
        }

        public string CourseName => Section?.CourseName ?? Enrollment.SectionId;
        public string SectionId => Enrollment.SectionId;
        public string RegisteredAt => Enrollment.RegisteredAt.ToString("dd/MM/yyyy HH:mm");
        public string StatusText => Enrollment.Status switch
        {
            EnrollmentStatus.Enrolled => "Đã đăng ký",
            EnrollmentStatus.Cancelled => "Đã hủy",
            _ => "Đang chờ"
        };
        public string StatusBadge => Enrollment.Status switch
        {
            EnrollmentStatus.Enrolled => "badge-ok",
            EnrollmentStatus.Cancelled => "badge-off",
            _ => "badge-warn"
        };
    }

    public class HistoryViewModel : ViewModelBase
    {
        private readonly EnrollmentService _enrollSvc = new();
        public List<HistoryRow> Rows { get; }

        public HistoryViewModel(Student student)
        {
            Rows = _enrollSvc.GetHistory(student.StudentId)
                .Select(e =>
                {
                    var s = _enrollSvc.GetSection(e.SectionId);
                    SectionRow? row = s is null ? null : new SectionRow(s, _enrollSvc.GetCourse(s.CourseCode), _enrollSvc.GetLecturer(s.LecturerId));
                    return new HistoryRow(e, row);
                })
                .ToList();
        }
    }
}
