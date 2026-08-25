using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using EduPath.Avalonia.Data;
using EduPath.Avalonia.Models;
using EduPath.Avalonia.Services;

namespace EduPath.Avalonia.ViewModels
{
    public class SectionsAdminViewModel : ViewModelBase, IRefreshable
    {
        private readonly InMemoryStore _store = InMemoryStore.Instance;
        private readonly SectionService _sectionSvc = new();

        // Danh sách chính hiển thị trên bảng
        public ObservableCollection<SectionRow> Sections { get; } = new();
        public bool IsAdminRole { get; } = true; 

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

        // ========================================================
        // 1. TÍNH NĂNG MASTER-DETAIL (XEM CHI TIẾT) & PHÂN QUYỀN
        // ========================================================
        
        // Giả lập quyền: True = Admin (Toàn quyền), False = Sinh viên (Chỉ xem)
        // Thực tế sau này bạn truyền biến này từ lúc Đăng nhập vào
        //public bool IsAdminRole { get; } = true; 

        // private SectionRow? _selectedSection;
        // public SectionRow? SelectedSection
        // {
        //     get => _selectedSection;
        //     set 
        //     { 
        //         if (SetProperty(ref _selectedSection, value))
        //         {
        //             // Khi có dòng được chọn, tự động báo XAML hiện bảng chi tiết
        //             RaisePropertyChanged(nameof(IsDetailVisible));
        //         }
        //     }
        // }

        // Cờ điều khiển ẩn/hiện khối chi tiết phía dưới
       // public bool IsDetailVisible => SelectedSection != null;

        //public RelayCommand CloseDetailCommand { get; }


        // ========================================================
        // 2. CÁC BIẾN & COMMAND CỦA FORM TẠO MỚI (Giữ nguyên của bạn)
        // ========================================================
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
            // Khởi tạo lệnh đóng bảng chi tiết
            CloseDetailCommand = new RelayCommand(() => SelectedSection = null);

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
            SelectedSection = null; // Reset bảng chi tiết khi load lại

            // Load dữ liệu thật từ Store
            var realSections = _store.Sections.OrderBy(s => s.SectionId).ToList();

            if (realSections.Any())
            {
                foreach (var s in realSections)
                {
                    Sections.Add(new SectionRow(s,
                        _store.Courses.FirstOrDefault(c => c.CourseCode == s.CourseCode),
                        _store.Lecturers.FirstOrDefault(l => l.LecturerId == s.LecturerId)));
                }
            }
            else
            {
                // ==========================================================
                // 3. DỮ LIỆU CỨNG TẠM THỜI (Chạy khi Store chưa có dữ liệu)
                // ==========================================================
                
                // Môn học ảo
                var fakeCourse1 = new Course { CourseCode = "CT101", CourseName = "Cấu trúc dữ liệu và giải thuật", Credits = 3 };
                var fakeCourse2 = new Course { CourseCode = "CSDL101", CourseName = "Cơ sở dữ liệu", Credits = 4 };
                
                // Giảng viên ảo
                var fakeLec1 = new Lecturer { LecturerId = "GV01", FullName = "Trần Thế Anh" };
                var fakeLec2 = new Lecturer { LecturerId = "GV02", FullName = "Nguyễn Minh Thu" };

                // Tạo đối tượng SectionRow ép cứng vào danh sách
                Sections.Add(new SectionRow(
                    new Section { SectionId = "CT101.01", CourseCode = "CT101", LecturerId = "GV01", Capacity = 50, Enrolled = 45, Term = "HK1", IsOpen = true },
                    fakeCourse1, fakeLec1));

                Sections.Add(new SectionRow(
                    new Section { SectionId = "CSDL101.02", CourseCode = "CSDL101", LecturerId = "GV02", Capacity = 50, Enrolled = 48, Term = "HK1", IsOpen = true },
                    fakeCourse2, fakeLec2));
            }
        }

        private void SubmitForm()
        {
            // ... (Giữ nguyên code SubmitForm của bạn) ...
        }

        private void CloseRegistration(SectionRow? row)
        {
            // ... (Giữ nguyên code CloseRegistration của bạn) ...
        }
    }
}