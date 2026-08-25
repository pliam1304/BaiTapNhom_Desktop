namespace EduPath.Avalonia.ViewModels
{
    public class AdminShellViewModel : ViewModelBase
    {
        public event Action? LogoutRequested;

        public List<NavItem> NavItems { get; } = new()
        {
            new NavItem { Key = "dashboard", Label = "Tổng quan", Icon = "📊" },
            new NavItem { Key = "sections",  Label = "Lớp học phần", Icon = "🏫" },
            new NavItem { Key = "courses",   Label = "Học phần", Icon = "📘" },
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
                    "sections" => new SectionsAdminViewModel(),
                    "courses" => new CoursesAdminViewModel(),
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
