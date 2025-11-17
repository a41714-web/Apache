using Microsoft.Extensions.Logging;
using Apache.Views;
using Apache.ViewModels;
using Apache.Data;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Controls;

namespace Apache
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Register DatabaseConfig using factory that reads configuration
            builder.Services.AddSingleton(sp => DatabaseConfig.CreateFromConfiguration());

            // Register ViewModels
            builder.Services.AddSingleton<LoginViewModel>();

            // Register Login view and ViewModel always
            builder.Services.AddSingleton<LoginPage>();

            // Register platform-specific pages and ViewModels
            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                // Android: Customer only
                builder.Services.AddSingleton<CustomerPage>();
                builder.Services.AddSingleton<CustomerViewModel>();

                // Register route for navigation
                Routing.RegisterRoute("customer", typeof(CustomerPage));
            }
            else 
            {
                // Desktop (Windows/macOS): Admin only
                builder.Services.AddSingleton<AdminPage>();
                builder.Services.AddSingleton<AdminViewModel>();

                // Register route for navigation
                Routing.RegisterRoute("admin", typeof(AdminPage));
            }

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
