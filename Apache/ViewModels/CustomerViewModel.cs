using Apache.Data;
using Apache.Models;
using Apache.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Apache.ViewModels
{
    /// <summary>
    /// ViewModel for customer browsing products and placing orders.
    /// </summary>
    public class CustomerViewModel : BaseViewModel
    {
        private ObservableCollection<Product> _products;
        private ObservableCollection<Order> _customerOrders;
        private Customer _currentCustomer;
        private Order _currentOrder;
        private Product _selectedProduct;
        private int _selectedQuantity = 1;
        private readonly DataRepository _repository;
        private readonly LoggingService _logger;

        public ObservableCollection<Product> Products
        {
            get => _products;
            set => SetProperty(ref _products, value);
        }

        public ObservableCollection<Order> CustomerOrders
        {
            get => _customerOrders;
            set => SetProperty(ref _customerOrders, value);
        }

        public Product SelectedProduct
        {
            get => _selectedProduct;
            set => SetProperty(ref _selectedProduct, value);
        }

        public int SelectedQuantity
        {
            get => _selectedQuantity;
            set => SetProperty(ref _selectedQuantity, Math.Max(1, value));
        }

        public decimal CartTotal => _currentOrder?.GetTotal() ?? 0;
        public int CartItemCount => _currentOrder?.GetItemCount() ?? 0;

        public ICommand AddToCartCommand { get; }
        public ICommand PlaceOrderCommand { get; }
        public ICommand RefreshProductsCommand { get; }
        public ICommand LogoutCommand { get; }

        public CustomerViewModel()
        {
            _repository = DataRepository.Instance;
            _logger = LoggingService.Instance;
            
            Products = new ObservableCollection<Product>();
            CustomerOrders = new ObservableCollection<Order>();
            _currentOrder = new Order();

            AddToCartCommand = new RelayCommand(async () => await ExecuteAddToCart());
            PlaceOrderCommand = new RelayCommand(async () => await ExecutePlaceOrder());
            RefreshProductsCommand = new RelayCommand(async () => await LoadProducts());
            LogoutCommand = new RelayCommand(async () => await ExecuteLogout());

            LoadProducts();
        }

        /// <summary>
        /// Initializes the view model with the logged-in customer.
        /// </summary>
        public void InitializeWithCustomer(Customer customer)
        {
            _currentCustomer = customer;
            _currentOrder = new Order { CustomerId = customer.Id };
            _logger.LogInfo($"Customer view initialized for: {customer.Name}");
            LoadOrders();
        }

        /// <summary>
        /// Loads all available products from the repository.
        /// </summary>
        private async Task LoadProducts()
        {
            try
            {
                IsLoading = true;
                _logger.LogDebug("Loading products");

                await Task.Delay(300); // Simulate network delay

                Products.Clear();
                var products = _repository.GetAllProducts();
                foreach (var product in products)
                {
                    Products.Add(product);
                }

                _logger.LogInfo($"Loaded {products.Count} products");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error loading products", ex);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Loads customer's previous orders.
        /// </summary>
        private void LoadOrders()
        {
            try
            {
                if (_currentCustomer == null)
                    return;

                CustomerOrders.Clear();
                var orders = _repository.GetOrdersByCustomerId(_currentCustomer.Id);
                foreach (var order in orders)
                {
                    CustomerOrders.Add(order);
                }

                _logger.LogDebug($"Loaded {orders.Count} orders for customer {_currentCustomer.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error loading customer orders", ex);
            }
        }

        /// <summary>
        /// Adds selected product to the shopping cart.
        /// </summary>
        private async Task ExecuteAddToCart()
        {
            try
            {
                if (SelectedProduct == null)
                {
                    await App.Current.MainPage.DisplayAlert("Alert", "Please select a product", "OK");
                    return;
                }

                _currentOrder.AddItem(SelectedProduct, SelectedQuantity);
                _logger.LogInfo($"Added to cart: {SelectedProduct.Name} x{SelectedQuantity}");

                OnPropertyChanged(nameof(CartTotal));
                OnPropertyChanged(nameof(CartItemCount));

                await App.Current.MainPage.DisplayAlert("Success", "Product added to cart", "OK");
                SelectedQuantity = 1;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error adding to cart", ex);
                await App.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        /// <summary>
        /// Places the order with items in the shopping cart.
        /// </summary>
        private async Task ExecutePlaceOrder()
        {
            try
            {
                if (_currentCustomer == null)
                {
                    await App.Current.MainPage.DisplayAlert("Error", "Customer context not initialized. Please log in again.", "OK");
                    _logger.LogError("ExecutePlaceOrder called with null _currentCustomer");
                    return;
                }

                if (_currentOrder.Items.Count == 0)
                {
                    await App.Current.MainPage.DisplayAlert("Alert", "Cart is empty", "OK");
                    return;
                }

                IsLoading = true;

                // Reduce stock for each item
                foreach (var item in _currentOrder.Items)
                {
                    var product = _repository.GetProductById(item.ProductId);
                    if (product != null)
                    {
                        product.ReduceStock(item.Quantity);
                    }
                }

                _repository.AddOrder(_currentOrder);
                _logger.LogInfo($"Order placed successfully: Order ID {_currentOrder.Id}");

                await App.Current.MainPage.DisplayAlert(
                    "Success",
                    $"Order placed! Order ID: {_currentOrder.Id}\nTotal: ${_currentOrder.GetTotal():F2}",
                    "OK"
                );

                LoadOrders();
                _currentOrder = new Order { CustomerId = _currentCustomer.Id };
                OnPropertyChanged(nameof(CartTotal));
                OnPropertyChanged(nameof(CartItemCount));
            }
            catch (Exception ex)
            {
                _logger.LogError("Error placing order", ex);
                await App.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Logs out the customer and returns to login screen.
        /// </summary>
        private async Task ExecuteLogout()
        {
            try
            {
                _logger.LogInfo($"Customer logged out: {_currentCustomer?.Name}");
                await Shell.Current.GoToAsync("//login");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error during logout", ex);
            }
        }
    }
}
