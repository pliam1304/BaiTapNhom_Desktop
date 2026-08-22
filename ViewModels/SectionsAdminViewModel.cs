using System.Collections.ObjectModel;
using EduPath.Avalonia.Data;
using EduPath.Avalonia.Models;
using EduPath.Avalonia.Services;

namespace EduPath.Avalonia.ViewModels
{
    public class SectionsAdminViewModel : ViewModelBase, IRefreshable
    {
        private readonly InMemoryStore _store = InMemoryStore.Instance;
        private readonly SectionService _sectionSvc = new();

        public ObservableCollection<SectionRow> Sections { get; } = new();

        // --- form tạo lớp học phần mới (thay cho modal FrmCreateSection) ---
        private bool _isFormOpen;
        public bool IsFormOpen { get => _isFormOpen; set => SetProperty(ref _isFormOpen, value); }

        public string NewSectionId { get; set; } = "";
        public string NewCourseCode { get; set; } = "";
        public string NewLecturerId { get; set; } = "";
        public string NewRoomId { get; set; } = "";
        public string NewTerm { get; set; } = "HK1 2026-2027";
        public decimal NewDayOfWeek { get; set; } = 2;
        public string NewStartTime { get; set; } = "09:00";
        public string NewEndTime { get; set; } = "10:30";
        public decimal NewCapacity { get; set; } = 60;

        public List<Course> AvailableCourses => _store.Courses.Where(c => c.IsActive).ToList();
        public List<Lecturer> AvailableLecturers => _store.Lecturers.Where(l => l.IsActive).ToList();
        public List<Room> AvailableRooms => _store.Rooms.Where(r => r.IsAvailable).ToList();

        private string? _formError;
        public string? FormError { get => _formError; set { SetProperty(ref _formError, value); RaisePropertyChanged(nameof(HasFormError)); } }
        public bool HasFormError => !string.IsNullOrEmpty(FormError);

        private string? _feedback;
        public string? Feedback { get => _feedback; set { SetProperty(ref _feedback, value); RaisePropertyChanged(nameof(HasFeedback)); } }
        public bool HasFeedback => !string.IsNullOrEmpty(Feedback);

        public RelayCommand OpenFormCommand { get; }
        public RelayCommand CloseFormCommand { get; }
        public RelayCommand SubmitFormCommand { get; }
        public RelayCommand<SectionRow> CloseRegistrationCommand { get; }

        public SectionsAdminViewModel()
        {
            OpenFormCommand = new RelayCommand(() => { FormError = null; IsFormOpen = true; });
            CloseFormCommand = new RelayCommand(() => IsFormOpen = false);
            SubmitFormCommand = new RelayCommand(SubmitForm);
            CloseRegistrationCommand = new RelayCommand<SectionRow>(CloseRegistration);
            Load();
        }

        public void Refresh() => Load();

        private void Load()
        {
            Sections.Clear();
            foreach (var s in _store.Sections.OrderBy(s => s.SectionId))
            {
                Sections.Add(new SectionRow(s,
                    _store.Courses.FirstOrDefault(c => c.CourseCode == s.CourseCode),
                    _store.Lecturers.FirstOrDefault(l => l.LecturerId == s.LecturerId)));
            }
        }

        private void SubmitForm()
        {
            if (!TimeSpan.TryParse(NewStartTime, out var start) || !TimeSpan.TryParse(NewEndTime, out var end))
            {
                FormError = "Giờ bắt đầu/kết thúc không hợp lệ (định dạng HH:mm).";
                return;
            }

            var section = new Section
            {
                SectionId = NewSectionId.Trim(),
                CourseCode = NewCourseCode,
                Term = NewTerm.Trim(),
                LecturerId = NewLecturerId,
                RoomId = NewRoomId,
                DayOfWeek = (int)NewDayOfWeek,
                StartTime = start,
                EndTime = end,
                Capacity = (int)NewCapacity,
                Enrolled = 0,
                IsOpen = true
            };

            var (ok, error) = _sectionSvc.Create(section);
            if (!ok)
            {
                FormError = error;
                return;
            }

            Feedback = $"Đã tạo lớp học phần {section.SectionId}.";
            IsFormOpen = false;
            NewSectionId = "";
            Load();
        }

        private void CloseRegistration(SectionRow? row)
        {
            if (row is null) return;
            var (ok, error) = _sectionSvc.CloseRegistration(row.SectionId);
            Feedback = ok ? $"Đã đóng đăng ký lớp {row.SectionId}." : error;
            if (ok) Load();
        }
    }
}
