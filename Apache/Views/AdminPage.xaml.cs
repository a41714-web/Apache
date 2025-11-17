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

    private void ProductsCollection_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection != null && e.CurrentSelection.Count > 0)
        {
            var product = e.CurrentSelection[0] as Apache.Models.Product;
            _viewModel.SelectedProduct = product;
        }
    }

    private void OrdersCollection_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection != null && e.CurrentSelection.Count > 0)
        {
            var order = e.CurrentSelection[0] as Apache.Models.Order;
            _viewModel.SelectedOrder = order;
        }
    }

    private void CustomersCollection_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection != null && e.CurrentSelection.Count > 0)
        {
            var customer = e.CurrentSelection[0] as Apache.Models.Customer;
            _viewModel.SelectedCustomer = customer;
        }
    }
}
