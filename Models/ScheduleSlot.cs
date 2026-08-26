// Models/ScheduleSlot.cs
namespace EduPath.Avalonia.Models
{
    public class ScheduleSlot
    {
        public int DayOfWeek { get; set; }          // 2 = Thứ 2 ... 8 = Chủ nhật
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string RoomId { get; set; } = "";
        public string SessionType { get; set; } = "Lý thuyết"; // Lý thuyết / Thực hành
        public string Periods { get; set; } = "Tiết 1-3";      // hiển thị

        public string DayName => DayOfWeek switch
        {
            2 => "Thứ 2",
            3 => "Thứ 3",
            4 => "Thứ 4",
            5 => "Thứ 5",
            6 => "Thứ 6",
            7 => "Thứ 7",
            8 => "Chủ nhật",
            _ => "Chưa có"
        };

        public string TimeRange =>
            $"{StartTime:hh\\:mm}-{EndTime:hh\\:mm}";

        public string DisplaySchedule =>
            $"{DayName} {TimeRange}";
    }
}