using System;
using System.Collections.Generic;
using System.Linq;
using EduPath.Avalonia.Data;
using EduPath.Avalonia.Models;

namespace EduPath.Avalonia.ViewModels
{
    // Model đại diện cho 1 ô trong bảng thời khóa biểu
    public class TimeSlotCell
    {
        public int DayOfWeek { get; set; }     // 2 = Thứ 2, 3 = Thứ 3, ..., 7 = Thứ 7
        public string TimeSlot { get; set; } = string.Empty; // Ví dụ: "07:00 - 09:00"
        public SectionRow? Section { get; set; }
        public ScheduleSlot? ScheduleSlot { get; set; } // Lưu vết thông tin slot lịch
        public bool HasClass => Section != null;
        public string BackgroundColor { get; set; } = "#F1F5F9";
        public string BorderColor { get; set; } = "Transparent";
    }

    // Model đại diện cho 1 dòng (khung giờ)
    public class TimeSlotRow
    {
        public string TimeLabel { get; set; } = string.Empty;
        public List<TimeSlotCell> Cells { get; set; } = new();
    }

    public class TimetableViewModel : ViewModelBase
    {
        private readonly InMemoryStore _store = InMemoryStore.Instance;

        public List<string> DayHeaders { get; } = new() { "Thứ 2", "Thứ 3", "Thứ 4", "Thứ 5", "Thứ 6", "Thứ 7" };
        public List<TimeSlotRow> MatrixRows { get; } = new();

        // 5 Khung giờ chính tương ứng với các ca học
        private readonly List<(TimeSpan Start, TimeSpan End, string Label)> _timeSlots = new()
        {
            (new TimeSpan(7, 0, 0), new TimeSpan(9, 0, 0), "07:00 - 09:00"),
            (new TimeSpan(9, 0, 0), new TimeSpan(11, 0, 0), "09:00 - 11:00"),
            (new TimeSpan(11, 0, 0), new TimeSpan(13, 0, 0), "11:00 - 13:00"),
            (new TimeSpan(13, 0, 0), new TimeSpan(15, 0, 0), "13:00 - 15:00"),
            (new TimeSpan(15, 0, 0), new TimeSpan(17, 0, 0), "15:00 - 17:00")
        };

        // Bảng màu cho thẻ môn học
        private readonly string[] _colors = new[] { "#E0EDFF", "#D1FAE5", "#FEF3C7", "#FCE7F3", "#EDE9FE" };
        private readonly string[] _borderColors = new[] { "#93C5FD", "#6EE7B7", "#FCD34D", "#F472B6", "#C4B5FD" };

        public TimetableViewModel(Student student)
        {
            // 1. LẤY DỮ LIỆU ĐĂNG KÝ TỪ INMEMORYSTORE
            var enrolledSectionIds = _store.Enrollments
                .Where(e => e.StudentId == student.StudentId && e.Status.ToString() == "Active")
                .Select(e => e.SectionId)
                .ToHashSet();

            // Lấy danh sách các Section tương ứng
            var activeSections = _store.Sections
                .Where(s => enrolledSectionIds.Contains(s.SectionId))
                .ToList();

            // Nếu sinh viên chưa có môn nào trong Enrollments, lấy các lớp IsOpen để test hiển thị
            if (!activeSections.Any())
            {
                activeSections = _store.Sections.Where(s => s.IsOpen).Take(5).ToList();
            }

            // Chuyển sang danh sách SectionRow kèm thông tin Course & Lecturer
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

            // 3. XÂY DỰNG MA TRẬN BẢNG THỜI KHÓA BIỂU
            foreach (var slot in _timeSlots)
            {
                var row = new TimeSlotRow { TimeLabel = slot.Label };

                for (int day = 2; day <= 7; day++)
                {
                    SectionRow? matchedSection = null;
                    ScheduleSlot? matchedSlot = null;

                    // Đọc từ danh sách Schedules (nếu có)
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

                MatrixRows.Add(row);
            }
        }
    }
}