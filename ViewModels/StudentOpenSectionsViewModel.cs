using System;
using System.Collections.ObjectModel;
using System.Linq;
using EduPath.Avalonia.Models;
using EduPath.Avalonia.Services;

namespace EduPath.Avalonia.ViewModels
{
    public class StudentOpenSectionsViewModel : ViewModelBase, IRefreshable
    {
        private readonly Student _student;
        private readonly StudentShellViewModel _shell;
        private readonly EnrollmentService _enrollSvc = new();
        private readonly RegistrationPeriodService _periodSvc = new();

        public ObservableCollection<SectionRow> Sections { get; } = new();

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set { if (SetProperty(ref _searchText, value)) Load(); }
        }

        private string? _feedback;
        public string? Feedback
        {
            get => _feedback;
            set { SetProperty(ref _feedback, value); RaisePropertyChanged(nameof(HasFeedback)); }
        }
        
        public bool HasFeedback => !string.IsNullOrEmpty(Feedback);
        
        private bool _feedbackIsError;
        public bool FeedbackIsError 
        { 
            get => _feedbackIsError; 
            set { SetProperty(ref _feedbackIsError, value); RaisePropertyChanged(nameof(FeedbackClass)); } 
        }
        
        public string FeedbackClass => FeedbackIsError ? "feedback-error" : "feedback-ok";

        private bool _noResults;
        public bool NoResults 
        { 
            get => _noResults; 
            set => SetProperty(ref _noResults, value); 
        }

        private SectionRow? _selectedSection;
        public SectionRow? SelectedSection 
        { 
            get => _selectedSection; 
            set { SetProperty(ref _selectedSection, value); RaisePropertyChanged(nameof(IsDetailVisible)); } 
        }

        // Trạng thái hiển thị bảng chi tiết (ẩn nếu chưa chọn lớp nào)
        public bool IsDetailVisible => SelectedSection != null;

        public RelayCommand<SectionRow> RegisterCommand { get; }

        public StudentOpenSectionsViewModel(Student student, StudentShellViewModel shell)
        {
            _student = student;
            _shell = shell;
            RegisterCommand = new RelayCommand<SectionRow>(Register);
            Load();
        }

        public void Refresh() => Load();

        // Hàm đóng chi tiết
        public void CloseDetail()
        {
            SelectedSection = null;
        }

        private void Load()
        {
            var period = _periodSvc.GetCurrent();
            var term = period?.Term ?? "";

            var query = _enrollSvc.GetOpenSections(term)
                .Select(s => new SectionRow(s, _enrollSvc.GetCourse(s.CourseCode), _enrollSvc.GetLecturer(s.LecturerId)));

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var kw = SearchText.Trim();
                query = query.Where(r =>
                    r.CourseName.Contains(kw, StringComparison.OrdinalIgnoreCase) ||
                    r.CourseCode.Contains(kw, StringComparison.OrdinalIgnoreCase) ||
                    r.SectionId.Contains(kw, StringComparison.OrdinalIgnoreCase));
            }

            Sections.Clear();
            foreach (var row in query) Sections.Add(row);
            NoResults = Sections.Count == 0;
        }

        private void Register(SectionRow? row)
        {
            if (row is null) return;

            var (ok, error) = _enrollSvc.Register(_student, row.Section);
            FeedbackIsError = !ok;
            Feedback = ok
                ? $"Đã đăng ký thành công lớp {row.SectionId} ({row.CourseName})."
                : error;

            if (ok)
            {
                _shell.InvalidateAll();
                Load();
            }
        }
    }

    /// <summary>RelayCommand có tham số kiểu generic — tiện cho binding CommandParameter trong DataTemplate.</summary>
    public class RelayCommand<T> : System.Windows.Input.ICommand
    {
        private readonly Action<T?> _execute;
        private readonly Func<T?, bool>? _canExecute;

        public RelayCommand(Action<T?> execute, Func<T?, bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;
        public bool CanExecute(object? parameter) => _canExecute?.Invoke((T?)parameter) ?? true;
        public void Execute(object? parameter) => _execute((T?)parameter);
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}