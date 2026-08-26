using System;
using EduPath.Avalonia.Models;

namespace EduPath.Avalonia.ViewModels
{
    public class SectionRow
    {
        public Section Section { get; }

        public Course? Course { get; }

        public Lecturer? Lecturer { get; }


        // =====================================================
        // SECTION
        // =====================================================

        public string SectionId =>
            Section.SectionId;

        public string CourseCode =>
            Section.CourseCode;

        public string LecturerId =>
            Section.LecturerId;

        public string Term =>
            Section.Term;

        public string RoomId =>
            Section.RoomId;


        // =====================================================
        // COURSE
        // =====================================================

        public string CourseName =>
            Course?.CourseName
            ?? "Chưa xác định môn học";

        public int Credits =>
            Course?.Credits ?? 0;


        // =====================================================
        // LECTURER
        // =====================================================

        public string LecturerName =>
            Lecturer?.FullName
            ?? "Chưa phân công";


        // =====================================================
        // SCHEDULE
        // =====================================================

        public string Schedule
        {
            get
            {
                string day =
                    GetDayName(
                        Section.DayOfWeek
                    );

                string start =
                    Section.StartTime
                        .ToString(@"hh\:mm");

                string end =
                    Section.EndTime
                        .ToString(@"hh\:mm");

                return $"{day} {start}-{end}";
            }
        }


        private static string GetDayName(
            int day)
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

        public int Capacity =>
            Section.Capacity;

        public int Enrolled =>
            Section.Enrolled;


        public int RemainingSeats
        {
            get
            {
                int remaining =
                    Capacity - Enrolled;

                return remaining < 0
                    ? 0
                    : remaining;
            }
        }


        public bool IsFull =>
            RemainingSeats <= 0;


        public string SeatsLabel =>
            $"{RemainingSeats}/{Capacity} chỗ";


        // =====================================================
        // STATUS
        // =====================================================

        public string StatusText
        {
            get
            {
                if (!Section.IsOpen)
                {
                    return "Đã đóng";
                }

                if (IsFull)
                {
                    return "Đã đầy";
                }

                return "Đang mở";
            }
        }


        public string StatusBadge
        {
            get
            {
                if (!Section.IsOpen)
                {
                    return "status-closed";
                }

                if (IsFull)
                {
                    return "status-full";
                }

                return "status-open";
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