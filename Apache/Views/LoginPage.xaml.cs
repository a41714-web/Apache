using Apache.ViewModels;
using Microsoft.Maui.Controls;

namespace Apache.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
        BindingContext = new LoginViewModel();
    }
}
