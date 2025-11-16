using Microsoft.Extensions.DependencyInjection;

namespace Apache
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }

        protected override async void OnStart()
        {
            base.OnStart();
            // Navigate to login page
            await Shell.Current.GoToAsync("//login");
        }
    }
}