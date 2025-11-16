using Apache.Data;
using Apache.ViewModels;

namespace Apache.Views;

public partial class AdminPage : ContentPage
{
    private readonly AdminViewModel _viewModel;

    public AdminPage()
    {
        InitializeComponent();
        _viewModel = new AdminViewModel();
        BindingContext = _viewModel;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        
        // Get admin ID from query parameters
        if (Shell.Current.CurrentState.Location.OriginalString.Contains("id="))
        {
            var idString = Shell.Current.CurrentState.Location.OriginalString.Split("id=")[1];
            if (int.TryParse(idString, out int adminId))
            {
                // For now, we'll use a direct admin lookup
                // In a real app, you'd retrieve from database/repository
                var repository = DataRepository.Instance;
                var allOrders = repository.GetAllOrders();
                
                // Initialize the admin view (ID is verified during login)
                var admin = new Apache.Models.Admin 
                { 
                    Id = adminId, 
                    Name = "Administrator",
                    Department = "Management"
                };
                
                _viewModel.InitializeWithAdmin(admin);
            }
        }
    }
}
