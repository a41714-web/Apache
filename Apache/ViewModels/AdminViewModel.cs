using Apache.Data;
using Apache.Models;
using Apache.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Apache.ViewModels
{
    /// <summary>
    /// ViewModel for admin dashboard and product management.
    /// </summary>
    public class AdminViewModel : BaseViewModel
    {
        private ObservableCollection<Product> _products;
        private ObservableCollection<Order> _orders;
        private ObservableCollection<Customer> _customers;
        private Admin _currentAdmin;
        private Product _selectedProduct;
        private string _newProductName;
        private decimal _newProductPrice;
        private int _newProductStock;
        private string _selectedTab = "Products";
        private readonly DataRepository _repository;
        private readonly LoggingService _logger;

        public ObservableCollection<Product> Products
        {
            get => _products;
            set => SetProperty(ref _products, value);
        }

        public ObservableCollection<Order> Orders
        {
            get => _orders;
            set => SetProperty(ref _orders, value);
        }

        public ObservableCollection<Customer> Customers
        {
            get => _customers;
            set => SetProperty(ref _customers, value);
        }

        public Product SelectedProduct
        {
            get => _selectedProduct;
            set => SetProperty(ref _selectedProduct, value);
        }

        public string NewProductName
        {
            get => _newProductName;
            set => SetProperty(ref _newProductName, value);
        }

        public decimal NewProductPrice
        {
            get => _newProductPrice;
            set => SetProperty(ref _newProductPrice, value);
        }

        public int NewProductStock
        {
            get => _newProductStock;
            set => SetProperty(ref _newProductStock, value);
        }

        public string SelectedTab
        {
            get => _selectedTab;
            set => SetProperty(ref _selectedTab, value);
        }

        public ICommand AddProductCommand { get; }
        public ICommand UpdateProductCommand { get; }
        public ICommand ViewOrderDetailsCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand SelectTabCommand { get; }

        public AdminViewModel()
        {
            _repository = DataRepository.Instance;
            _logger = LoggingService.Instance;
            
            Products = new ObservableCollection<Product>();
            Orders = new ObservableCollection<Order>();
            Customers = new ObservableCollection<Customer>();

            AddProductCommand = new RelayCommand(async () => await ExecuteAddProduct());
            UpdateProductCommand = new RelayCommand(async () => await ExecuteUpdateProduct());
            ViewOrderDetailsCommand = new RelayCommand<Order>(async (order) => await ExecuteViewOrderDetails(order));
            RefreshCommand = new RelayCommand(async () => await LoadData());
            LogoutCommand = new RelayCommand(async () => await ExecuteLogout());
            SelectTabCommand = new RelayCommand<string>((tab) => ExecuteSelectTab(tab));

            LoadData();
        }

        /// <summary>
        /// Selects the active tab.
        /// </summary>
        private void ExecuteSelectTab(string tabName)
        {
            if (!string.IsNullOrWhiteSpace(tabName))
            {
                SelectedTab = tabName;
            }
        }

        /// <summary>
        /// Initializes the view model with the logged-in admin.
        /// </summary>
        public void InitializeWithAdmin(Admin admin)
        {
            _currentAdmin = admin;
            _logger.LogInfo($"Admin view initialized for: {admin.Name}");
        }

        /// <summary>
        /// Loads all products, orders, and customers.
        /// </summary>
        private async Task LoadData()
        {
            try
            {
                IsLoading = true;
                _logger.LogDebug("Loading admin dashboard data");

                await Task.Delay(300); // Simulate network delay

                // Load products
                Products.Clear();
                var products = _repository.GetAllProducts();
                foreach (var product in products)
                {
                    Products.Add(product);
                }

                // Load orders
                Orders.Clear();
                var orders = _repository.GetAllOrders();
                foreach (var order in orders)
                {
                    Orders.Add(order);
                }

                // Load customers
                Customers.Clear();
                var customers = _repository.GetAllCustomers();
                foreach (var customer in customers)
                {
                    Customers.Add(customer);
                }

                _logger.LogInfo($"Dashboard loaded: {products.Count} products, {orders.Count} orders, {customers.Count} customers");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error loading dashboard data", ex);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Adds a new product to the catalog.
        /// </summary>
        private async Task ExecuteAddProduct()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(NewProductName) || NewProductPrice <= 0 || NewProductStock < 0)
                {
                    await App.Current.MainPage.DisplayAlert("Error", "Please fill in all fields correctly", "OK");
                    return;
                }

                var product = new Product
                {
                    Name = NewProductName,
                    Price = NewProductPrice,
                    Stock = NewProductStock,
                    Category = "General",
                    Description = "New product"
                };

                _repository.AddProduct(product);
                _logger.LogInfo($"New product added: {product.Name}");

                Products.Add(product);

                NewProductName = string.Empty;
                NewProductPrice = 0;
                NewProductStock = 0;

                await App.Current.MainPage.DisplayAlert("Success", "Product added successfully", "OK");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error adding product", ex);
                await App.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        /// <summary>
        /// Updates the selected product's information.
        /// </summary>
        private async Task ExecuteUpdateProduct()
        {
            try
            {
                if (SelectedProduct == null)
                {
                    await App.Current.MainPage.DisplayAlert("Alert", "Please select a product", "OK");
                    return;
                }

                _repository.UpdateProduct(SelectedProduct);
                _logger.LogInfo($"Product updated: {SelectedProduct.Name}");

                await App.Current.MainPage.DisplayAlert("Success", "Product updated successfully", "OK");
                await LoadData();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error updating product", ex);
                await App.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        /// <summary>
        /// Displays order details.
        /// </summary>
        private async Task ExecuteViewOrderDetails(Order order)
        {
            try
            {
                if (order == null)
                    return;

                var itemsInfo = string.Join("\n", order.Items.Select(item => 
                    $"{item.ProductName} x{item.Quantity} = ${item.GetLineTotal():F2}"));

                await App.Current.MainPage.DisplayAlert(
                    $"Order #{order.Id}",
                    $"Customer: {order.CustomerId}\nStatus: {order.Status}\n\nItems:\n{itemsInfo}\n\nTotal: ${order.GetTotal():F2}",
                    "OK"
                );

                _logger.LogDebug($"Viewed order details: Order #{order.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error viewing order details", ex);
            }
        }

        /// <summary>
        /// Logs out the admin and returns to login screen.
        /// </summary>
        private async Task ExecuteLogout()
        {
            try
            {
                _logger.LogInfo($"Admin logged out: {_currentAdmin?.Name}");
                await Shell.Current.GoToAsync("//login");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error during logout", ex);
            }
        }
    }
}
