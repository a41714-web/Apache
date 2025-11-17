using Apache.Data;
using Apache.Models;
using Apache.Services;
using System.Windows.Input;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Controls;

namespace Apache.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private string _email;
        private string _password;
        private string _errorMessage;
        private bool _isCustomerMode = true;
        private bool _canToggleMode = false;
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

        public bool CanToggleMode
        {
            get => _canToggleMode;
            set => SetProperty(ref _canToggleMode, value);
        }

        public ICommand LoginCommand { get; }
        public ICommand ToggleModeCommand { get; }

        public LoginViewModel()
        {
            _repository = DataRepository.Instance;
            _logger = LoggingService.Instance;

            DetectPlatformAndSetMode();

            LoginCommand = new RelayCommand(async () => await ExecuteLogin());
            ToggleModeCommand = new RelayCommand(() => ToggleLoginMode());
        }

        private void DetectPlatformAndSetMode()
        {
            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                IsCustomerMode = true;
                CanToggleMode = false;
                _logger.LogDebug("Platform: Android - Customer Mode");
            }
            else
            {
                IsCustomerMode = false;
                CanToggleMode = false;
                _logger.LogDebug("Platform: Desktop - Admin Mode");
            }
        }

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
                await Task.Delay(500); // Simulate network delay

                if (IsCustomerMode)
                {
                    var customer = _repository.AuthenticateCustomer(Email, Password);
                    if (customer != null)
                    {
                        _logger.LogInfo($"Customer login successful: {Email}");
                        // Use proper navigation with parameters
                        await Shell.Current.GoToAsync($"customer?id={customer.Id}");
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
                        await Shell.Current.GoToAsync($"admin?id={admin.Id}");
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
                await Application.Current.MainPage.DisplayAlert("Login Error", ex.Message, "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ToggleLoginMode()
        {
            if (!CanToggleMode)
            {
                _logger.LogDebug("Login mode toggle disabled");
                return;
            }

            IsCustomerMode = !IsCustomerMode;
            Email = string.Empty;
            Password = string.Empty;
            ErrorMessage = string.Empty;
            _logger.LogDebug($"Mode: {(IsCustomerMode ? "Customer" : "Admin")}");
        }
    }
}