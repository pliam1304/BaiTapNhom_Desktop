using EduPath.Avalonia.Models;
using EduPath.Avalonia.Services;
using System.Collections.Generic;
using System.Linq;

namespace EduPath.Avalonia.ViewModels  // ← namespace này phải khớp với các ViewModel khác
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
            var activeSections = _enrollSvc.GetActiveEnrollments(student.StudentId)
                .Select(e => _enrollSvc.GetSection(e.SectionId))
                .Where(s => s != null)
                .ToList();

            for (int day = 2; day <= 8; day++)
            {
                var label = day == 8 ? "Chủ nhật" : $"Thứ {day}";

                var sectionsOnDay = activeSections
                    .Where(s => s.DayOfWeek == day)
                    .OrderBy(s => s.StartTime)
                    .ToList();

                var rows = sectionsOnDay
                    .Select(s => new SectionRow(
                        s,
                        _enrollSvc.GetCourse(s.CourseCode),
                        _enrollSvc.GetLecturer(s.LecturerId)))
                    .ToList();

                Days.Add(new DayColumn { Label = label, Sections = rows });
            }
        }
    }
}