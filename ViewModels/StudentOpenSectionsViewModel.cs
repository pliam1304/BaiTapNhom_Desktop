// ViewModels/StudentOpenSectionsViewModel.cs

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using EduPath.Avalonia.Models;
using EduPath.Avalonia.Services;

namespace EduPath.Avalonia.ViewModels
{
    public class StudentOpenSectionsViewModel
        : ViewModelBase, IRefreshable
    {
        private readonly Student _student;

        private readonly StudentShellViewModel _shell;

        private readonly EnrollmentService _enrollSvc =
            new();

        private readonly RegistrationPeriodService _periodSvc =
            new();


        // =====================================================
        // SECTIONS
        // =====================================================

        public ObservableCollection<SectionRow> Sections { get; }
            = new();


        // =====================================================
        // SEARCH
        // =====================================================

        private string _searchText = string.Empty;

        public string SearchText
        {
            get => _searchText;

            set
            {
                if (SetProperty(
                    ref _searchText,
                    value))
                {
                    Load();
                }
            }
        }


        // =====================================================
        // FEEDBACK
        // =====================================================

        private string? _feedback;

        public string? Feedback
        {
            get => _feedback;

            set
            {
                if (SetProperty(
                    ref _feedback,
                    value))
                {
                    RaisePropertyChanged(
                        nameof(HasFeedback)
                    );
                }
            }
        }


        public bool HasFeedback =>
            !string.IsNullOrEmpty(
                Feedback
            );


        // =====================================================
        // FEEDBACK TYPE
        // =====================================================

        private bool _feedbackIsError;

        public bool FeedbackIsError
        {
            get => _feedbackIsError;

            set
            {
                if (SetProperty(
                    ref _feedbackIsError,
                    value))
                {
                    RaisePropertyChanged(
                        nameof(FeedbackClass)
                    );
                }
            }
        }


        public string FeedbackClass =>
            FeedbackIsError
                ? "feedback-error"
                : "feedback-ok";


        // =====================================================
        // NO RESULTS
        // =====================================================

        private bool _noResults;

        public bool NoResults
        {
            get => _noResults;

            set => SetProperty(
                ref _noResults,
                value
            );
        }


        // =====================================================
        // SELECTED SECTION
        // =====================================================

        private SectionRow? _selectedSection;

        public SectionRow? SelectedSection
        {
            get => _selectedSection;

            set
            {
                if (SetProperty(
                    ref _selectedSection,
                    value))
                {
                    RaisePropertyChanged(
                        nameof(IsDetailVisible)
                    );
                }
            }
        }


        public bool IsDetailVisible =>
            SelectedSection != null;


        // =====================================================
        // COMMAND
        // =====================================================

        public RelayCommand<SectionRow> RegisterCommand
            { get; }


        // =====================================================
        // CONSTRUCTOR
        // =====================================================

        public StudentOpenSectionsViewModel(
            Student student,
            StudentShellViewModel shell)
        {
            _student = student;

            _shell = shell;

            RegisterCommand =
                new RelayCommand<SectionRow>(
                    Register
                );

            Load();
        }


        // =====================================================
        // REFRESH
        // =====================================================

        public void Refresh()
        {
            Load();
        }


        // =====================================================
        // CLOSE DETAIL
        // =====================================================

        public void CloseDetail()
        {
            SelectedSection = null;
        }


        // =====================================================
        // LOAD
        // =====================================================

    private void Load()
{
    Console.WriteLine("========================================");
    Console.WriteLine("[StudentOpenSections] Load()");

    var period = _periodSvc.GetCurrent();
    var term = period?.Term ?? "HK1 2026-2027";

    Console.WriteLine($"[StudentOpenSections] Term: {term}");

    // =================================================
    // LẤY TRỰC TIẾP TỪ InMemoryStore (test)
    // =================================================
    var store = EduPath.Avalonia.Data.InMemoryStore.Instance;

    var query = store.Sections
        .Where(s => s.Term == term && s.IsOpen)
        .Select(s => new SectionRow(
            s,
            store.Courses.FirstOrDefault(c => c.CourseCode == s.CourseCode),
            store.Lecturers.FirstOrDefault(l => l.LecturerId == s.LecturerId)
        ))
        .ToList();

    Console.WriteLine($"[StudentOpenSections] Data thật: {query.Count}");

    // Nếu vẫn = 0 thì in thêm để debug
    if (query.Count == 0)
    {
        Console.WriteLine($"[DEBUG] Tổng Section trong store: {store.Sections.Count}");
        Console.WriteLine($"[DEBUG] Các Term có trong store: {string.Join(", ", store.Sections.Select(s => s.Term).Distinct())}");
    }

    // =================================================
    // SEARCH
    // =================================================
    if (!string.IsNullOrWhiteSpace(SearchText))
    {
        var kw = SearchText.Trim();

        query = query
            .Where(r =>
                r.CourseName.Contains(kw, StringComparison.OrdinalIgnoreCase) ||
                r.CourseCode.Contains(kw, StringComparison.OrdinalIgnoreCase) ||
                r.SectionId.Contains(kw, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    // =================================================
    // UPDATE COLLECTION
    // =================================================
    Sections.Clear();

    foreach (var row in query)
        Sections.Add(row);

    NoResults = Sections.Count == 0;

    Console.WriteLine($"[StudentOpenSections] Sections.Count = {Sections.Count}");
    Console.WriteLine("========================================");
}

        // =====================================================
        // REGISTER
        // =====================================================

        private void Register(
            SectionRow? row)
        {
            if (row is null)
            {
                return;
            }


            Console.WriteLine(
                $"[StudentOpenSections] " +
                $"Đăng ký: {row.SectionId}"
            );


            // =================================================
            // GỌI SERVICE THẬT
            // =================================================

            var (ok, error) =
                _enrollSvc.Register(
                    _student,
                    row.Section
                );


            FeedbackIsError = !ok;


            Feedback =
                ok
                    ? $"Đã đăng ký thành công lớp " +
                      $"{row.SectionId} " +
                      $"({row.CourseName})."

                    : error;


            // =================================================
            // ĐĂNG KÝ THÀNH CÔNG
            // =================================================

            if (ok)
            {
                _shell.InvalidateAll();

                Load();
            }
        }
    }


    // =========================================================
    // GENERIC RELAY COMMAND
    // =========================================================

    public class RelayCommand<T>
        : System.Windows.Input.ICommand
    {
        private readonly Action<T?> _execute;

        private readonly Func<T?, bool>? _canExecute;


        public RelayCommand(
            Action<T?> execute,
            Func<T?, bool>? canExecute = null)
        {
            _execute = execute;

            _canExecute = canExecute;
        }


        public event EventHandler? CanExecuteChanged;


        public bool CanExecute(
            object? parameter)
        {
            return _canExecute?.Invoke(
                (T?)parameter
            ) ?? true;
        }


        public void Execute(
            object? parameter)
        {
            _execute(
                (T?)parameter
            );
        }


        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(
                this,
                EventArgs.Empty
            );
        }
    }


    // =========================================================
    // MOCK DATA
    // =========================================================

    public static class MockDataHelper
    {
        public static List<SectionRow>
            GetMockSectionRows()
        {
            var list =
                new List<SectionRow>();


            var subjects =
                new[]
                {
                    "Lập trình C#",
                    "Cấu trúc dữ liệu",
                    "Cơ sở dữ liệu",
                    "Mạng máy tính",
                    "Hệ điều hành",
                    "Trí tuệ nhân tạo",
                    "Phát triển Web",
                    "Phát triển Mobile",
                    "Toán rời rạc",
                    "Kỹ nghệ phần mềm"
                };


            var days =
                new[]
                {
                    "Thứ 2",
                    "Thứ 3",
                    "Thứ 4",
                    "Thứ 5",
                    "Thứ 6"
                };


            for (int i = 1; i <= 10; i++)
            {
                var course =
                    new Course
                    {
                        CourseCode =
                            $"IT{100 + i}",

                        CourseName =
                            subjects[i - 1],

                        Credits = 3
                    };


                var lecturer =
                    new Lecturer
                    {
                        LecturerId =
                            $"GV{i:D3}",

                        FullName =
                            $"Nguyễn Văn Giảng Viên {i}"
                    };


                var section =
                    new Section
                    {
                        SectionId =
                            $"IT{100 + i}-01",

                        CourseCode =
                            course.CourseCode,

                        Term =
                            "HK1 2026-2027",

                        LecturerId =
                            lecturer.LecturerId,

                        RoomId =
                            $"A1-{100 + i}",

                        DayOfWeek =
                            GetDayOfWeek(
                                days[i % 5]
                            ),

                        StartTime =
                            new TimeSpan(
                                7,
                                0,
                                0
                            ),

                        EndTime =
                            new TimeSpan(
                                9,
                                30,
                                0
                            ),

                        Capacity = 40,

                        Enrolled = i * 3,

                        IsOpen = true
                    };


                list.Add(
                    new SectionRow(
                        section,
                        course,
                        lecturer
                    )
                );
            }


            return list;
        }


        private static int GetDayOfWeek(
            string dayName)
        {
            return dayName switch
            {
                "Thứ 2" => 2,
                "Thứ 3" => 3,
                "Thứ 4" => 4,
                "Thứ 5" => 5,
                "Thứ 6" => 6,
                "Thứ 7" => 7,
                "Chủ nhật" => 8,
                _ => 2
            };
        }


        // =====================================================
        // MOCK STUDENTS
        // =====================================================

        public static List<Student>
            GetMockStudents()
        {
            var list =
                new List<Student>();


            for (int i = 1; i <= 10; i++)
            {
                list.Add(
                    new Student
                    {
                        StudentId =
                            $"SV2026{i:D3}",

                        FullName =
                            $"Sinh Viên Ảo Số {i}",

                        Email =
                            $"sv{i}@edupath.edu.vn",

                        Faculty =
                            "Công nghệ thông tin"
                    }
                );
            }


            return list;
        }


        // =====================================================
        // MOCK ADMINS
        // =====================================================

        public static List<object>
            GetMockAdmins()
        {
            var list =
                new List<object>();


            for (int i = 1; i <= 10; i++)
            {
                list.Add(
                    new
                    {
                        AdminId =
                            $"AD{i:D3}",

                        FullName =
                            $"Quản Trị Viên {i}",

                        Email =
                            $"admin{i}@edupath.edu.vn",

                        Role =
                            "Quản trị cấp cao"
                    }
                );
            }


            return list;
        }

        
    }
}