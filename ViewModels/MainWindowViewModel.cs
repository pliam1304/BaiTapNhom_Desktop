using EduPath.Avalonia.Models;
using EduPath.Avalonia.Services;

namespace EduPath.Avalonia.ViewModels
{
    /// <summary>
    /// Điều phối toàn ứng dụng: Đăng nhập -> Shell Sinh viên hoặc Shell Quản trị.
    /// Việc chuyển "CurrentView" được bọc trong TransitioningContentControl ở MainWindow.axaml
    /// để có hiệu ứng fade + trượt thay vì bản WinForms cũ (ẩn/hiện Form đột ngột).
    /// </summary>
    public class MainWindowViewModel : ViewModelBase
    {
        private object _currentView;
        public object CurrentView
        {
            get => _currentView;
            private set => SetProperty(ref _currentView, value);
        }

        public MainWindowViewModel()
        {
            _currentView = CreateLogin();
        }

        private LoginViewModel CreateLogin()
        {
            var vm = new LoginViewModel();
            vm.LoginSucceeded += OnLoginSucceeded;
            return vm;
        }

        private void OnLoginSucceeded(Account account, Student? student)
        {
            SessionContext.SignIn(account, student);

            if (account.Role == Role.Admin)
            {
                var admin = new AdminShellViewModel();
                admin.LogoutRequested += SignOut;
                CurrentView = admin;
            }
            else
            {
                var stu = new StudentShellViewModel(student!);
                stu.LogoutRequested += SignOut;
                CurrentView = stu;
            }
        }

        private void SignOut()
        {
            SessionContext.SignOut();
            CurrentView = CreateLogin();
        }
    }
}
