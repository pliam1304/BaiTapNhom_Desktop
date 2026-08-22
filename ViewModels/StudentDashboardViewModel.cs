using EduPath.Avalonia.Models;
using EduPath.Avalonia.Services;

namespace EduPath.Avalonia.ViewModels
{
    public class StudentDashboardViewModel : ViewModelBase, IRefreshable
    {
        private readonly Student _student;
        private readonly StudentShellViewModel _shell;
        private readonly EnrollmentService _enrollSvc = new();
        private readonly RegistrationPeriodService _periodSvc = new();

        public string StudentName => _student.FullName;
        public string StudentMeta => $"{_student.StudentId} · Lớp {_student.ClassCode} · Khóa {_student.IntakeYear}";

        public int RegisteredCredits { get; private set; }
        public int MinCredits => _student.MinCreditsPerTerm;
        public int MaxCredits => _student.MaxCreditsPerTerm;
        public double CreditProgress => MaxCredits == 0 ? 0 : Math.Clamp((double)RegisteredCredits / MaxCredits * 100, 0, 100);
        public string CreditLabel => $"{RegisteredCredits}/{MaxCredits} tín chỉ (tối thiểu {MinCredits})";

        public int EnrolledCount { get; private set; }
        public string PeriodName { get; private set; } = "";
        public bool PeriodOpen { get; private set; }
        public string PeriodStatusText => PeriodOpen ? "Đang mở đăng ký" : "Đã đóng";
        public string PeriodBadge => PeriodOpen ? "badge-ok" : "badge-off";
        public string PeriodDateRange { get; private set; } = "";

        public List<SectionRow> RecentEnrollments { get; private set; } = new();
        public bool HasNoRecentEnrollments => RecentEnrollments.Count == 0;

        public RelayCommand GoToOpenSections { get; }
        public RelayCommand GoToTimetable { get; }

        public StudentDashboardViewModel(Student student, StudentShellViewModel shell)
        {
            _student = student;
            _shell = shell;
            GoToOpenSections = new RelayCommand(() => _shell.Navigate("open"));
            GoToTimetable = new RelayCommand(() => _shell.Navigate("timetable"));
            Load();
        }

        public void Refresh() => Load();

        private void Load()
        {
            RegisteredCredits = _enrollSvc.GetTotalRegisteredCredits(_student.StudentId);

            var active = _enrollSvc.GetActiveEnrollments(_student.StudentId).ToList();
            EnrolledCount = active.Count;

            var period = _periodSvc.GetCurrent();
            if (period != null)
            {
                PeriodName = period.Name;
                PeriodOpen = period.IsCurrentlyOpen(DateTime.Now);
                PeriodDateRange = $"{period.StartDate:dd/MM/yyyy} – {period.EndDate:dd/MM/yyyy}";
            }

            RecentEnrollments = active
                .Select(e => _enrollSvc.GetSection(e.SectionId))
                .Where(s => s != null)
                .Select(s => new SectionRow(s!, _enrollSvc.GetCourse(s!.CourseCode), _enrollSvc.GetLecturer(s!.LecturerId)))
                .Take(4)
                .ToList();

            RaisePropertyChanged(nameof(RegisteredCredits));
            RaisePropertyChanged(nameof(CreditProgress));
            RaisePropertyChanged(nameof(CreditLabel));
            RaisePropertyChanged(nameof(EnrolledCount));
            RaisePropertyChanged(nameof(PeriodName));
            RaisePropertyChanged(nameof(PeriodOpen));
            RaisePropertyChanged(nameof(PeriodStatusText));
            RaisePropertyChanged(nameof(PeriodBadge));
            RaisePropertyChanged(nameof(PeriodDateRange));
            RaisePropertyChanged(nameof(RecentEnrollments));
            RaisePropertyChanged(nameof(HasNoRecentEnrollments));
        }
    }
}
