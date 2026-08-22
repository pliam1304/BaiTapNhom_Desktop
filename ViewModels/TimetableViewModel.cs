using EduPath.Avalonia.Models;
using EduPath.Avalonia.Services;

namespace EduPath.Avalonia.ViewModels
{
    public class DayColumn
    {
        public string Label { get; init; } = string.Empty;
        public List<SectionRow> Sections { get; init; } = new();
        public bool HasClasses => Sections.Count > 0;
    }

    public class TimetableViewModel : ViewModelBase
    {
        private readonly EnrollmentService _enrollSvc = new();

        public List<DayColumn> Days { get; } = new();

        public TimetableViewModel(Student student)
        {
            var active = _enrollSvc.GetActiveEnrollments(student.StudentId)
                .Select(e => _enrollSvc.GetSection(e.SectionId))
                .Where(s => s != null)
                .Select(s => new SectionRow(s!, _enrollSvc.GetCourse(s!.CourseCode), _enrollSvc.GetLecturer(s!.LecturerId)))
                .ToList();

            for (int day = 2; day <= 8; day++)
            {
                var label = day == 8 ? "Chủ nhật" : $"Thứ {day}";
                var sections = active.Where(r => r.Section.DayOfWeek == day)
                    .OrderBy(r => r.Section.StartTime)
                    .ToList();
                Days.Add(new DayColumn { Label = label, Sections = sections });
            }
        }
    }
}
