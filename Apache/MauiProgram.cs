using Microsoft.Extensions.Logging;
using Apache.Views;
using Apache.Data;

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

            // Initialize database
            _ = DatabaseConfig.Instance;

            // Register Views and ViewModels
            builder.Services.AddSingleton<LoginPage>();
            builder.Services.AddSingleton<CustomerPage>();
            builder.Services.AddSingleton<AdminPage>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
