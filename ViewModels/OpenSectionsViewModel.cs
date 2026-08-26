using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using EduPath.Avalonia.Data;
using EduPath.Avalonia.Models;
using EduPath.Avalonia.Services;

namespace EduPath.Avalonia.ViewModels
{
    public class OpenSectionsViewModel
        : ViewModelBase, IRefreshable
    {
        private readonly InMemoryStore _store =
            InMemoryStore.Instance;

        private readonly RegistrationPeriodService _periodService =
            new();

        private readonly EnrollmentService _enrollmentService =
            new();


        // =====================================================
        // STUDENT
        // =====================================================

        public Student CurrentStudent { get; }

        public StudentShellViewModel Shell { get; }


        // =====================================================
        // REGISTRATION PERIOD
        // =====================================================

        private RegistrationPeriod? _currentPeriod;

        public RegistrationPeriod? CurrentPeriod
        {
            get => _currentPeriod;
            private set
            {
                if (SetProperty(ref _currentPeriod, value))
                {
                    RaisePropertyChanged(
                        nameof(RegistrationTitle)
                    );

                    RaisePropertyChanged(
                        nameof(RegistrationStatusText)
                    );

                    RaisePropertyChanged(
                        nameof(IsRegistrationOpen)
                    );
                }
            }
        }


        public string RegistrationTitle
        {
            get
            {
                if (CurrentPeriod == null)
                {
                    return
                        "Đợt đăng ký: chưa có thông tin";
                }

                return CurrentPeriod.Name;
            }
        }


        public bool IsRegistrationOpen =>
            CurrentPeriod?.IsCurrentlyOpen(DateTime.Now)
            ?? false;


        public string RegistrationStatusText =>
            IsRegistrationOpen
                ? "ĐỢT ĐĂNG KÝ: ĐANG MỞ"
                : "ĐỢT ĐĂNG KÝ: ĐÃ ĐÓNG";


        public string RegistrationDateText
        {
            get
            {
                if (CurrentPeriod == null)
                    return "Chưa xác định thời gian";

                return
                    $"{CurrentPeriod.StartDate:dd/MM/yyyy} - " +
                    $"{CurrentPeriod.EndDate:dd/MM/yyyy}";
            }
        }


        public string StudentWelcomeText =>
            $"👋 Chào mừng trở lại, {CurrentStudent.FullName}";


        public string StudentInfoText =>
            $"{CurrentStudent.StudentId} • " +
            $"{CurrentStudent.ClassCode} • " +
            $"Khoa {CurrentStudent.Faculty}";


        // =====================================================
        // ALL SECTIONS
        // =====================================================

        private readonly List<SectionRow>
            _allSections = new();


        // =====================================================
        // DISPLAYED SECTIONS
        // =====================================================

        public ObservableCollection<SectionRow>
            RequiredSections { get; } = new();


        public ObservableCollection<SectionRow>
            ElectiveSections { get; } = new();


        // =====================================================
        // SELECTED SECTIONS
        // =====================================================

        public ObservableCollection<SectionRow>
            SelectedSections { get; } = new();


        public int SelectedCredits =>
            SelectedSections.Sum(s => s.Credits);


        public int SelectedElectiveCredits =>
            SelectedSections
                .Where(s => s.IsElective)
                .Sum(s => s.Credits);


        public int SelectedRequiredCourseCount =>
            SelectedSections
                .Where(s => s.IsRequired)
                .Select(s => s.CourseCode)
                .Distinct()
                .Count();


        public int RequiredCourseCount =>
            _allSections
                .Where(s => s.IsRequired)
                .Select(s => s.CourseCode)
                .Distinct()
                .Count();


        public bool HasSelectedSections =>
            SelectedSections.Count > 0;


        // =====================================================
        // CREDIT LIMITS
        // =====================================================

        public int MinCredits =>
            CurrentPeriod?.MinCredits ?? 0;


        public int MaxCredits =>
            CurrentPeriod?.MaxCredits
            ?? CurrentStudent.MaxCreditsPerTerm;


        public int MinElectiveCredits =>
            CurrentPeriod?.MinElectiveCredits ?? 0;


        public int MaxElectiveCredits =>
            CurrentPeriod?.MaxElectiveCredits ?? 0;


        public double SelectedProgress =>
            MaxCredits <= 0
                ? 0
                : Math.Min(
                    100,
                    SelectedCredits /
                    (double)MaxCredits * 100
                );


        public string SelectedSummaryText =>
            $"{SelectedCredits} / {MaxCredits} tín chỉ";


        public string ElectiveSummaryText =>
            $"{SelectedElectiveCredits} / " +
            $"{MinElectiveCredits} - " +
            $"{MaxElectiveCredits} TC";


        public string RequiredSummaryText =>
            $"{SelectedRequiredCourseCount} / " +
            $"{RequiredCourseCount} môn";


        // =====================================================
        // VALIDATION
        // =====================================================

        public bool IsRequiredCoursesValid =>
            SelectedRequiredCourseCount >=
            RequiredCourseCount;


        public bool IsElectiveCreditsValid =>
            SelectedElectiveCredits >=
            MinElectiveCredits
            &&
            SelectedElectiveCredits <=
            MaxElectiveCredits;


        public bool IsTotalCreditsValid =>
            SelectedCredits >= MinCredits
            &&
            SelectedCredits <= MaxCredits;


        public bool HasScheduleConflict =>
            FindScheduleConflict() != null;


        public string? ScheduleConflictMessage =>
            FindScheduleConflict();


        public bool CanSubmit =>
            IsRegistrationOpen
            &&
            SelectedSections.Count > 0
            &&
            IsRequiredCoursesValid
            &&
            IsElectiveCreditsValid
            &&
            IsTotalCreditsValid
            &&
            !HasScheduleConflict;


        public string RegistrationValidationMessage
        {
            get
            {
                if (!IsRegistrationOpen)
                {
                    return
                        "Đợt đăng ký học phần hiện đã đóng.";
                }

                if (!HasSelectedSections)
                {
                    return
                        "Bạn chưa chọn lớp học phần nào.";
                }

                if (!IsRequiredCoursesValid)
                {
                    int missing =
                        RequiredCourseCount -
                        SelectedRequiredCourseCount;

                    return
                        $"Bạn còn thiếu {missing} " +
                        $"học phần bắt buộc.";
                }

                if (SelectedElectiveCredits <
                    MinElectiveCredits)
                {
                    int missing =
                        MinElectiveCredits -
                        SelectedElectiveCredits;

                    return
                        $"Bạn cần chọn thêm ít nhất " +
                        $"{missing} tín chỉ tự chọn.";
                }

                if (SelectedElectiveCredits >
                    MaxElectiveCredits)
                {
                    return
                        $"Bạn đã vượt quá giới hạn " +
                        $"{MaxElectiveCredits} tín chỉ tự chọn.";
                }

                if (SelectedCredits < MinCredits)
                {
                    return
                        $"Tổng tín chỉ chưa đạt mức tối thiểu " +
                        $"({MinCredits} TC).";
                }

                if (SelectedCredits > MaxCredits)
                {
                    return
                        $"Bạn đã vượt quá số tín chỉ tối đa " +
                        $"({MaxCredits} TC).";
                }

                if (HasScheduleConflict)
                {
                    return ScheduleConflictMessage
                           ?? "Có lớp học bị trùng lịch.";
                }

                return
                    "Tất cả điều kiện đăng ký đã hợp lệ.";
            }
        }


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


        private string _feedbackClass =
            string.Empty;

        public string FeedbackClass
        {
            get => _feedbackClass;

            set => SetProperty(
                ref _feedbackClass,
                value
            );
        }


        // =====================================================
        // COMMANDS
        // =====================================================

        public RelayCommand<SectionRow>
            RegisterCommand { get; }


        public RelayCommand<SectionRow>
            RemoveSelectedCommand { get; }


        public RelayCommand
            ClearAllCommand { get; }


        public RelayCommand
            SubmitRegistrationCommand { get; }


        // =====================================================
        // CONSTRUCTOR
        // =====================================================

        public OpenSectionsViewModel(
            Student student,
            StudentShellViewModel shell)
        {
            CurrentStudent = student;

            Shell = shell;


            RegisterCommand =
                new RelayCommand<SectionRow>(
                    RegisterSection
                );


            RemoveSelectedCommand =
                new RelayCommand<SectionRow>(
                    RemoveSelectedSection
                );


            ClearAllCommand =
                new RelayCommand(
                    ClearAll,
                    () => SelectedSections.Count > 0
                );


            SubmitRegistrationCommand =
                new RelayCommand(
                    SubmitRegistration,
                    () => CanSubmit
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
            RequiredSections.Clear();

            ElectiveSections.Clear();

            _allSections.Clear();

            CurrentPeriod =
                _periodService.GetCurrent();


            foreach (
                Section section
                in _store.Sections)
            {
                if (!section.IsOpen)
                    continue;


                Course? course =
                    _store.Courses
                        .FirstOrDefault(
                            c =>
                                c.CourseCode ==
                                section.CourseCode
                        );


                if (course == null)
                    continue;


                Lecturer? lecturer =
                    _store.Lecturers
                        .FirstOrDefault(
                            l =>
                                l.LecturerId ==
                                section.LecturerId
                        );


                SectionRow row =
                    new SectionRow(
                        section,
                        course,
                        lecturer
                    );


                row.IsSelected =
                    SelectedSections.Any(
                        s =>
                            s.SectionId ==
                            row.SectionId
                    );


                _allSections.Add(row);
            }


            FilterSections();

            UpdateRegistrationState();
        }


        // =====================================================
        // FILTER
        // ONE SEARCH BOX FOR REQUIRED + ELECTIVE
        // =====================================================

        private void FilterSections()
        {
            RequiredSections.Clear();

            ElectiveSections.Clear();


            string keyword =
                SearchText?.Trim()
                ?? string.Empty;


            foreach (
                SectionRow row
                in _allSections)
            {
                if (!MatchesSearch(
                        row,
                        keyword))
                {
                    continue;
                }


                if (row.IsRequired)
                {
                    RequiredSections.Add(row);
                }
                else
                {
                    ElectiveSections.Add(row);
                }
            }


            RaisePropertyChanged(
                nameof(RequiredCourseCount)
            );
        }


        private static bool MatchesSearch(
            SectionRow row,
            string keyword)
        {
            if (string.IsNullOrWhiteSpace(
                keyword))
            {
                return true;
            }


            return
                row.SectionId.Contains(
                    keyword,
                    StringComparison.OrdinalIgnoreCase
                )
                ||
                row.CourseCode.Contains(
                    keyword,
                    StringComparison.OrdinalIgnoreCase
                )
                ||
                row.CourseName.Contains(
                    keyword,
                    StringComparison.OrdinalIgnoreCase
                )
                ||
                row.LecturerName.Contains(
                    keyword,
                    StringComparison.OrdinalIgnoreCase
                );
        }


        // =====================================================
        // REGISTER
        // =====================================================

        private void RegisterSection(
            SectionRow? row)
        {
            if (row == null)
                return;


            if (!IsRegistrationOpen)
            {
                SetFeedback(
                    "Đợt đăng ký hiện đã đóng.",
                    true
                );

                return;
            }


            if (!row.CanAdd)
                return;


            bool sameCourseAlreadySelected =
                SelectedSections.Any(
                    s =>
                        s.CourseCode ==
                        row.CourseCode
                );


            if (sameCourseAlreadySelected)
            {
                SetFeedback(
                    $"Bạn đã chọn một lớp khác " +
                    $"của môn {row.CourseCode}.",
                    true
                );

                return;
            }


            if (row.IsElective)
            {
                int newElectiveCredits =
                    SelectedElectiveCredits +
                    row.Credits;


                if (newElectiveCredits >
                    MaxElectiveCredits)
                {
                    SetFeedback(
                        $"Không thể thêm. " +
                        $"Số tín chỉ tự chọn tối đa là " +
                        $"{MaxElectiveCredits} TC.",
                        true
                    );

                    return;
                }
            }


            if (SelectedCredits +
                row.Credits >
                MaxCredits)
            {
                SetFeedback(
                    $"Không thể thêm vì sẽ vượt quá " +
                    $"{MaxCredits} tín chỉ.",
                    true
                );

                return;
            }


            SelectedSections.Add(row);

            row.IsSelected = true;


            string? conflict =
                FindScheduleConflict();

            if (conflict != null)
            {
                SelectedSections.Remove(row);

                row.IsSelected = false;

                SetFeedback(
                    conflict,
                    true
                );

                UpdateRegistrationState();

                return;
            }


            SetFeedback(
                $"Đã thêm lớp {row.SectionId}.",
                false
            );

            UpdateRegistrationState();
        }


        // =====================================================
        // REMOVE
        // =====================================================

        private void RemoveSelectedSection(
            SectionRow? row)
        {
            if (row == null)
                return;


            if (!SelectedSections.Remove(row))
                return;


            row.IsSelected = false;


            SetFeedback(
                $"Đã bỏ chọn lớp {row.SectionId}.",
                false
            );


            UpdateRegistrationState();
        }


        // =====================================================
        // CLEAR ALL
        // =====================================================

        private void ClearAll()
        {
            foreach (
                SectionRow row
                in SelectedSections.ToList())
            {
                row.IsSelected = false;
            }


            SelectedSections.Clear();


            SetFeedback(
                "Đã xóa toàn bộ học phần đã chọn.",
                false
            );


            UpdateRegistrationState();
        }


        // =====================================================
        // SCHEDULE CONFLICT
        // =====================================================

        private string? FindScheduleConflict()
        {
            for (
                int i = 0;
                i < SelectedSections.Count;
                i++)
            {
                for (
                    int j = i + 1;
                    j < SelectedSections.Count;
                    j++)
                {
                    Section first =
                        SelectedSections[i].Section;

                    Section second =
                        SelectedSections[j].Section;


                    if (first.TimeOverlaps(second))
                    {
                        return
                            $"Lớp {first.SectionId} " +
                            $"bị trùng lịch với " +
                            $"{second.SectionId}.";
                    }
                }
            }


            return null;
        }


        // =====================================================
        // UPDATE UI + COMMAND STATE
        // =====================================================

        private void UpdateRegistrationState()
        {
            RaisePropertyChanged(
                nameof(SelectedSections)
            );

            RaisePropertyChanged(
                nameof(SelectedCredits)
            );

            RaisePropertyChanged(
                nameof(SelectedElectiveCredits)
            );

            RaisePropertyChanged(
                nameof(SelectedRequiredCourseCount)
            );

            RaisePropertyChanged(
                nameof(HasSelectedSections)
            );

            RaisePropertyChanged(
                nameof(SelectedProgress)
            );

            RaisePropertyChanged(
                nameof(SelectedSummaryText)
            );

            RaisePropertyChanged(
                nameof(ElectiveSummaryText)
            );

            RaisePropertyChanged(
                nameof(RequiredSummaryText)
            );

            RaisePropertyChanged(
                nameof(IsRequiredCoursesValid)
            );

            RaisePropertyChanged(
                nameof(IsElectiveCreditsValid)
            );

            RaisePropertyChanged(
                nameof(IsTotalCreditsValid)
            );

            RaisePropertyChanged(
                nameof(HasScheduleConflict)
            );

            RaisePropertyChanged(
                nameof(ScheduleConflictMessage)
            );

            RaisePropertyChanged(
                nameof(CanSubmit)
            );

            RaisePropertyChanged(
                nameof(RegistrationValidationMessage)
            );


            ClearAllCommand
                .RaiseCanExecuteChanged();


            SubmitRegistrationCommand
                .RaiseCanExecuteChanged();
        }


        // =====================================================
        // SUBMIT
        // =====================================================

        private void SubmitRegistration()
        {
            if (!CanSubmit)
            {
                SetFeedback(
                    RegistrationValidationMessage,
                    true
                );

                return;
            }


            var registered =
                new List<SectionRow>();


            foreach (
                SectionRow row
                in SelectedSections)
            {
                var result =
                    _enrollmentService.Register(
                        CurrentStudent,
                        row.Section
                    );


                if (!result.ok)
                {
                    foreach (
                        SectionRow registeredRow
                        in registered)
                    {
                        _enrollmentService.Cancel(
                            CurrentStudent.StudentId,
                            registeredRow.SectionId
                        );
                    }


                    SetFeedback(
                        result.error
                        ?? "Không thể gửi đăng ký.",
                        true
                    );

                    return;
                }


                registered.Add(row);
            }


            SetFeedback(
                $"Đăng ký thành công " +
                $"{registered.Count} lớp học phần.",
                false
            );


            foreach (
                SectionRow row
                in SelectedSections)
            {
                row.IsSelected = false;
            }


            SelectedSections.Clear();

            UpdateRegistrationState();

            Load();
        }


        // =====================================================
        // FEEDBACK HELPER
        // =====================================================

        private void SetFeedback(
            string message,
            bool isError)
        {
            Feedback = message;

            FeedbackClass =
                isError
                    ? "feedback-error"
                    : "feedback-ok";


            RaisePropertyChanged(
                nameof(HasFeedback)
            );
        }
    }
}