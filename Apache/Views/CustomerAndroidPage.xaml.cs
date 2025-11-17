using Apache.ViewModels;
using Microsoft.Maui.Controls;

namespace Apache.Views;

[QueryProperty(nameof(CustomerId), "id")]
public partial class CustomerAndroidPage : ContentPage
{
    private readonly CustomerViewModel _viewModel;
    private int _customerId;

    public int CustomerId
    {
        get => _customerId;
        set
        {
            _customerId = value;
            LoadCustomerData();
        }
    }

    public CustomerAndroidPage()
    {
        InitializeComponent();
        _viewModel = new CustomerViewModel();
        BindingContext = _viewModel;
    }

    private void LoadCustomerData()
    {
        try
        {
            if (_customerId > 0)
            {
                var customer = Apache.Data.DataRepository.Instance.GetCustomerById(_customerId);
                if (customer != null)
                {
                    _viewModel.InitializeWithCustomer(customer);
                }
                else
                {
                    DisplayAlert("Error", "Customer not found", "OK");
                    Shell.Current.GoToAsync("//login");
                }
            }
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", $"Failed to load customer: {ex.Message}", "OK");
            Shell.Current.GoToAsync("//login");
        }
    }

    private void ProductsCollection_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection != null && e.CurrentSelection.Count > 0)
        {
            var product = e.CurrentSelection[0] as Apache.Models.Product;
            _viewModel.SelectedProduct = product;
        }
        else
        {
            _viewModel.SelectedProduct = null;
        }
    }

    private void OnFrameTapped(object sender, EventArgs e)
    {
        if (sender is Frame frame && frame.BindingContext is Apache.Models.Product product)
        {
            // manually set selection on the CollectionView
            ProductsCollection.SelectedItem = product;
            _viewModel.SelectedProduct = product;
        }
    }
}
