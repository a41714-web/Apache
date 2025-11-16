using Apache.Data;
using Apache.ViewModels;

namespace Apache.Views;

public partial class CustomerPage : ContentPage
{
    private readonly CustomerViewModel _viewModel;

    public CustomerPage()
    {
        InitializeComponent();
        _viewModel = new CustomerViewModel();
        BindingContext = _viewModel;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        
        // Get customer ID from query parameters
        if (Shell.Current.CurrentState.Location.OriginalString.Contains("id="))
        {
            var idString = Shell.Current.CurrentState.Location.OriginalString.Split("id=")[1];
            if (int.TryParse(idString, out int customerId))
            {
                var customer = DataRepository.Instance.GetCustomerById(customerId);
                if (customer != null)
                {
                    _viewModel.InitializeWithCustomer(customer);
                }
            }
        }
    }
}
