using EduPath.WinForms.Models;

namespace EduPath.WinForms.Data
{
    /// <summary>
    /// "Cơ sở dữ liệu" giả lập bằng List&lt;T&gt; trong RAM, dùng chung cho toàn ứng dụng qua Singleton.
    /// Khi chuyển sang SQL Server/SQLite thật, chỉ cần thay các List này bằng DbSet&lt;T&gt; của EF Core —
    /// toàn bộ Services ở tầng trên không cần đổi vì chỉ thao tác qua interface.
    /// </summary>
    public sealed class InMemoryStore
    {
        private static readonly Lazy<InMemoryStore> _instance = new(() => new InMemoryStore());
        public static InMemoryStore Instance => _instance.Value;

        public List<Account> Accounts { get; } = new();
        public List<Student> Students { get; } = new();
        public List<Lecturer> Lecturers { get; } = new();
        public List<Course> Courses { get; } = new();
        public List<Section> Sections { get; } = new();
        public List<Room> Rooms { get; } = new();
        public List<RegistrationPeriod> Periods { get; } = new();
        public List<Enrollment> Enrollments { get; } = new();

        private InMemoryStore() => Seed();

        private void Seed()
        {
            // ---- Rooms ----
            Rooms.AddRange(new[]
            {
                new Room { RoomId = "A201", Building = "A", Capacity = 60, RoomType = "Lý thuyết" },
                new Room { RoomId = "B203", Building = "B", Capacity = 60, RoomType = "Lý thuyết" },
                new Room { RoomId = "A302", Building = "A", Capacity = 60, RoomType = "Lý thuyết" },
                new Room { RoomId = "C102", Building = "C", Capacity = 50, RoomType = "Lý thuyết" },
            });

            // ---- Lecturers ----
            Lecturers.AddRange(new[]
            {
                new Lecturer { LecturerId = "GV0008", FullName = "Trần Minh Khoa", Email = "khoa.tm@edu.vn", Department = "CNTT" },
                new Lecturer { LecturerId = "GV0012", FullName = "Đỗ Quang Huy",  Email = "huy.dq@edu.vn",  Department = "CNTT" },
                new Lecturer { LecturerId = "GV0015", FullName = "Vũ Đình Long",  Email = "long.vd@edu.vn", Department = "CNTT" },
                new Lecturer { LecturerId = "GV0020", FullName = "Lê Mai",        Email = "mai.le@edu.vn",  Department = "Toán" },
            });

            // ---- Courses ----
            Courses.AddRange(new[]
            {
                new Course { CourseCode = "CS101", CourseName = "Nhập môn lập trình",           Credits = 3, Faculty = "CNTT", PrerequisiteCode = null },
                new Course { CourseCode = "CS201", CourseName = "Cấu trúc dữ liệu và giải thuật", Credits = 4, Faculty = "CNTT", PrerequisiteCode = "CS101" },
                new Course { CourseCode = "CS208", CourseName = "Cơ sở dữ liệu",                 Credits = 4, Faculty = "CNTT", PrerequisiteCode = "CS201" },
                new Course { CourseCode = "CS305", CourseName = "Mạng máy tính",                 Credits = 3, Faculty = "CNTT", PrerequisiteCode = null },
                new Course { CourseCode = "MA104", CourseName = "Toán rời rạc",                  Credits = 3, Faculty = "Toán", PrerequisiteCode = null },
            });

            // ---- Registration period ----
            Periods.Add(new RegistrationPeriod
            {
                Name = "Đợt đăng ký HK1 2026-2027",
                Term = "HK1 2026-2027",
                StartDate = new DateTime(2026, 8, 20),
                EndDate = new DateTime(2026, 8, 30),
                MinCredits = 12,
                MaxCredits = 24,
                IsOpen = true
            });
            Periods.Add(new RegistrationPeriod
            {
                Name = "Đợt bổ sung HK1",
                Term = "HK1 2026-2027",
                StartDate = new DateTime(2026, 9, 5),
                EndDate = new DateTime(2026, 9, 7),
                MinCredits = 12,
                MaxCredits = 24,
                IsOpen = false
            });

            // ---- Sections ----
            Sections.AddRange(new[]
            {
                new Section { SectionId = "CS201-01", CourseCode = "CS201", Term = "HK1 2026-2027", LecturerId = "GV0008", RoomId = "A201", DayOfWeek = 2, StartTime = new TimeSpan(9,0,0),  EndTime = new TimeSpan(10,30,0), Capacity = 60, Enrolled = 45 },
                new Section { SectionId = "CS208-02", CourseCode = "CS208", Term = "HK1 2026-2027", LecturerId = "GV0012", RoomId = "B203", DayOfWeek = 3, StartTime = new TimeSpan(13,0,0), EndTime = new TimeSpan(14,30,0), Capacity = 60, Enrolled = 58 },
                new Section { SectionId = "CS305-01", CourseCode = "CS305", Term = "HK1 2026-2027", LecturerId = "GV0015", RoomId = "A302", DayOfWeek = 4, StartTime = new TimeSpan(9,0,0),  EndTime = new TimeSpan(10,30,0), Capacity = 60, Enrolled = 60 },
                new Section { SectionId = "MA104-01", CourseCode = "MA104", Term = "HK1 2026-2027", LecturerId = "GV0020", RoomId = "C102", DayOfWeek = 5, StartTime = new TimeSpan(13,0,0), EndTime = new TimeSpan(14,30,0), Capacity = 50, Enrolled = 30 },
            });

            // ---- Students ----
            Students.Add(new Student
            {
                StudentId = "SV20260018",
                FullName = "Nguyễn Minh Anh",
                Email = "minhanh@edu.vn",
                Faculty = "CNTT",
                IntakeYear = 2026,
                ClassCode = "CNTT01",
                CompletedCourseCodes = new HashSet<string> { "CS101" } // đã học xong CS101 -> đủ điều kiện học CS201
            });

            // ---- Accounts ----
            Accounts.AddRange(new[]
            {
                new Account { Username = "SV20260018", PasswordHash = "123456", Role = Role.Student, LinkedId = "SV20260018" },
                new Account { Username = "AD0001",     PasswordHash = "admin123", Role = Role.Admin,   LinkedId = null },
            });

            // ---- Enrollments (đã đăng ký sẵn CS201-01 và CS208-02 cho demo) ----
            Enrollments.AddRange(new[]
            {
                new Enrollment { StudentId = "SV20260018", SectionId = "CS201-01", RegisteredAt = new DateTime(2026,8,20,8,35,0), Status = EnrollmentStatus.Enrolled },
                new Enrollment { StudentId = "SV20260018", SectionId = "CS208-02", RegisteredAt = new DateTime(2026,8,20,8,40,0), Status = EnrollmentStatus.Enrolled },
            });
        }
    }
}
