using EduPath.Avalonia.Models;

namespace EduPath.Avalonia.Data
{
    /// <summary>
    /// "Cơ sở dữ liệu" giả lập bằng List<T> trong RAM, dùng chung cho toàn ứng dụng qua Singleton.
    /// Khi chuyển sang SQL Server/SQLite thật, chỉ cần thay các List này bằng DbSet<T> của EF Core —
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
            // =========================================================
            // ROOMS
            // =========================================================
            Rooms.AddRange(new[]
            {
                // Dữ liệu gốc
                new Room { RoomId = "A201", Building = "A", Capacity = 60, RoomType = "Lý thuyết" },
                new Room { RoomId = "B203", Building = "B", Capacity = 60, RoomType = "Lý thuyết" },
                new Room { RoomId = "A302", Building = "A", Capacity = 60, RoomType = "Lý thuyết" },
                new Room { RoomId = "C102", Building = "C", Capacity = 50, RoomType = "Lý thuyết" },

                // Data bổ sung
                new Room { RoomId = "A202", Building = "A", Capacity = 50, RoomType = "Lý thuyết" },
                new Room { RoomId = "A305", Building = "A", Capacity = 45, RoomType = "Phòng máy" },
                new Room { RoomId = "B205", Building = "B", Capacity = 50, RoomType = "Lý thuyết" },
                new Room { RoomId = "B301", Building = "B", Capacity = 40, RoomType = "Phòng máy" },
                new Room { RoomId = "C201", Building = "C", Capacity = 45, RoomType = "Phòng máy" },
                new Room { RoomId = "C205", Building = "C", Capacity = 40, RoomType = "Lý thuyết" }
            });

            // =========================================================
            // LECTURERS
            // Tổng cộng 10 giảng viên
            // =========================================================
            Lecturers.AddRange(new[]
            {
                new Lecturer { LecturerId = "GV0008", FullName = "Trần Minh Khoa", Email = "khoa.tm@dlu.edu.vn", Department = "CNTT" },
                new Lecturer { LecturerId = "GV0012", FullName = "Đỗ Quang Huy", Email = "huy.dq@dlu.edu.vn", Department = "CNTT" },
                new Lecturer { LecturerId = "GV0015", FullName = "Vũ Đình Long", Email = "long.vd@dlu.edu.vn", Department = "CNTT" },
                new Lecturer { LecturerId = "GV0020", FullName = "Lê Mai", Email = "mai.le@dlu.edu.vn", Department = "Toán" },
                new Lecturer { LecturerId = "GV0025", FullName = "Nguyễn Hoàng Nam", Email = "nam.nh@dlu.edu.vn", Department = "CNTT" },
                new Lecturer { LecturerId = "GV0031", FullName = "Phạm Thị Ngọc Anh", Email = "anh.ptn@dlu.edu.vn", Department = "CNTT" },
                new Lecturer { LecturerId = "GV0037", FullName = "Nguyễn Đức Thành", Email = "thanh.nd@dlu.edu.vn", Department = "CNTT" },
                new Lecturer { LecturerId = "GV0042", FullName = "Trần Quốc Việt", Email = "viet.tq@dlu.edu.vn", Department = "Toán" },
                new Lecturer { LecturerId = "GV0048", FullName = "Lâm Thị Thanh Hương", Email = "huong.ltt@dlu.edu.vn", Department = "CNTT" },
                new Lecturer { LecturerId = "GV0053", FullName = "Phan Anh Tuấn", Email = "tuan.pa@dlu.edu.vn", Department = "CNTT" }
            });

