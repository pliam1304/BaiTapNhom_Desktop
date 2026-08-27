using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Avalonia.Media.Imaging;
using EduPath.Avalonia.Models;

namespace EduPath.Avalonia.ViewModels
{
    // =========================================================
    // NAV ITEM
    // =========================================================
    public class NavItem
    {
        public string Key { get; init; } = string.Empty;

        public string Label { get; init; } = string.Empty;

        public string IconPath { get; init; } = string.Empty;

        public Bitmap? IconImage { get; }

        public NavItem()
        {
        }

        public NavItem(
            string key,
            string label,
            string iconPath)
        {
            Key = key;
            Label = label;
            IconPath = iconPath;

            IconImage = TaoBitmap(iconPath);
        }

        private static Bitmap? TaoBitmap(string imagePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(imagePath))
                {
                    return null;
                }

                string fullPath = Path.Combine(
                    AppContext.BaseDirectory,
                    imagePath.Replace(
                        '/',
                        Path.DirectorySeparatorChar
                    )
                );

                Console.WriteLine(
                    $"[NavItem] Đang load ảnh: {fullPath}"
                );

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
                Console.WriteLine(
                    $"[NavItem] Lỗi load ảnh: {imagePath}"
                );

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
        // MENU
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
                "enrolled",
                "Lớp học phần",
                "images_icons/danhSach.png"
            ),

            new NavItem(
                "timetable",
                "Thời khoá biểu",
                "images_icons/thoiKhoaBieu.png"
            ),

            new NavItem(
                "tuition",
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
                if (SetProperty(
                    ref _selectedNav,
                    value))
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

            private set
            {
                SetProperty(
                    ref _currentPage,
                    value
                );
            }
        }


        // =====================================================
        // LOGOUT
        // =====================================================

        public RelayCommand LogoutCommand { get; }


        // =====================================================
        // PAGE CACHE
        // =====================================================

        private readonly Dictionary<string, object> _pageCache
            = new();


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
            Console.WriteLine(
                $"[StudentShell] Navigate: {key}"
            );

            if (!_pageCache.TryGetValue(
                key,
                out var page))
            {
                page = key switch
                {
                    // ==============================
                    // TỔNG QUAN
                    // ==============================

                    "dashboard" =>
                        new StudentDashboardViewModel(
                            Student,
                            this
                        ),


                    // ==============================
                    // ĐĂNG KÝ HỌC PHẦN
                    // ==============================

                    "open" =>
                        new OpenSectionsViewModel(
                            Student,
                            this
                        ),


                    // ==============================
                    // LỚP HỌC PHẦN ĐÃ ĐĂNG KÝ
                    // ==============================

                    "enrolled" =>
                        new StudentOpenSectionsViewModel(
                            Student,
                            this
                        ),


                    // ==============================
                    // THỜI KHÓA BIỂU
                    // ==============================

                    "timetable" =>
                        new TimetableViewModel(
                            Student
                        ),



                    // ==============================
                    // HOÁ ĐƠN
                    // ==============================
                    "tuition"  =>
                        new TuitionViewModel(
                            Student
                        ),

                    // ==============================
                    // LỊCH SỬ
                    // ==============================

                    "history" =>
                        new HistoryViewModel(
                            Student
                        ),


                    // ==============================
                    // MẶC ĐỊNH
                    // ==============================

                    _ =>
                        new StudentDashboardViewModel(
                            Student,
                            this
                        )
                };

                _pageCache[key] = page;
            }
            else
            {
                if (page is IRefreshable refreshable)
                {
                    refreshable.Refresh();
                }
            }

            CurrentPage = page;

            NavItem? match =
                NavItems.FirstOrDefault(
                    n => n.Key == key
                );

            if (match != null)
            {
                _selectedNav = match;

                RaisePropertyChanged(
                    nameof(SelectedNav)
                );
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