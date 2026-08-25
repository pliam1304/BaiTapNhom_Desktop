// Views/Features/Shells/StudentShellViewModel.cs

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EduPath.Avalonia.Models;
using Avalonia.Media.Imaging;

namespace EduPath.Avalonia.ViewModels
{
    // =========================================================
    // NAV ITEM DÙNG CHUNG CHO STUDENT + ADMIN
    // =========================================================
    public class NavItem
    {
        public string Key { get; init; } = string.Empty;

        public string Label { get; init; } = string.Empty;

        // Ví dụ:
        // images_icons/tongQuan.png
        public string IconPath { get; init; } = string.Empty;

        public Bitmap? IconImage { get; }

        public NavItem()
        {
        }

        public NavItem(string key, string label, string iconPath)
        {
            Key = key;
            Label = label;
            IconPath = iconPath;

            IconImage = TaoBitmap(iconPath);
        }

        // =====================================================
        // ĐỌC ẢNH TRỰC TIẾP TỪ FILE
        // =====================================================
        private static Bitmap? TaoBitmap(string imagePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(imagePath))
                {
                    return null;
                }

                // AppContext.BaseDirectory:
                // bin/Debug/net8.0/
                string fullPath = Path.Combine(
                    AppContext.BaseDirectory,
                    imagePath.Replace(
                        '/',
                        Path.DirectorySeparatorChar
                    )
                );

                //Console.WriteLine($"[NavItem] Đang load ảnh:");
                //Console.WriteLine($"[NavItem] {fullPath}");

                if (!File.Exists(fullPath))
                {
                    Console.WriteLine(
                        $"[NavItem] KHÔNG TÌM THẤY FILE: {fullPath}"
                    );

                    return null;
                }

                return new Bitmap(fullPath);
            }
            catch (Exception ex)
            {
                    // //Console.WriteLine(
                    //     $"[NavItem] Lỗi khi load ảnh: {imagePath}"
                    // );

                Console.WriteLine(ex);

                return null;
            }
        }
    }


    // =========================================================
    // STUDENT SHELL
    // =========================================================
    public class StudentShellViewModel : ViewModelBase
    {
        public event Action? LogoutRequested;

        public Student Student { get; }

        // =====================================================
        // MENU STUDENT
        // =====================================================
        public List<NavItem> NavItems { get; } = new()
        {
            new NavItem(
                "dashboard",
                "Tổng quan",
                "images_icons/tongQuan.png"
            ),
            new NavItem(
                "open", 
                "Đăng ký học phần",
                "images_icons/dangKiHocPhan.png"
            ),
            new NavItem(
                "enrolled", // Đổi từ "open" thành "enrolled" để map với EnrolledSectionsViewModel
                "Lớp học phần",
                "images_icons/danhSach.png"
            ),
            new NavItem(
                "timetable",
                "Thời khoá biểu",
                "images_icons/thoiKhoaBieu.png"
            ),
            // Các trang dưới đây chưa có View trong hàm Navigate, 
            // tạm thời đặt key riêng để không bị đụng độ với "open"
            new NavItem(
                "fee", 
                "Học phí",
                "images_icons/hocPhi.png"
            ),
            new NavItem(
                "notification", 
                "Thông báo",
                "images_icons/thongBao.png"
            )
        };


        // =====================================================
        // SELECTED NAV
        // =====================================================
        private NavItem _selectedNav;

        public NavItem SelectedNav
        {
            get => _selectedNav;

            set
            {
                if (SetProperty(ref _selectedNav, value))
                {
                    Navigate(value.Key);
                }
            }
        }


        // =====================================================
        // CURRENT PAGE
        // =====================================================
        private object _currentPage = null!;

        public object CurrentPage
        {
            get => _currentPage;

            private set => SetProperty(
                ref _currentPage,
                value
            );
        }


        // =====================================================
        // LOGOUT
        // =====================================================
        public RelayCommand LogoutCommand { get; }


        // =====================================================
        // PAGE CACHE
        // =====================================================
        private readonly Dictionary<string, object> _pageCache = new();


        // =====================================================
        // CONSTRUCTOR
        // =====================================================
        public StudentShellViewModel(Student student)
        {
            Student = student;

            LogoutCommand = new RelayCommand(
                () => LogoutRequested?.Invoke()
            );

            _selectedNav = NavItems[0];

            Navigate("dashboard");
        }


        // =====================================================
        // NAVIGATE
        // =====================================================
        public void Navigate(string key)
        {
            if (!_pageCache.TryGetValue(key, out var page))
            {
                page = key switch
                {
                    "dashboard" =>
                        new StudentDashboardViewModel(
                            Student,
                            this
                        ),

                    "open" =>
                        new OpenSectionsViewModel(
                            Student,
                            this
                        ),

                    "enrolled" =>
                        new StudentOpenSectionsViewModel(
                            Student,
                            this
                        ),

                    "timetable" =>
                        new TimetableViewModel(
                            Student
                        ),

                    "history" =>
                        new HistoryViewModel(
                            Student
                        ),

                    _ =>
                        new StudentDashboardViewModel(
                            Student,
                            this
                        )
                };

                _pageCache[key] = page;
            }
            else if (page is IRefreshable refreshable)
            {
                refreshable.Refresh();
            }

            CurrentPage = page;

            var match = NavItems.FirstOrDefault(
                n => n.Key == key
            );

            if (match != null)
            {
                _selectedNav = match;
            }
        }


        // =====================================================
        // CLEAR CACHE
        // =====================================================
        public void InvalidateAll()
        {
            _pageCache.Clear();
        }
    }


    // =========================================================
    // REFRESHABLE
    // =========================================================
    public interface IRefreshable
    {
        void Refresh();
    }
}