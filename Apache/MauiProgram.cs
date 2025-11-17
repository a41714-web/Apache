using Microsoft.Extensions.Logging;
using Apache.Views;
using Apache.ViewModels;
using Apache.Data;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Controls;
using System;
using System.Linq;

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

            // Ensure Config.xml is available in AppDataDirectory
            EnsureConfigFileExists();

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
                builder.Services.AddSingleton<CustomerAndroidPage>();
                builder.Services.AddSingleton<CustomerViewModel>();

                // Register route for navigation
                Routing.RegisterRoute("customer", typeof(CustomerAndroidPage));
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

        /// <summary>
        /// Ensures Config.xml exists in the app data directory by copying it from resources if needed.
        /// </summary>
        private static void EnsureConfigFileExists()
        {
            try
            {
                var configPath = Path.Combine(FileSystem.AppDataDirectory, "Config.xml");
                
                // If config doesn't exist, try to create it from the default
                if (!File.Exists(configPath))
                {
                    // Try to load from embedded resource or create default
                    var resourcePath = "Config.xml";
                    var assembly = typeof(MauiProgram).Assembly;
                    var embeddedResourceName = assembly.GetManifestResourceNames()
                        .FirstOrDefault(rn => rn.EndsWith(resourcePath));

                    if (!string.IsNullOrEmpty(embeddedResourceName))
                    {
                        using (var stream = assembly.GetManifestResourceStream(embeddedResourceName))
                        {
                            if (stream != null)
                            {
                                using (var fileStream = File.Create(configPath))
                                {
                                    stream.CopyTo(fileStream);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error ensuring Config.xml exists: {ex.Message}");
            }
        }
    }
}