            // =========================================================
            // COURSES
            // PHÂN LOẠI: BẮT BUỘC (IsRequired = true) / TỰ CHỌN (IsRequired = false)
            // ElectiveGroup: nhóm tự chọn (nếu có)
            // =========================================================
            Courses.AddRange(new[]
            {
                // ==================== BẮT BUỘC ====================
                // Môn đại cương
                new Course
                {
                    CourseCode = "MA104",
                    CourseName = "Toán rời rạc",
                    Credits = 3,
                    Faculty = "Toán",
                    PrerequisiteCode = null,
                    IsRequired = true,
                    ElectiveGroup = ""
                },
                new Course
                {
                    CourseCode = "EN101",
                    CourseName = "Tiếng Anh cơ bản",
                    Credits = 2,
                    Faculty = "Ngoại ngữ",
                    PrerequisiteCode = null,
                    IsRequired = true,
                    ElectiveGroup = ""
                },
                // Môn cơ sở ngành
                new Course
                {
                    CourseCode = "CS101",
                    CourseName = "Nhập môn lập trình",
                    Credits = 3,
                    Faculty = "CNTT",
                    PrerequisiteCode = null,
                    IsRequired = true,
                    ElectiveGroup = ""
                },
                new Course
                {
                    CourseCode = "CS201",
                    CourseName = "Cấu trúc dữ liệu và giải thuật",
                    Credits = 4,
                    Faculty = "CNTT",
                    PrerequisiteCode = "CS101",
                    IsRequired = true,
                    ElectiveGroup = ""
                },
                new Course
                {
                    CourseCode = "CS208",
                    CourseName = "Cơ sở dữ liệu",
                    Credits = 4,
                    Faculty = "CNTT",
                    PrerequisiteCode = "CS201",
                    IsRequired = true,
                    ElectiveGroup = ""
                },
                new Course
                {
                    CourseCode = "CS310",
                    CourseName = "Lập trình hướng đối tượng",
                    Credits = 3,
                    Faculty = "CNTT",
                    PrerequisiteCode = "CS101",
                    IsRequired = true,
                    ElectiveGroup = ""
                },
                new Course
                {
                    CourseCode = "CS320",
                    CourseName = "Hệ điều hành",
                    Credits = 4,
                    Faculty = "CNTT",
                    PrerequisiteCode = "CS201",
                    IsRequired = true,
                    ElectiveGroup = ""
                },
                new Course
                {
                    CourseCode = "CS325",
                    CourseName = "Công nghệ phần mềm",
                    Credits = 3,
                    Faculty = "CNTT",
                    PrerequisiteCode = "CS310",
                    IsRequired = true,
                    ElectiveGroup = ""
                },
                new Course
                {
                    CourseCode = "CS330",
                    CourseName = "Phân tích và thiết kế hệ thống",
                    Credits = 3,
                    Faculty = "CNTT",
                    PrerequisiteCode = "CS310",
                    IsRequired = true,
                    ElectiveGroup = ""
                },

                // ==================== TỰ CHỌN ====================
                // Nhóm CNTT tự chọn
                new Course
                {
                    CourseCode = "CS305",
                    CourseName = "Mạng máy tính",
                    Credits = 3,
                    Faculty = "CNTT",
                    PrerequisiteCode = null,
                    IsRequired = false,
                    ElectiveGroup = "CNTT Tự chọn"
                },
                new Course
                {
                    CourseCode = "CS315",
                    CourseName = "Lập trình Web",
                    Credits = 3,
                    Faculty = "CNTT",
                    PrerequisiteCode = "CS310",
                    IsRequired = false,
                    ElectiveGroup = "CNTT Tự chọn"
                },
                new Course
                {
                    CourseCode = "CS340",
                    CourseName = "Trí tuệ nhân tạo",
                    Credits = 3,
                    Faculty = "CNTT",
                    PrerequisiteCode = "CS201",
                    IsRequired = false,
                    ElectiveGroup = "CNTT Tự chọn"
                },
                new Course
                {
                    CourseCode = "CS350",
                    CourseName = "Khai phá dữ liệu",
                    Credits = 3,
                    Faculty = "CNTT",
                    PrerequisiteCode = "CS208",
                    IsRequired = false,
                    ElectiveGroup = "CNTT Tự chọn"
                },
                new Course
                {
                    CourseCode = "CS360",
                    CourseName = "An toàn thông tin",
                    Credits = 3,
                    Faculty = "CNTT",
                    PrerequisiteCode = "CS305",
                    IsRequired = false,
                    ElectiveGroup = "CNTT Tự chọn"
                },
                // Môn tự chọn mới
                new Course
                {
                    CourseCode = "CS410",
                    CourseName = "Lập trình di động",
                    Credits = 3,
                    Faculty = "CNTT",
                    PrerequisiteCode = "CS310",
                    IsRequired = false,
                    ElectiveGroup = "CNTT Tự chọn"
                },
                new Course
                {
                    CourseCode = "CS420",
                    CourseName = "Phát triển game",
                    Credits = 3,
                    Faculty = "CNTT",
                    PrerequisiteCode = "CS310",
                    IsRequired = false,
                    ElectiveGroup = "CNTT Tự chọn"
                },
                new Course
                {
                    CourseCode = "CS430",
                    CourseName = "Học máy",
                    Credits = 4,
                    Faculty = "CNTT",
                    PrerequisiteCode = "CS340",
                    IsRequired = false,
                    ElectiveGroup = "CNTT Tự chọn"
                },
                new Course
                {
                    CourseCode = "CS440",
                    CourseName = "Big Data",
                    Credits = 3,
                    Faculty = "CNTT",
                    PrerequisiteCode = "CS350",
                    IsRequired = false,
                    ElectiveGroup = "CNTT Tự chọn"
                },

                // Nhóm Toán tự chọn
                new Course
                {
                    CourseCode = "MA201",
                    CourseName = "Xác suất thống kê",
                    Credits = 3,
                    Faculty = "Toán",
                    PrerequisiteCode = null,
                    IsRequired = false,
                    ElectiveGroup = "Toán Tự chọn"
                },
                new Course
                {
                    CourseCode = "MA301",
                    CourseName = "Toán ứng dụng",
                    Credits = 3,
                    Faculty = "Toán",
                    PrerequisiteCode = "MA104",
                    IsRequired = false,
                    ElectiveGroup = "Toán Tự chọn"
                },

                // Nhóm Ngoại ngữ tự chọn
                new Course
                {
                    CourseCode = "EN201",
                    CourseName = "Tiếng Anh chuyên ngành CNTT",
                    Credits = 2,
                    Faculty = "Ngoại ngữ",
                    PrerequisiteCode = "EN101",
                    IsRequired = false,
                    ElectiveGroup = "Ngoại ngữ Tự chọn"
                },
                new Course
                {
                    CourseCode = "EN202",
                    CourseName = "Tiếng Anh giao tiếp nâng cao",
                    Credits = 2,
                    Faculty = "Ngoại ngữ",
                    PrerequisiteCode = "EN101",
                    IsRequired = false,
                    ElectiveGroup = "Ngoại ngữ Tự chọn"
                }
            });

