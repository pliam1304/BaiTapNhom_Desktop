using EduPath.Avalonia.Data;
using EduPath.Avalonia.Services;

namespace EduPath.Avalonia.ViewModels
{
    public class AdminDashboardViewModel : ViewModelBase, IRefreshable
    {
        private readonly InMemoryStore _store = InMemoryStore.Instance;
        private readonly AdminShellViewModel _shell;
        private readonly RegistrationPeriodService _periodSvc = new();

        public int TotalStudents { get; private set; }
        public int TotalCourses { get; private set; }
        public int TotalSections { get; private set; }
        public int OpenSections { get; private set; }
        public int TotalEnrollments { get; private set; }
        public string PeriodName { get; private set; } = "";
        public bool PeriodOpen { get; private set; }
        public string PeriodBadge => PeriodOpen ? "badge-ok" : "badge-off";
        public string PeriodStatusText => PeriodOpen ? "Đang mở đăng ký" : "Đã đóng";

        public List<SectionRow> NearFullSections { get; private set; } = new();

        public RelayCommand GoToSections { get; }

        public AdminDashboardViewModel(AdminShellViewModel shell)
        {
            _shell = shell;
            GoToSections = new RelayCommand(() => _shell.Navigate("sections"));
            Load();
        }

        public void Refresh() => Load();

        private void Load()
        {
            TotalStudents = _store.Students.Count;
            TotalCourses = _store.Courses.Count(c => c.IsActive);
            TotalSections = _store.Sections.Count;
            OpenSections = _store.Sections.Count(s => s.IsOpen);
            TotalEnrollments = _store.Enrollments.Count(e => e.Status == Models.EnrollmentStatus.Enrolled);

            var period = _periodSvc.GetCurrent();
            if (period != null)
            {
                PeriodName = period.Name;
                PeriodOpen = period.IsCurrentlyOpen(DateTime.Now);
            }

            NearFullSections = _store.Sections
                .Where(s => s.IsOpen)
                .OrderByDescending(s => (double)s.Enrolled / Math.Max(1, s.Capacity))
                .Take(5)
                .Select(s => new SectionRow(s,
                    _store.Courses.FirstOrDefault(c => c.CourseCode == s.CourseCode),
                    _store.Lecturers.FirstOrDefault(l => l.LecturerId == s.LecturerId)))
                .ToList();

            RaisePropertyChanged(nameof(TotalStudents));
            RaisePropertyChanged(nameof(TotalCourses));
            RaisePropertyChanged(nameof(TotalSections));
            RaisePropertyChanged(nameof(OpenSections));
            RaisePropertyChanged(nameof(TotalEnrollments));
            RaisePropertyChanged(nameof(PeriodName));
            RaisePropertyChanged(nameof(PeriodOpen));
            RaisePropertyChanged(nameof(PeriodBadge));
            RaisePropertyChanged(nameof(PeriodStatusText));
            RaisePropertyChanged(nameof(NearFullSections));
        }
    }
}
