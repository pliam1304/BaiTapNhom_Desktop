using System.Collections.ObjectModel;
using System.Linq;
using EduPath.Avalonia.Data;
using EduPath.Avalonia.Models;

namespace EduPath.Avalonia.ViewModels
{
    public class OpenSectionsViewModel : ViewModelBase, IRefreshable
    {
        private readonly InMemoryStore _store = InMemoryStore.Instance;
        
        // Nhận thông tin sinh viên đang đăng nhập từ Shell
        public Student CurrentStudent { get; }
        public StudentShellViewModel Shell { get; }

        public ObservableCollection<SectionRow> Sections { get; } = new();

        // Ép cứng quyền Admin = false để XAML tự động ẩn các nút chỉnh sửa/xóa
        public bool IsAdminRole => false; 

        private SectionRow? _selectedSection;
        public SectionRow? SelectedSection
        {
            get => _selectedSection;
            set 
            { 
                if (SetProperty(ref _selectedSection, value))
                    RaisePropertyChanged(nameof(IsDetailVisible));
            }
        }
        public bool IsDetailVisible => SelectedSection != null;

        public RelayCommand CloseDetailCommand { get; }
        
        // Lệnh dành riêng cho sinh viên
        public RelayCommand<SectionRow> RegisterCommand { get; } 

        public OpenSectionsViewModel(Student student, StudentShellViewModel shell)
        {
            CurrentStudent = student;
            Shell = shell;
            
            CloseDetailCommand = new RelayCommand(() => SelectedSection = null);
            RegisterCommand = new RelayCommand<SectionRow>(RegisterSection);
            
            Load();
        }

        public void Refresh() => Load();

        private void Load()
        {
            Sections.Clear();
            SelectedSection = null;

            // Chế độ Sinh viên: Chỉ hiển thị các lớp ĐANG MỞ (IsOpen = true)
            var openSections = _store.Sections.Where(s => s.IsOpen).OrderBy(s => s.SectionId).ToList();

            if (openSections.Any())
            {
                foreach (var s in openSections)
                {
                    Sections.Add(new SectionRow(s,
                        _store.Courses.FirstOrDefault(c => c.CourseCode == s.CourseCode),
                        _store.Lecturers.FirstOrDefault(l => l.LecturerId == s.LecturerId)));
                }
            }
            else
            {
                // DỮ LIỆU CỨNG tạm thời để test giao diện
                var fakeCourse = new Course { CourseCode = "CT101", CourseName = "Cấu trúc dữ liệu và giải thuật", Credits = 3 };
                var fakeLec = new Lecturer { LecturerId = "GV01", FullName = "Trần Thế Anh" };
                
                Sections.Add(new SectionRow(
                    new Section { SectionId = "CT101.01", CourseCode = "CT101", LecturerId = "GV01", Capacity = 50, Enrolled = 45, Term = "HK1", IsOpen = true },
                    fakeCourse, fakeLec));
            }
        }

        private void RegisterSection(SectionRow? row)
        {
            if (row == null) return;
            
            // TODO: Xử lý logic Đăng ký môn học tại đây sau khi bạn nối Database (gọi Service, kiểm tra trùng lịch...)
            // Tạm thời in ra màn hình console để test
            System.Console.WriteLine($"Sinh viên {CurrentStudent.FullName} yêu cầu đăng ký lớp: {row.SectionId}");
        }
    }
}