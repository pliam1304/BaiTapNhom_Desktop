using EduPath.Avalonia.Models;

namespace EduPath.Avalonia.ViewModels
{
    public class NavItem
    {
        public string Key { get; init; } = string.Empty;
        public string Label { get; init; } = string.Empty;
        public string Icon { get; init; } = string.Empty; // ký tự glyph đơn giản, không cần font icon ngoài
    }

    /// <summary>
    /// Shell của Sinh viên — thay cho StudentShellBuilder (sidebar trái) trong bản WinForms.
    /// Ở đây dùng thanh điều hướng dạng "pill" nằm ngang phía trên, mỗi lần chuyển trang sẽ
    /// phát lại animation fade/slide qua TransitioningContentControl.
    /// </summary>
    public class StudentShellViewModel : ViewModelBase
    {
        public event Action? LogoutRequested;

        public Student Student { get; }

        public List<NavItem> NavItems { get; } = new()
        {
            new NavItem { Key = "dashboard", Label = "Tổng quan", Icon = "🏠" },
            new NavItem { Key = "open",      Label = "Đăng ký học phần", Icon = "📚" },
            new NavItem { Key = "enrolled",  Label = "Đã đăng ký", Icon = "✅" },
            new NavItem { Key = "timetable", Label = "Thời khóa biểu", Icon = "🗓" },
            new NavItem { Key = "history",   Label = "Lịch sử", Icon = "🕘" },
        };

        private NavItem _selectedNav;
        public NavItem SelectedNav
        {
            get => _selectedNav;
            set
            {
                if (SetProperty(ref _selectedNav, value))
                    Navigate(value.Key);
            }
        }

        private object _currentPage = null!;
        public object CurrentPage
        {
            get => _currentPage;
            private set => SetProperty(ref _currentPage, value);
        }

        public RelayCommand LogoutCommand { get; }

        // Trang được tạo lười và giữ lại (giữ trạng thái filter/scroll khi quay lại)
        private readonly Dictionary<string, object> _pageCache = new();

        public StudentShellViewModel(Student student)
        {
            Student = student;
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
                    "dashboard" => new StudentDashboardViewModel(Student, this),
                    "open" => new OpenSectionsViewModel(Student, this),
                    "enrolled" => new EnrolledSectionsViewModel(Student, this),
                    "timetable" => new TimetableViewModel(Student),
                    "history" => new HistoryViewModel(Student),
                    _ => new StudentDashboardViewModel(Student, this)
                };
                _pageCache[key] = page;
            }
            else if (page is IRefreshable refreshable)
            {
                refreshable.Refresh();
            }

            CurrentPage = page;
            var match = NavItems.FirstOrDefault(n => n.Key == key);
            if (match != null) _selectedNav = match;
        }

        /// <summary>Xóa cache trang để lần Navigate kế tiếp build lại dữ liệu mới nhất (gọi sau khi đăng ký/hủy).</summary>
        public void InvalidateAll() => _pageCache.Clear();
    }

    public interface IRefreshable
    {
        void Refresh();
    }
}
