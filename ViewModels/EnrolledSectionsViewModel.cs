using System.Collections.ObjectModel;
using EduPath.Avalonia.Models;
using EduPath.Avalonia.Services;

namespace EduPath.Avalonia.ViewModels
{
    public class EnrolledSectionsViewModel : ViewModelBase, IRefreshable
    {
        private readonly Student _student;
        private readonly StudentShellViewModel _shell;
        private readonly EnrollmentService _enrollSvc = new();

        public ObservableCollection<SectionRow> Sections { get; } = new();
        public RelayCommand<SectionRow> CancelCommand { get; }

        private string? _feedback;
        public string? Feedback { get => _feedback; set { SetProperty(ref _feedback, value); RaisePropertyChanged(nameof(HasFeedback)); } }
        public bool HasFeedback => !string.IsNullOrEmpty(Feedback);

        public int TotalCredits { get; private set; }
        public bool NoResults => Sections.Count == 0;

        public EnrolledSectionsViewModel(Student student, StudentShellViewModel shell)
        {
            _student = student;
            _shell = shell;
            CancelCommand = new RelayCommand<SectionRow>(Cancel);
            Load();
        }

        public void Refresh() => Load();

        private void Load()
        {
            Sections.Clear();
            foreach (var e in _enrollSvc.GetActiveEnrollments(_student.StudentId))
            {
                var s = _enrollSvc.GetSection(e.SectionId);
                if (s is null) continue;
                Sections.Add(new SectionRow(s, _enrollSvc.GetCourse(s.CourseCode), _enrollSvc.GetLecturer(s.LecturerId)));
            }
            TotalCredits = _enrollSvc.GetTotalRegisteredCredits(_student.StudentId);
            RaisePropertyChanged(nameof(TotalCredits));
            RaisePropertyChanged(nameof(NoResults));
        }

        private void Cancel(SectionRow? row)
        {
            if (row is null) return;
            var (ok, error) = _enrollSvc.Cancel(_student.StudentId, row.SectionId);
            Feedback = ok ? $"Đã hủy đăng ký lớp {row.SectionId}." : error;
            if (ok)
            {
                _shell.InvalidateAll();
                Load();
            }
        }
    }
}
