using Microsoft.Maui.Devices;

namespace Apache
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Register platform specific routes
            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                Routing.RegisterRoute("customer", typeof(Views.CustomerPage));
            }
            else
            {
                Routing.RegisterRoute("admin", typeof(Views.AdminPage));
            }
        }
    }
}
