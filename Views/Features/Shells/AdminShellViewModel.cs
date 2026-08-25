// Views/Features/Shells/AdminShellViewModel.cs

using System;
using System.Collections.Generic;
using System.Linq;

namespace EduPath.Avalonia.ViewModels
{
    public class AdminShellViewModel : ViewModelBase
    {
        public event Action? LogoutRequested;


        // =====================================================
        // MENU ADMIN
        // =====================================================
        // Sử dụng NavItem dùng chung được khai báo trong
        // StudentShellViewModel.cs
        public List<NavItem> NavItems { get; } = new()
        {
            new NavItem(
                "dashboard",
                "Tổng quan",
                "images_icons/tongQuan.png"
            ),

            new NavItem(
                "courses",
                "Môn học",
                "images_icons/monHoc.png"
            ),

            new NavItem(
                "sections",
                "Lớp học phần",
                "images_icons/danhSach.png"
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
        public AdminShellViewModel()
        {
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
                        new AdminDashboardViewModel(this),

                    "courses" =>
                        new CoursesAdminViewModel(),

                    "sections" =>
                        new SectionsAdminViewModel(),

                    _ =>
                        new AdminDashboardViewModel(this)
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
}