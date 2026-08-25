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

        public List<Role> Roles { get; } = new() { Role.Student, Role.Admin };

        private Role _selectedRole = Role.Student;
        public Role SelectedRole
        {
            get => _selectedRole;
            set
            {
                if (SetProperty(ref _selectedRole, value))
                {
                    RaisePropertyChanged(nameof(IsStudentRole));
                    RaisePropertyChanged(nameof(IsAdminRole));
                }
            }
        }

        public bool IsStudentRole => SelectedRole == Role.Student;
        public bool IsAdminRole => SelectedRole == Role.Admin;

        public RelayCommand SelectStudentRoleCommand { get; }
        public RelayCommand SelectAdminRoleCommand { get; }

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
            SelectStudentRoleCommand = new RelayCommand(() => SelectedRole = Role.Student);
            SelectAdminRoleCommand = new RelayCommand(() => SelectedRole = Role.Admin);
        }

        private void DoLogin()
        {
            var result = _auth.Login(Username.Trim(), Password, SelectedRole);
            if (!result.Success)
            {
                ErrorMessage = result.ErrorMessage;
                return;
            }

            LoginSucceeded?.Invoke(result.Account!, result.Student);
        }
    }
}
