using EduPath.Avalonia.Models;
using EduPath.Avalonia.Services;

namespace EduPath.Avalonia.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly AuthService _auth = new();

        public event Action<Account, Student?>? LoginSucceeded;

        private string _username = string.Empty;
        public string Username
        {
            get => _username;
            set { SetProperty(ref _username, value); ErrorMessage = null; }
        }

        private string _password = string.Empty;
        public string Password
        {
            get => _password;
            set { SetProperty(ref _password, value); ErrorMessage = null; }
        }

        private string? _errorMessage;
        public string? ErrorMessage
        {
            get => _errorMessage;
            set { SetProperty(ref _errorMessage, value); RaisePropertyChanged(nameof(HasError)); }
        }

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        public RelayCommand LoginCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(DoLogin);
        }

        private void DoLogin()
        {
            // Gọi hàm đăng nhập tự động nhận diện vai trò (không truyền Role vào nữa)
            var result = _auth.Login(Username.Trim(), Password);
            if (!result.Success)
            {
                ErrorMessage = result.ErrorMessage;
                return;
            }

            // Đăng nhập thành công, truyền thông tin tài khoản và đối tượng sang tầng xử lý tiếp theo
            LoginSucceeded?.Invoke(result.Account!, result.Student);
        }
    }
}