            // =========================================================
            // REGISTRATION PERIOD
            // =========================================================
            Periods.Add(new RegistrationPeriod
            {
                Name = "Đợt đăng ký HK1 2026-2027",
                Term = "HK1 2026-2027",
                StartDate = new DateTime(2026, 8, 20),
                EndDate = new DateTime(2026, 8, 30),
                MinCredits = 12,
                MaxCredits = 40,
                MinElectiveCredits = 3,
                MaxElectiveCredits = 9,
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
                MinElectiveCredits = 3,
                MaxElectiveCredits = 9,
                IsOpen = false
            });

            // =========================================================
            // SECTIONS
            // Tổng cộng 20 lớp học phần (giữ nguyên, nhưng có thể bổ sung sau)
            // =========================================================
            Sections.AddRange(new[]
            {
                // 01 - CS201
                new Section
                {
                    SectionId = "CS201-01",
                    CourseCode = "CS201",
                    Term = "HK1 2026-2027",
                    LecturerId = "GV0008",
                    RoomId = "A201",
                    DayOfWeek = 2,
                    StartTime = new TimeSpan(9, 0, 0),
                    EndTime = new TimeSpan(10, 30, 0),
                    Capacity = 60,
                    Enrolled = 45
                },
                // 02 - CS201
                new Section
                {
                    SectionId = "CS201-02",
                    CourseCode = "CS201",
                    Term = "HK1 2026-2027",
                    LecturerId = "GV0012",
                    RoomId = "B203",
                    DayOfWeek = 5,
                    StartTime = new TimeSpan(13, 0, 0),
                    EndTime = new TimeSpan(14, 30, 0),
                    Capacity = 60,
                    Enrolled = 35
                },
                // 03 - CS208
                new Section
                {
                    SectionId = "CS208-01",
                    CourseCode = "CS208",
                    Term = "HK1 2026-2027",
                    LecturerId = "GV0015",
                    RoomId = "A302",
                    DayOfWeek = 3,
                    StartTime = new TimeSpan(13, 0, 0),
                    EndTime = new TimeSpan(14, 30, 0),
                    Capacity = 60,
                    Enrolled = 58
                },
                // 04 - CS208
                new Section
                {
                    SectionId = "CS208-02",
                    CourseCode = "CS208",
                    Term = "HK1 2026-2027",
                    LecturerId = "GV0025",
                    RoomId = "B203",
                    DayOfWeek = 6,
                    StartTime = new TimeSpan(15, 0, 0),
                    EndTime = new TimeSpan(16, 30, 0),
                    Capacity = 60,
                    Enrolled = 30
                },
                // 05 - CS305
                new Section
                {
                    SectionId = "CS305-01",
                    CourseCode = "CS305",
                    Term = "HK1 2026-2027",
                    LecturerId = "GV0015",
                    RoomId = "A302",
                    DayOfWeek = 4,
                    StartTime = new TimeSpan(9, 0, 0),
                    EndTime = new TimeSpan(10, 30, 0),
                    Capacity = 60,
                    Enrolled = 60
                },
                // 06 - CS305
                new Section
                {
                    SectionId = "CS305-02",
                    CourseCode = "CS305",
                    Term = "HK1 2026-2027",
                    LecturerId = "GV0037",
                    RoomId = "C205",
                    DayOfWeek = 2,
                    StartTime = new TimeSpan(18, 0, 0),
                    EndTime = new TimeSpan(19, 30, 0),
                    Capacity = 40,
                    Enrolled = 25
                },
                // 07 - CS310
                new Section
                {
                    SectionId = "CS310-01",
                    CourseCode = "CS310",
                    Term = "HK1 2026-2027",
                    LecturerId = "GV0031",
                    RoomId = "A305",
                    DayOfWeek = 3,
                    StartTime = new TimeSpan(7, 0, 0),
                    EndTime = new TimeSpan(8, 30, 0),
                    Capacity = 45,
                    Enrolled = 32
                },
                // 08 - CS310
                new Section
                {
                    SectionId = "CS310-02",
                    CourseCode = "CS310",
                    Term = "HK1 2026-2027",
                    LecturerId = "GV0053",
                    RoomId = "B301",
                    DayOfWeek = 6,
                    StartTime = new TimeSpan(19, 0, 0),
                    EndTime = new TimeSpan(20, 30, 0),
                    Capacity = 40,
                    Enrolled = 20
                },
                // 09 - CS315
                new Section
                {
                    SectionId = "CS315-01",
                    CourseCode = "CS315",
                    Term = "HK1 2026-2027",
                    LecturerId = "GV0025",
                    RoomId = "A305",
                    DayOfWeek = 4,
                    StartTime = new TimeSpan(13, 0, 0),
                    EndTime = new TimeSpan(14, 30, 0),
                    Capacity = 45,
                    Enrolled = 40
                },
                // 10 - CS320
                new Section
                {
                    SectionId = "CS320-01",
                    CourseCode = "CS320",
                    Term = "HK1 2026-2027",
                    LecturerId = "GV0037",
                    RoomId = "B301",
                    DayOfWeek = 5,
                    StartTime = new TimeSpan(7, 0, 0),
                    EndTime = new TimeSpan(8, 30, 0),
                    Capacity = 40,
                    Enrolled = 28
                },
                // 11 - CS325
                new Section
                {
                    SectionId = "CS325-01",
                    CourseCode = "CS325",
                    Term = "HK1 2026-2027",
                    LecturerId = "GV0048",
                    RoomId = "C102",
                    DayOfWeek = 2,
                    StartTime = new TimeSpan(9, 0, 0),
                    EndTime = new TimeSpan(10, 30, 0),
                    Capacity = 50,
                    Enrolled = 35
                },
                // 12 - CS330
                new Section
                {
                    SectionId = "CS330-01",
                    CourseCode = "CS330",
                    Term = "HK1 2026-2027",
                    LecturerId = "GV0031",
                    RoomId = "A202",
                    DayOfWeek = 4,
                    StartTime = new TimeSpan(15, 0, 0),
                    EndTime = new TimeSpan(16, 30, 0),
                    Capacity = 50,
                    Enrolled = 22
                },
                // 13 - CS340
                new Section
                {
                    SectionId = "CS340-01",
                    CourseCode = "CS340",
                    Term = "HK1 2026-2027",
                    LecturerId = "GV0008",
                    RoomId = "B203",
                    DayOfWeek = 5,
                    StartTime = new TimeSpan(9, 0, 0),
                    EndTime = new TimeSpan(10, 30, 0),
                    Capacity = 60,
                    Enrolled = 50
                },
                // 14 - CS350
                new Section
                {
                    SectionId = "CS350-01",
                    CourseCode = "CS350",
                    Term = "HK1 2026-2027",
                    LecturerId = "GV0012",
                    RoomId = "A305",
                    DayOfWeek = 6,
                    StartTime = new TimeSpan(9, 0, 0),
                    EndTime = new TimeSpan(10, 30, 0),
                    Capacity = 45,
                    Enrolled = 30
                },
                // 15 - CS360
                new Section
                {
                    SectionId = "CS360-01",
                    CourseCode = "CS360",
                    Term = "HK1 2026-2027",
                    LecturerId = "GV0053",
                    RoomId = "C205",
                    DayOfWeek = 3,
                    StartTime = new TimeSpan(18, 0, 0),
                    EndTime = new TimeSpan(19, 30, 0),
                    Capacity = 40,
                    Enrolled = 27
                },
                // 16 - MA104
                new Section
                {
                    SectionId = "MA104-01",
                    CourseCode = "MA104",
                    Term = "HK1 2026-2027",
                    LecturerId = "GV0020",
                    RoomId = "C102",
                    DayOfWeek = 5,
                    StartTime = new TimeSpan(13, 0, 0),
                    EndTime = new TimeSpan(14, 30, 0),
                    Capacity = 50,
                    Enrolled = 30
                },
                // 17 - MA201
                new Section
                {
                    SectionId = "MA201-01",
                    CourseCode = "MA201",
                    Term = "HK1 2026-2027",
                    LecturerId = "GV0042",
                    RoomId = "C102",
                    DayOfWeek = 2,
                    StartTime = new TimeSpan(15, 0, 0),
                    EndTime = new TimeSpan(16, 30, 0),
                    Capacity = 50,
                    Enrolled = 35
                },
                // 18 - EN101
                new Section
                {
                    SectionId = "EN101-01",
                    CourseCode = "EN101",
                    Term = "HK1 2026-2027",
                    LecturerId = "GV0048",
                    RoomId = "C201",
                    DayOfWeek = 3,
                    StartTime = new TimeSpan(15, 0, 0),
                    EndTime = new TimeSpan(16, 30, 0),
                    Capacity = 45,
                    Enrolled = 38
                },
                // 19 - EN201
                new Section
                {
                    SectionId = "EN201-01",
                    CourseCode = "EN201",
                    Term = "HK1 2026-2027",
                    LecturerId = "GV0048",
                    RoomId = "C201",
                    DayOfWeek = 6,
                    StartTime = new TimeSpan(13, 0, 0),
                    EndTime = new TimeSpan(14, 30, 0),
                    Capacity = 45,
                    Enrolled = 25
                },
                // 20 - CS101
                new Section
                {
                    SectionId = "CS101-01",
                    CourseCode = "CS101",
                    Term = "HK1 2026-2027",
                    LecturerId = "GV0008",
                    RoomId = "A201",
                    DayOfWeek = 4,
                    StartTime = new TimeSpan(19, 0, 0),
                    EndTime = new TimeSpan(20, 30, 0),
                    Capacity = 60,
                    Enrolled = 42
                }
            });

