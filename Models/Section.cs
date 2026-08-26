namespace EduPath.Avalonia.Models
{
    /// <summary>
    /// Lớp học phần: một Course được mở trong một học kỳ cụ thể, gắn GV/phòng/khung giờ/sĩ số.
    /// DayOfWeek dùng số 2-8 kiểu VN (Thứ 2 = 2 ... Chủ nhật = 8) để khớp UI mockup "Thứ 2".
    /// </summary>
    public class Section
    {
        // ======================================================================
        //  1. ĐỊNH DANH
        // ======================================================================
        public string SectionId { get; set; } = string.Empty;   // CS201-01
        public string CourseCode { get; set; } = string.Empty;
        public string Term { get; set; } = string.Empty;        // HK1 2026-2027

        // ======================================================================
        //  2. GIẢNG VIÊN & PHÒNG HỌC
        // ======================================================================
        public string LecturerId { get; set; } = string.Empty;
        public string RoomId { get; set; } = string.Empty;

        // ======================================================================
        //  3. THỜI KHÓA BIỂU
        // ======================================================================
        public int DayOfWeek { get; set; }                     // 2..8 (Thứ 2..Chủ nhật)
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        // ======================================================================
        //  4. SỨC CHỨA & ĐĂNG KÝ
        // ======================================================================
        public int Capacity { get; set; }
        public int Enrolled { get; set; }
        public bool IsOpen { get; set; } = true;

        // ======================================================================
        //  5. THUỘC TÍNH TÍNH TOÁN (READ-ONLY)
        // ======================================================================
        /// <summary>Số chỗ trống còn lại</summary>
        public int Remaining => Capacity - Enrolled;

        /// <summary>Nhãn ngày trong tuần (VD: "Thứ 3" hoặc "Chủ nhật")</summary>
        public string DayLabel => DayOfWeek == 8 ? "Chủ nhật" : $"Thứ {DayOfWeek}";

        /// <summary>Nhãn thời gian (VD: "09:00–10:30")</summary>
        public string TimeLabel => $"{StartTime:hh\\:mm}–{EndTime:hh\\:mm}";

        // ======================================================================
        //  6. LỊCH HỌC CHI TIẾT (DỰ PHÒNG CHO NHIỀU BUỔI)
        // ======================================================================
        /// <summary>
        /// Danh sách các buổi học cụ thể (có thể dùng thay cho DayOfWeek/StartTime/EndTime
        /// nếu lớp có lịch phức tạp, hoặc để hiển thị chi tiết).
        /// </summary>
        public List<ScheduleSlot> Schedules { get; set; } = new();

        // ======================================================================
        //  7. PHƯƠNG THỨC
        // ======================================================================
        /// <summary>
        /// Kiểm tra hai khung giờ có giao nhau về thời gian không
        /// (chỉ so sánh thứ và giờ, chưa xét phòng/GV).
        /// </summary>
        public bool TimeOverlaps(Section other) =>
            DayOfWeek == other.DayOfWeek &&
            StartTime < other.EndTime && other.StartTime < EndTime;
    }
}