using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using EduPath.Avalonia.Data;
using EduPath.Avalonia.Models;

namespace EduPath.Avalonia.ViewModels
{
    public class OpenSectionsViewModel
        : ViewModelBase, IRefreshable
    {
        private readonly InMemoryStore _store =
            InMemoryStore.Instance;


        // =====================================================
        // STUDENT
        // =====================================================

        public Student CurrentStudent { get; }

        public StudentShellViewModel Shell { get; }


        // =====================================================
        // ALL SECTIONS
        // =====================================================

        private readonly List<SectionRow> _allSections =
            new();


        // =====================================================
        // DISPLAYED SECTIONS
        // =====================================================

        public ObservableCollection<SectionRow> Sections { get; }
            = new();

        public ObservableCollection<SectionRow> SelectedSections { get; }
            = new();

        public int SelectedCredits => SelectedSections.Sum(s => s.Credits);

        public int MaxCredits => CurrentStudent.MaxCreditsPerTerm;

        public double SelectedProgress => MaxCredits == 0
            ? 0
            : Math.Min(100, (SelectedCredits / (double)MaxCredits) * 100);

        public string SelectedSummaryText => $"{SelectedCredits} / {MaxCredits} tín chỉ";

        public bool HasSelectedSections => SelectedSections.Count > 0;


        // =====================================================
        // ADMIN
        // =====================================================

        public bool IsAdminRole => false;


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
                    FilterSections();
                }
            }
        }


        // =====================================================
        // FEEDBACK
        // =====================================================

        private string _feedback = string.Empty;

        public string Feedback
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
            !string.IsNullOrWhiteSpace(
                Feedback
            );


        private string _feedbackClass = string.Empty;

        public string FeedbackClass
        {
            get => _feedbackClass;
            set => SetProperty(ref _feedbackClass, value);
        }


        // =====================================================
        // NO RESULTS
        // =====================================================

        public bool NoResults =>
            Sections.Count == 0;


        // =====================================================
        // COMMAND
        // =====================================================

        public RelayCommand CloseDetailCommand { get; }

        public RelayCommand<SectionRow> RegisterCommand { get; }

        public RelayCommand<SectionRow> RemoveSelectedCommand { get; }

        public RelayCommand SubmitRegistrationCommand { get; }


        // =====================================================
        // CONSTRUCTOR
        // =====================================================

        public OpenSectionsViewModel(
            Student student,
            StudentShellViewModel shell)
        {
            CurrentStudent = student;

            Shell = shell;

            CloseDetailCommand =
                new RelayCommand(
                    () =>
                    {
                        SelectedSection = null;
                    }
                );

            RegisterCommand =
                new RelayCommand<SectionRow>(
                    RegisterSection
                );

            RemoveSelectedCommand =
                new RelayCommand<SectionRow>(
                    RemoveSelectedSection
                );

            SubmitRegistrationCommand =
                new RelayCommand(
                    SubmitRegistration
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
        // LOAD
        // =====================================================

        private void Load()
        {
            Console.WriteLine(
                "======================================"
            );

            Console.WriteLine(
                "[OpenSections] Bắt đầu Load()"
            );

            Sections.Clear();

            _allSections.Clear();

            SelectedSection = null;


            // =================================================
            // ĐỌC DATA TỪ STORE
            // =================================================

            int storeCount =
                _store.Sections.Count;

            Console.WriteLine(
                $"[OpenSections] Store.Sections = {storeCount}"
            );


            for (int i = 0;
                 i < _store.Sections.Count;
                 i++)
            {
                Section section =
                    _store.Sections[i];

                if (!section.IsOpen)
                {
                    continue;
                }


                Course? course = null;

                for (int j = 0;
                     j < _store.Courses.Count;
                     j++)
                {
                    if (_store.Courses[j].CourseCode ==
                        section.CourseCode)
                    {
                        course =
                            _store.Courses[j];

                        break;
                    }
                }


                Lecturer? lecturer = null;

                for (int j = 0;
                     j < _store.Lecturers.Count;
                     j++)
                {
                    if (_store.Lecturers[j].LecturerId ==
                        section.LecturerId)
                    {
                        lecturer =
                            _store.Lecturers[j];

                        break;
                    }
                }


                SectionRow row =
                    new SectionRow(
                        section,
                        course,
                        lecturer
                    );

                _allSections.Add(row);
            }


            // =================================================
            // NẾU STORE KHÔNG CÓ DATA
            // TẠO DATA TEST
            // =================================================

            if (_allSections.Count == 0)
            {
                Console.WriteLine(
                    "[OpenSections] Không có data."
                );

                Console.WriteLine(
                    "[OpenSections] Tạo fake data."
                );


                Course fakeCourse =
                    new Course
                    {
                        CourseCode = "CT101",

                        CourseName =
                            "Cấu trúc dữ liệu và giải thuật",

                        Credits = 3
                    };


                Lecturer fakeLecturer =
                    new Lecturer
                    {
                        LecturerId = "GV01",

                        FullName =
                            "Trần Thế Anh"
                    };


                Section fakeSection =
                    new Section
                    {
                        SectionId = "CT101.01",

                        CourseCode = "CT101",

                        LecturerId = "GV01",

                        Capacity = 50,

                        Enrolled = 45,

                        Term = "HK1",

                        IsOpen = true
                    };


                SectionRow fakeRow =
                    new SectionRow(
                        fakeSection,
                        fakeCourse,
                        fakeLecturer
                    );


                _allSections.Add(
                    fakeRow
                );
            }


            // =================================================
            // HIỂN THỊ
            // =================================================

            FilterSections();

            if (SelectedSections.Count == 0)
            {
                for (int i = 0; i < Math.Min(3, Sections.Count); i++)
                {
                    SectionRow row = Sections[i];

                    if (!SelectedSections.Any(s => s.SectionId == row.SectionId))
                    {
                        SelectedSections.Add(row);
                    }
                }
            }

            RaisePropertyChanged(nameof(SelectedSections));
            RaisePropertyChanged(nameof(SelectedCredits));
            RaisePropertyChanged(nameof(SelectedProgress));
            RaisePropertyChanged(nameof(SelectedSummaryText));
            RaisePropertyChanged(nameof(HasSelectedSections));

            Console.WriteLine(
                $"[OpenSections] " +
                $"AllSections = {_allSections.Count}"
            );

            Console.WriteLine(
                $"[OpenSections] " +
                $"Sections = {Sections.Count}"
            );

            Console.WriteLine(
                "======================================"
            );
        }


        // =====================================================
        // FILTER
        // =====================================================

        private void FilterSections()
        {
            Sections.Clear();


            string keyword =
                SearchText.Trim();


            for (int i = 0;
                 i < _allSections.Count;
                 i++)
            {
                SectionRow row =
                    _allSections[i];


                if (string.IsNullOrWhiteSpace(
                    keyword))
                {
                    Sections.Add(row);

                    continue;
                }


                string courseName =
                    row.CourseName ?? string.Empty;

                string courseCode =
                    row.CourseCode ?? string.Empty;


                bool matched =
                    courseName.Contains(
                        keyword,
                        StringComparison.OrdinalIgnoreCase
                    )
                    ||
                    courseCode.Contains(
                        keyword,
                        StringComparison.OrdinalIgnoreCase
                    );


                if (matched)
                {
                    Sections.Add(row);
                }
            }


            RaisePropertyChanged(
                nameof(NoResults)
            );
        }


        // =====================================================
        // REGISTER
        // =====================================================

        private void RegisterSection(
            SectionRow? row)
        {
            if (row == null)
            {
                return;
            }


            Console.WriteLine(
                $"[OpenSections] " +
                $"Sinh viên: " +
                $"{CurrentStudent.FullName}"
            );

            Console.WriteLine(
                $"[OpenSections] " +
                $"Đăng ký lớp: " +
                $"{row.SectionId}"
            );


            if (row.IsFull)
            {
                Feedback =
                    $"Lớp {row.SectionId} đã đầy.";

                return;
            }


            if (SelectedSections.Any(s => s.SectionId == row.SectionId))
            {
                Feedback =
                    $"Lớp {row.SectionId} đã có trong danh sách chọn.";
                FeedbackClass = "feedback-error";
                RaisePropertyChanged(nameof(HasFeedback));
                return;
            }

            SelectedSections.Add(row);

            Feedback =
                $"Đã thêm lớp {row.SectionId} vào danh sách đăng ký.";
            FeedbackClass = "feedback-ok";

            RaisePropertyChanged(nameof(SelectedSections));
            RaisePropertyChanged(nameof(SelectedCredits));
            RaisePropertyChanged(nameof(SelectedProgress));
            RaisePropertyChanged(nameof(SelectedSummaryText));
            RaisePropertyChanged(nameof(HasSelectedSections));
            RaisePropertyChanged(nameof(HasFeedback));
        }

        private void RemoveSelectedSection(SectionRow? row)
        {
            if (row == null)
            {
                return;
            }

            SelectedSections.Remove(row);

            Feedback = $"Đã bỏ chọn lớp {row.SectionId}.";
            FeedbackClass = "feedback-ok";

            RaisePropertyChanged(nameof(SelectedSections));
            RaisePropertyChanged(nameof(SelectedCredits));
            RaisePropertyChanged(nameof(SelectedProgress));
            RaisePropertyChanged(nameof(SelectedSummaryText));
            RaisePropertyChanged(nameof(HasSelectedSections));
            RaisePropertyChanged(nameof(HasFeedback));
        }

        private void SubmitRegistration()
        {
            if (SelectedSections.Count == 0)
            {
                Feedback = "Bạn chưa chọn lớp học phần nào.";
                FeedbackClass = "feedback-error";
                RaisePropertyChanged(nameof(HasFeedback));
                return;
            }

            Feedback = $"Đã gửi đăng ký {SelectedSections.Count} lớp học phần thành công.";
            FeedbackClass = "feedback-ok";
            RaisePropertyChanged(nameof(HasFeedback));
        }
    }
}