            // =========================================================
            // STUDENTS
            // =========================================================
            Students.Add(new Student
            {
                StudentId = "SV2411869",
                FullName = "Huỳnh Phúc Lâm",
                Email = "2411869@dlu.edu.vn",
                Faculty = "CNTT",
                IntakeYear = 2026,
                ClassCode = "CNTT01",
                CompletedCourseCodes = new HashSet<string> { "CS101" }
            });
            Students.Add(new Student
            {
                StudentId = "SV2411870",
                FullName = "Nguyễn Minh Anh",
                Email = "2411870@dlu.edu.vn",
                Faculty = "CNTT",
                IntakeYear = 2026,
                ClassCode = "CNTT01",
                CompletedCourseCodes = new HashSet<string> { "CS101", "MA104", "EN101" }
            });
            Students.Add(new Student
            {
                StudentId = "SV2411871",
                FullName = "Trần Quốc Bảo",
                Email = "2411871@dlu.edu.vn",
                Faculty = "CNTT",
                IntakeYear = 2025,
                ClassCode = "CNTT02",
                CompletedCourseCodes = new HashSet<string>
                {
                    "CS101", "CS201", "CS208", "CS305", "CS310",
                    "MA104", "MA201", "EN101"
                }
            });
            Students.Add(new Student
            {
                StudentId = "SV2411872",
                FullName = "Lê Hoàng Nam",
                Email = "2411872@dlu.edu.vn",
                Faculty = "CNTT",
                IntakeYear = 2025,
                ClassCode = "CNTT03",
                CompletedCourseCodes = new HashSet<string> { "CS101", "CS201", "CS208", "CS310", "MA104" }
            });

