Khi bạn đã hoàn thiện Database (lưu dữ liệu đăng ký thành công vào bảng Enrollments), bạn cần sửa lại TimetableViewModel với 2 thay đổi rất quan trọng sau đây để nó hoạt động thực tế:

Xóa bỏ đoạn code fallback sinh dữ liệu giả: Không tự động lấy các môn IsOpen khi sinh viên chưa đăng ký môn nào nữa. Nếu chưa đăng ký, lịch học phải hiển thị trống.

Thêm cơ chế tự động làm mới (IRefreshable): Vì người dùng thao tác ở tab Đăng ký học phần, sau đó mới bấm sang tab Thời khóa biểu. Nếu bạn chỉ load dữ liệu ở hàm khởi tạo (Constructor), lịch học sẽ không được cập nhật trừ khi tắt app mở lại.

Dưới đây là cấu trúc hoàn chỉnh, sẵn sàng cho Production của TimetableViewModel.cs mà bạn nên áp dụng:

File TimetableViewModel.cs (Sẵn sàng cho DB thật)
C#
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using EduPath.Avalonia.Data;
using EduPath.Avalonia.Models;

namespace EduPath.Avalonia.ViewModels
{
    public class TimeSlotCell
    {
        public int DayOfWeek { get; set; }
        public string TimeSlot { get; set; } = string.Empty;
        public SectionRow? Section { get; set; }
        public ScheduleSlot? ScheduleSlot { get; set; }
        public bool HasClass => Section != null;
        public string BackgroundColor { get; set; } = "#F1F5F9";
        public string BorderColor { get; set; } = "Transparent";
    }

    public class TimeSlotRow
    {
        public string TimeLabel { get; set; } = string.Empty;
        public List<TimeSlotCell> Cells { get; set; } = new();
    }

    // ✅ BƯỚC 1: Kế thừa thêm IRefreshable để tự động load lại khi chuyển tab
    public class TimetableViewModel : ViewModelBase, IRefreshable
    {
        private readonly InMemoryStore _store = InMemoryStore.Instance;
        private readonly Student _currentStudent; // Lưu trữ lại biến student để dùng khi Refresh

        public List<string> DayHeaders { get; } = new() { "Thứ 2", "Thứ 3", "Thứ 4", "Thứ 5", "Thứ 6", "Thứ 7" };
        
        // ✅ BƯỚC 2: Đổi List thành ObservableCollection để UI tự động vẽ lại khi dữ liệu thay đổi
        public ObservableCollection<TimeSlotRow> MatrixRows { get; } = new();

        private readonly List<(TimeSpan Start, TimeSpan End, string Label)> _timeSlots = new()
        {
            (new TimeSpan(7, 0, 0), new TimeSpan(9, 0, 0), "07:00 - 09:00"),
            (new TimeSpan(9, 0, 0), new TimeSpan(11, 0, 0), "09:00 - 11:00"),
            (new TimeSpan(11, 0, 0), new TimeSpan(13, 0, 0), "11:00 - 13:00"),
            (new TimeSpan(13, 0, 0), new TimeSpan(15, 0, 0), "13:00 - 15:00"),
            (new TimeSpan(15, 0, 0), new TimeSpan(17, 0, 0), "15:00 - 17:00")
        };

        private readonly string[] _colors = new[] { "#E0EDFF", "#D1FAE5", "#FEF3C7", "#FCE7F3", "#EDE9FE" };
        private readonly string[] _borderColors = new[] { "#93C5FD", "#6EE7B7", "#FCD34D", "#F472B6", "#C4B5FD" };

        public TimetableViewModel(Student student)
        {
            _currentStudent = student;
            Load(); // Load lần đầu khi khởi tạo
        }

        // Thực thi interface IRefreshable (Được gọi từ StudentShellViewModel khi bấm vào Menu)
        public void Refresh() => Load();

        private void Load()
        {
            // Xóa sạch lịch cũ trên UI trước khi vẽ lịch mới
            MatrixRows.Clear();

            // 1. TRUY VẤN DB THẬT: Tìm các môn đã đăng ký thành công
            var enrolledSectionIds = _store.Enrollments
                .Where(e => e.StudentId == _currentStudent.StudentId && e.Status == EnrollmentStatus.Active)
                .Select(e => e.SectionId)
                .ToHashSet();

            var activeSections = _store.Sections
                .Where(s => enrolledSectionIds.Contains(s.SectionId))
                .ToList();

            // ✅ ĐÃ XÓA ĐOẠN IF LẤY DỮ LIỆU FAKE Ở ĐÂY. 
            // Nếu activeSections rỗng (chưa đăng ký môn nào), thuật toán bên dưới sẽ tự vẽ ra các ô trống màu xám.

            var sectionRows = activeSections.Select(s => new SectionRow(
                s,
                _store.Courses.FirstOrDefault(c => c.CourseCode == s.CourseCode),
                _store.Lecturers.FirstOrDefault(l => l.LecturerId == s.LecturerId)
            )).ToList();

            // 2. MÁP MÀU SẮC CHO TỪNG MÔN HỌC
            int colorIndex = 0;
            var courseColorMap = new Dictionary<string, (string bg, string border)>();
            foreach (var row in sectionRows)
            {
                if (!courseColorMap.ContainsKey(row.CourseCode))
                {
                    courseColorMap[row.CourseCode] = (_colors[colorIndex % _colors.Length], _borderColors[colorIndex % _borderColors.Length]);
                    colorIndex++;
                }
            }

            // 3. XÂY DỰNG LẠI MA TRẬN
            foreach (var slot in _timeSlots)
            {
                var row = new TimeSlotRow { TimeLabel = slot.Label };

                for (int day = 2; day <= 7; day++)
                {
                    SectionRow? matchedSection = null;
                    ScheduleSlot? matchedSlot = null;

                    foreach (var secRow in sectionRows)
                    {
                        if (secRow.Section.Schedules != null && secRow.Section.Schedules.Any())
                        {
                            var slotMatch = secRow.Section.Schedules.FirstOrDefault(sch =>
                                sch.DayOfWeek == day &&
                                sch.StartTime < slot.End && sch.EndTime > slot.Start);

                            if (slotMatch != null)
                            {
                                matchedSection = secRow;
                                matchedSlot = slotMatch;
                                break;
                            }
                        }
                        else if (secRow.Section.DayOfWeek == day &&
                                 secRow.Section.StartTime < slot.End &&
                                 secRow.Section.EndTime > slot.Start)
                        {
                            matchedSection = secRow;
                            break;
                        }
                    }

                    string bg = "#F1F5F9";
                    string border = "Transparent";

                    if (matchedSection != null)
                    {
                        var style = courseColorMap[matchedSection.CourseCode];
                        bg = style.bg;
                        border = style.border;
                    }

                    row.Cells.Add(new TimeSlotCell
                    {
                        DayOfWeek = day,
                        TimeSlot = slot.Label,
                        Section = matchedSection,
                        ScheduleSlot = matchedSlot,
                        BackgroundColor = bg,
                        BorderColor = border
                    });
                }

                // Thêm hàng vào ObservableCollection để UI tự động cập nhật
                MatrixRows.Add(row);
            }
        }
    }
}