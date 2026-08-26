using System;
using EduPath.Avalonia.Models;

namespace EduPath.Avalonia.ViewModels
{
    public class SectionRow : ViewModelBase
    {
        public Section Section { get; }

        public Course? Course { get; }

        public Lecturer? Lecturer { get; }

        private bool _isSelected;

        

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (SetProperty(ref _isSelected, value))
                {
                    RaisePropertyChanged(nameof(CanAdd));
                    RaisePropertyChanged(nameof(AddButtonText));
                }
            }
        }

        // =====================================================
        // SECTION
        // =====================================================

        public string SectionId => Section.SectionId;

        public string CourseCode => Section.CourseCode;

        public string LecturerId => Section.LecturerId;

        public string Term => Section.Term;

        public string RoomId => Section.RoomId;

        // =====================================================
        // COURSE
        // =====================================================

        public string CourseName =>
            Course?.CourseName ?? "Chưa xác định môn học";

        public int Credits =>
            Course?.Credits ?? 0;

        public bool IsRequired =>
            Course?.IsRequired ?? true;

        public bool IsElective => !IsRequired;

        public string CourseTypeText =>
            IsRequired ? "Bắt buộc" : "Tự chọn";

        // =====================================================
        // ADD BUTTON
        // =====================================================

        public bool CanAdd =>
            !IsFull &&
            !IsSelected &&
            Section.IsOpen;

        public string AddButtonText
        {
            get
            {
                if (IsSelected)
                    return "✓ Đã chọn";

                if (IsFull)
                    return "Đã đầy";

                if (!Section.IsOpen)
                    return "Đã đóng";

                return "+ Thêm";
            }
        }

        // =====================================================
        // LECTURER
        // =====================================================

        public string LecturerName =>
            Lecturer?.FullName ?? "Chưa phân công";

        // =====================================================
        // SCHEDULE
        // =====================================================

        public string Schedule
        {
            get
            {
                if (Section.DayOfWeek <= 0)
                    return "Chưa có lịch";

                string day = GetDayName(Section.DayOfWeek);

                string start =
                    Section.StartTime.ToString(@"hh\:mm");

                string end =
                    Section.EndTime.ToString(@"hh\:mm");

                return $"{day} {start}-{end}";
            }
        }

        private static string GetDayName(int day)
        {
            return day switch
            {
                2 => "Thứ 2",
                3 => "Thứ 3",
                4 => "Thứ 4",
                5 => "Thứ 5",
                6 => "Thứ 6",
                7 => "Thứ 7",
                8 => "Chủ nhật",
                _ => "Chưa có lịch"
            };
        }

        // =====================================================
        // CAPACITY
        // =====================================================

        public int Capacity => Section.Capacity;

        public int Enrolled => Section.Enrolled;

        public int RemainingSeats
        {
            get
            {
                int remaining = Capacity - Enrolled;

                return remaining < 0 ? 0 : remaining;
            }
        }

        public bool IsFull =>
            RemainingSeats <= 0;

        public string SeatsLabel =>
            $"{Enrolled}/{Capacity}";

    // =====================================================
        // STATUS
        // =====================================================

        public string StatusText
        {
            get
            {
                if (!Section.IsOpen)
                    return "Đã đóng";

                if (IsFull)
                    return "Đã đầy";

                return "Còn chỗ";
            }
        }

        // BỔ SUNG THUỘC TÍNH NÀY: Trả về class tương ứng để XAML đổi màu Badge
        public string StatusBadge
        {
            get
            {
                if (!Section.IsOpen)
                    return "badge-error"; // Màu đỏ/xám cho "Đã đóng"

                if (IsFull)
                    return "badge-warning"; // Màu vàng/cam cho "Đã đầy" (nếu XAML của bạn hỗ trợ, nếu không thì dùng "badge-error")

                return "badge-ok"; // Màu xanh cho "Còn chỗ"
            }
        }

        // =====================================================
        // CONSTRUCTOR
        // =====================================================

        public SectionRow(
            Section section,
            Course? course,
            Lecturer? lecturer)
        {
            Section = section;
            Course = course;
            Lecturer = lecturer;
        }
    }
}