            // =========================================================
            // ACCOUNTS
            // =========================================================
            Accounts.AddRange(new[]
            {
                new Account { Username = "sinhvien", PasswordHash = "000", Role = Role.Student, LinkedId = "SV2411869" },
                new Account { Username = "SV2411870", PasswordHash = "123456", Role = Role.Student, LinkedId = "SV2411870" },
                new Account { Username = "SV2411871", PasswordHash = "123456", Role = Role.Student, LinkedId = "SV2411871" },
                new Account { Username = "SV2411872", PasswordHash = "123456", Role = Role.Student, LinkedId = "SV2411872" },
                new Account { Username = "admin", PasswordHash = "111", Role = Role.Admin, LinkedId = null }
            });

            // =========================================================
            // ENROLLMENTS
            // =========================================================
            Enrollments.AddRange(new[]
            {
                // Sinh viên SV2411869
                new Enrollment { StudentId = "SV2411869", SectionId = "CS201-01", RegisteredAt = new DateTime(2026, 8, 20, 8, 35, 0), Status = EnrollmentStatus.Enrolled },
                new Enrollment { StudentId = "SV2411869", SectionId = "CS208-02", RegisteredAt = new DateTime(2026, 8, 20, 8, 40, 0), Status = EnrollmentStatus.Enrolled },
                // Sinh viên SV2411870
                new Enrollment { StudentId = "SV2411870", SectionId = "MA104-01", RegisteredAt = new DateTime(2026, 8, 21, 9, 10, 0), Status = EnrollmentStatus.Enrolled },
                new Enrollment { StudentId = "SV2411870", SectionId = "EN101-01", RegisteredAt = new DateTime(2026, 8, 21, 9, 15, 0), Status = EnrollmentStatus.Enrolled },
                // Sinh viên SV2411871
                new Enrollment { StudentId = "SV2411871", SectionId = "CS305-02", RegisteredAt = new DateTime(2026, 8, 21, 10, 20, 0), Status = EnrollmentStatus.Enrolled },
                new Enrollment { StudentId = "SV2411871", SectionId = "CS340-01", RegisteredAt = new DateTime(2026, 8, 21, 10, 25, 0), Status = EnrollmentStatus.Enrolled },
                // Sinh viên SV2411872
                new Enrollment { StudentId = "SV2411872", SectionId = "CS350-01", RegisteredAt = new DateTime(2026, 8, 22, 7, 30, 0), Status = EnrollmentStatus.Enrolled },
                new Enrollment { StudentId = "SV2411872", SectionId = "CS360-01", RegisteredAt = new DateTime(2026, 8, 22, 7, 35, 0), Status = EnrollmentStatus.Enrolled }
            });
        }
    }
}