using Apache.Data;
using Apache.Models;
using Apache.Services;
using System.Windows.Input;

namespace Apache.ViewModels
{
    /// <summary>
    /// ViewModel for authentication (login) screen.
    /// Handles both customer and admin login scenarios.
    /// </summary>
    public class LoginViewModel : BaseViewModel
    {
        private string _email;
        private string _password;
        private string _errorMessage;
        private bool _isCustomerMode = true;
        private readonly DataRepository _repository;
        private readonly LoggingService _logger;

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool IsCustomerMode
        {
            get => _isCustomerMode;
            set => SetProperty(ref _isCustomerMode, value);
        }

        public ICommand LoginCommand { get; }
        public ICommand ToggleModeCommand { get; }

        public LoginViewModel()
        {
            _repository = DataRepository.Instance;
            _logger = LoggingService.Instance;
            
            LoginCommand = new RelayCommand(async () => await ExecuteLogin());
            ToggleModeCommand = new RelayCommand(() => ToggleLoginMode());
        }

        /// <summary>
        /// Attempts to authenticate the user with provided credentials.
        /// </summary>
        private async Task ExecuteLogin()
        {
            try
            {
                ErrorMessage = string.Empty;

                if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
                {
                    throw new ArgumentException("Email and password are required");
                }

                IsLoading = true;

                // Simulate network delay
                await Task.Delay(500);

                if (IsCustomerMode)
                {
                    var customer = _repository.AuthenticateCustomer(Email, Password);
                    if (customer != null)
                    {
                        _logger.LogInfo($"Customer login successful: {Email}");
                        await Shell.Current.GoToAsync($"///customer?id={customer.Id}");
                    }
                    else
                    {
                        throw new InvalidOperationException("Invalid email or password");
                    }
                }
                else
                {
                    var admin = _repository.AuthenticateAdmin(Email, Password);
                    if (admin != null)
                    {
                        _logger.LogInfo($"Admin login successful: {Email}");
                        await Shell.Current.GoToAsync($"///admin?id={admin.Id}");
                    }
                    else
                    {
                        throw new InvalidOperationException("Invalid admin credentials");
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                _logger.LogError("Login failed", ex);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Toggles between customer and admin login modes.
        /// </summary>
        private void ToggleLoginMode()
        {
            IsCustomerMode = !IsCustomerMode;
            Email = string.Empty;
            Password = string.Empty;
            ErrorMessage = string.Empty;
            _logger.LogDebug($"Login mode switched to: {(IsCustomerMode ? "Customer" : "Admin")}");
        }
    }
}
