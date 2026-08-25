// View/Features/Shells/AdminShellViewModel.cs
using System;
using System.Collections.Generic;
using System.Linq;

namespace EduPath.Avalonia.ViewModels
{
    public class AdminShellViewModel : ViewModelBase
    {
        public event Action? LogoutRequested;

        // Đã cập nhật sử dụng IconPath với file ảnh .png giống hệt cấu trúc của Student
        public List<NavItem> NavItems { get; } = new()
        {
            new NavItem { Key = "dashboard", Label = "Tổng quan", IconPath = "avares://EduPath.Avalonia/images_icons/tổng quan.png" },
            new NavItem { Key = "courses",   Label = "Môn học", IconPath = "avares://EduPath.Avalonia/images_icons/môn học.png" },
            new NavItem { Key = "sections",  Label = "Lớp học phần", IconPath = "avares://EduPath.Avalonia/images_icons/danh sách.png" }
        };

        private NavItem _selectedNav;
        public NavItem SelectedNav
        {
            get => _selectedNav;
            set { if (SetProperty(ref _selectedNav, value)) Navigate(value.Key); }
        }

        private object _currentPage = null!;
        public object CurrentPage
        {
            get => _currentPage;
            private set => SetProperty(ref _currentPage, value);
        }

        public RelayCommand LogoutCommand { get; }

        private readonly Dictionary<string, object> _pageCache = new();

        public AdminShellViewModel()
        {
            LogoutCommand = new RelayCommand(() => LogoutRequested?.Invoke());
            _selectedNav = NavItems[0];
            Navigate("dashboard");
        }

        public void Navigate(string key)
        {
            if (!_pageCache.TryGetValue(key, out var page))
            {
                page = key switch
                {
                    "dashboard" => new AdminDashboardViewModel(this),
                    "courses" => new CoursesAdminViewModel(),
                    "sections" => new SectionsAdminViewModel(),
                    _ => new AdminDashboardViewModel(this)
                };
                _pageCache[key] = page;
            }
            else if (page is IRefreshable r)
            {
                r.Refresh();
            }

            CurrentPage = page;
            var match = NavItems.FirstOrDefault(n => n.Key == key);
            if (match != null) _selectedNav = match;
        }

        public void InvalidateAll() => _pageCache.Clear();
    }
}