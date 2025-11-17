using Apache.Data;
using Apache.Models;
using Apache.Services;
using Microsoft.Maui.Graphics;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Globalization;

namespace Apache.ViewModels
{
    /// <summary>
    /// ViewModel para o painel de administração
    /// </summary>
    public class AdminViewModel : BaseViewModel
    {
        // Observable collections para produtos, pedidos e clientes (Data Binding)
        private ObservableCollection<Product> _products;
        private ObservableCollection<Order> _orders;
        private ObservableCollection<Customer> _customers;
        private Admin? _currentAdmin; // Pode ser nulo até inicialização
        private Product? _selectedProduct; // Pode ser nulo até seleção
        private Order? _selectedOrder;
        private Customer? _selectedCustomer;
        private string _newProductName;
        private decimal _newProductPrice;
        private int _newProductStock;
        private string _newProductPriceText;
        private string _newProductStockText;
        private string _selectedTab = "Products";
        private OrderStatus _selectedOrderStatus;
        // Serviços de dados e logging
        private readonly DataRepository _repository;
        private readonly LoggingService _logger;

        /// <summary>  
        /// Pega ou define a coleção de produtos
        /// </summary>
        public ObservableCollection<Product> Products
        {
            get => _products;
            set => SetProperty(ref _products, value);
        }

        /// <summary>
        /// Pega ou define a coleção de pedidos
        /// </summary>
        public ObservableCollection<Order> Orders
        {
            get => _orders;
            set => SetProperty(ref _orders, value);
        }

        /// <summary>
        /// Pega ou define a coleção de clientes
        /// </summary>
        public ObservableCollection<Customer> Customers
        {
            get => _customers;
            set => SetProperty(ref _customers, value);
        }

        /// <summary>
        /// Commandos para ações do administrador
        /// </summary>
        public Product? SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                if (SetProperty(ref _selectedProduct, value))
                {
                    // When a product is selected in the UI, populate the edit inputs
                    if (_selectedProduct != null)
                    {
                        NewProductName = _selectedProduct.Name;
                        NewProductPrice = _selectedProduct.Price;
                        NewProductStock = _selectedProduct.Stock;
                        NewProductPriceText = _selectedProduct.Price.ToString(CultureInfo.InvariantCulture);
                        NewProductStockText = _selectedProduct.Stock.ToString(CultureInfo.InvariantCulture);
                    }
                }
            }
        }

        public Order? SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                if (SetProperty(ref _selectedOrder, value))
                {
                    if (_selectedOrder != null)
                    {
                        SelectedOrderStatus = _selectedOrder.Status;
                    }
                }
            }
        }

        public Customer? SelectedCustomer
        {
            get => _selectedCustomer;
            set => SetProperty(ref _selectedCustomer, value);
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

        // String-backed properties to bind to Entry.Text to avoid conversion issues
        public string NewProductPriceText
        {
            get => _newProductPriceText;
            set
            {
                if (SetProperty(ref _newProductPriceText, value))
                {
                    // Try parse using invariant culture; ignore parse failure (keep previous numeric value)
                    if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var dec))
                        NewProductPrice = dec;
                }
            }
        }

        public string NewProductStockText
        {
            get => _newProductStockText;
            set
            {
                if (SetProperty(ref _newProductStockText, value))
                {
                    if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i))
                        NewProductStock = i;
                }
            }
        }

        public string SelectedTab
        {
            get => _selectedTab;
            set => SetProperty(ref _selectedTab, value);
        }

        public OrderStatus SelectedOrderStatus
        {
            get => _selectedOrderStatus;
            set => SetProperty(ref _selectedOrderStatus, value);
        }

        public IEnumerable<OrderStatus> OrderStatusOptions => Enum.GetValues(typeof(OrderStatus)).Cast<OrderStatus>();

        public ICommand AddProductCommand { get; }
        public ICommand UpdateProductCommand { get; }
        public ICommand DeleteProductCommand { get; }
        public ICommand SelectProductCommand { get; }
        public ICommand ViewOrderDetailsCommand { get; }
        public ICommand SetOrderStatusCommand { get; }
        public ICommand DeleteOrderCommand { get; }
        public ICommand EditCustomerCommand { get; }
        public ICommand DeleteCustomerCommand { get; }
        public ICommand CreateCustomerCommand { get; }
        public ICommand ApplyOrderStatusCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand SelectTabCommand { get; }
        public ICommand OpenEditCustomerPageCommand { get; }

        public AdminViewModel()
        {
            _repository = DataRepository.Instance;
            _logger = LoggingService.Instance;

            _products = new ObservableCollection<Product>();
            _orders = new ObservableCollection<Order>();
            _customers = new ObservableCollection<Customer>();
            _currentAdmin = null;
            _selectedProduct = null;
            _selectedOrder = null;
            _selectedCustomer = null;
            _newProductName = string.Empty;
            _newProductPrice = 0;
            _newProductStock = 0;
            _newProductPriceText = "0";
            _newProductStockText = "0";

            Products = _products;
            Orders = _orders;
            Customers = _customers;

            AddProductCommand = new RelayCommand(async () => await ExecuteAddProduct());
            UpdateProductCommand = new RelayCommand(async () => await ExecuteUpdateProduct());
            DeleteProductCommand = new RelayCommand(async () => await ExecuteDeleteProduct());
            SelectProductCommand = new RelayCommand<Product>((p) => ExecuteSelectProduct(p));
            ViewOrderDetailsCommand = new RelayCommand<Order>(async (order) => await ExecuteViewOrderDetails(order));
            SetOrderStatusCommand = new RelayCommand<Order>(async (order) => await ExecuteSetOrderStatus(order));
            DeleteOrderCommand = new RelayCommand<Order>(async (order) => await ExecuteDeleteOrder(order));
            EditCustomerCommand = new RelayCommand<Customer>(async (c) => await ExecuteEditCustomer(c));
            DeleteCustomerCommand = new RelayCommand<Customer>(async (c) => await ExecuteDeleteCustomer(c));
            CreateCustomerCommand = new RelayCommand(async () => await ExecuteCreateCustomer());
            ApplyOrderStatusCommand = new RelayCommand(async () => await ExecuteApplyOrderStatus());
            RefreshCommand = new RelayCommand(async () => await LoadData());
            LogoutCommand = new RelayCommand(async () => await ExecuteLogout());
            SelectTabCommand = new RelayCommand<string>((tab) => ExecuteSelectTab(tab));
            OpenEditCustomerPageCommand = new RelayCommand<Customer>(async (c) => await ExecuteOpenEditCustomerPage(c));

            LoadData();
        }

        /// <summary>
        /// Seleciona a aba especificada no painel de administração.
        /// </summary>
        private void ExecuteSelectTab(string tabName)
        {
            if (!string.IsNullOrWhiteSpace(tabName))
            {
                SelectedTab = tabName;
            }
        }

        /// <summary>
        /// Inicializa a ViewModel com o administrador atual.
        /// </summary>
        public void InitializeWithAdmin(Admin admin)
        {
            _currentAdmin = admin;
            _logger.LogInfo($"Admin view initialized for: {admin.Name}");
        }

        /// <summary>
        /// Processa o carregamento dos dados do painel de administração.
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
        /// Adiciona um novo produto ao inventário.
        /// </summary>
        private async Task ExecuteAddProduct()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(NewProductName) || NewProductPrice <= 0 || NewProductStock < 0)
                {
                    await App.Current.MainPage.DisplayAlertAsync("Error", "Please fill in all fields correctly", "OK");
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
                NewProductPriceText = "0";
                NewProductStockText = "0";

                await App.Current.MainPage.DisplayAlertAsync("Success", "Product added successfully", "OK");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error adding product", ex);
                await App.Current.MainPage.DisplayAlertAsync("Error", ex.Message, "OK");
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

                // Apply values entered in the edit fields back to the selected product before saving
                SelectedProduct.Name = NewProductName;
                SelectedProduct.Price = NewProductPrice;
                SelectedProduct.Stock = NewProductStock;

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

        private void ExecuteSelectProduct(Product p)
        {
            if (p == null) return;
            SelectedProduct = p;
            NewProductName = p.Name;
            NewProductPrice = p.Price;
            NewProductStock = p.Stock;
            NewProductPriceText = p.Price.ToString(CultureInfo.InvariantCulture);
            NewProductStockText = p.Stock.ToString(CultureInfo.InvariantCulture);
        }

        private async Task ExecuteDeleteProduct()
        {
            try
            {
                if (SelectedProduct == null)
                {
                    await App.Current.MainPage.DisplayAlert("Alert", "Please select a product to delete", "OK");
                    return;
                }

                bool ok = await App.Current.MainPage.DisplayAlert("Confirm", "Delete selected product?", "Yes", "No");
                if (!ok) return;

                _repository.DeleteProduct(SelectedProduct.Id);
                Products.Remove(SelectedProduct);
                SelectedProduct = null;
                await App.Current.MainPage.DisplayAlert("Success", "Product deleted", "OK");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error deleting product", ex);
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

                await Application.Current.MainPage.DisplayAlertAsync(
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

        private async Task ExecuteSetOrderStatus(Order order)
        {
            try
            {
                if (order == null) return;
                _repository.UpdateOrderStatus(order.Id, OrderStatus.Shipped);
                await App.Current.MainPage.DisplayAlert("Success", "Order marked as shipped", "OK");
                await LoadData();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error setting order status", ex);
                await App.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async Task ExecuteDeleteOrder(Order order)
        {
            try
            {
                if (order == null) return;
                bool ok = await App.Current.MainPage.DisplayAlert("Confirm", "Delete this order?", "Yes", "No");
                if (!ok) return;
                _repository.DeleteOrder(order.Id);
                Orders.Remove(order);
                await App.Current.MainPage.DisplayAlert("Success", "Order deleted", "OK");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error deleting order", ex);
                await App.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async Task ExecuteEditCustomer(Customer c)
        {
            try
            {
                if (c == null) return;
                SelectedCustomer = c;

                // Prompt for multiple fields with initial values
                string newName = await App.Current.MainPage.DisplayPromptAsync("Edit Customer", "Name:", initialValue: c.Name);
                string newEmail = await App.Current.MainPage.DisplayPromptAsync("Edit Customer", "Email:", initialValue: c.Email);
                string newPassword = await App.Current.MainPage.DisplayPromptAsync("Edit Customer", "Password (leave blank to keep)", initialValue: "", maxLength: -1, keyboard: Keyboard.Text);
                string newAddress = await App.Current.MainPage.DisplayPromptAsync("Edit Customer", "Address:", initialValue: c.Address);
                string newPhone = await App.Current.MainPage.DisplayPromptAsync("Edit Customer", "Phone:", initialValue: c.PhoneNumber);

                string activeChoice = await App.Current.MainPage.DisplayActionSheet("Active status", "Cancel", null, "Active", "Inactive");

                // if user cancelled action sheet, keep existing
                bool? isActiveChoice = null;
                if (activeChoice == "Active") isActiveChoice = true;
                else if (activeChoice == "Inactive") isActiveChoice = false;

                // Apply changes if provided
                if (!string.IsNullOrWhiteSpace(newName)) c.Name = newName;
                if (!string.IsNullOrWhiteSpace(newEmail)) c.Email = newEmail; // will validate in User.Email setter
                if (!string.IsNullOrWhiteSpace(newPassword)) c.Password = newPassword; // will validate in setter
                c.Address = newAddress; // allow clearing
                c.PhoneNumber = newPhone; // allow clearing
                if (isActiveChoice.HasValue) c.IsActive = isActiveChoice.Value;

                _repository.UpdateCustomer(c);
                await App.Current.MainPage.DisplayAlert("Success", "Customer updated", "OK");
                await LoadData();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error editing customer", ex);
                await App.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async Task ExecuteDeleteCustomer(Customer c)
        {
            try
            {
                if (c == null) return;
                bool ok = await App.Current.MainPage.DisplayAlert("Confirm", "Delete this customer?", "Yes", "No");
                if (!ok) return;
                _repository.DeleteCustomer(c.Id);
                Customers.Remove(c);
                await App.Current.MainPage.DisplayAlert("Success", "Customer deleted", "OK");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error deleting customer", ex);
                await App.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async Task ExecuteCreateCustomer()
        {
            try
            {
                var email = await App.Current.MainPage.DisplayPromptAsync("Create Customer", "Email:");
                var name = await App.Current.MainPage.DisplayPromptAsync("Create Customer", "Name:");
                var password = await App.Current.MainPage.DisplayPromptAsync("Create Customer", "Password:");
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                {
                    await App.Current.MainPage.DisplayAlert("Error", "Email and password are required", "OK");
                    return;
                }
                var customer = new Customer { Email = email, Name = name, Password = password };
                _repository.AddCustomer(customer);
                Customers.Add(customer);
                await App.Current.MainPage.DisplayAlert("Success", "Customer created", "OK");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error creating customer", ex);
                await App.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async Task ExecuteApplyOrderStatus()
        {
            try
            {
                if (SelectedOrder == null)
                {
                    await App.Current.MainPage.DisplayAlert("Alert", "Select an order", "OK");
                    return;
                }
                _repository.UpdateOrderStatus(SelectedOrder.Id, SelectedOrderStatus);
                await App.Current.MainPage.DisplayAlert("Success", "Order status updated", "OK");
                await LoadData();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error applying order status", ex);
                await App.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
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

        private async Task ExecuteOpenEditCustomerPage(Customer c)
        {
            try
            {
                if (c == null) return;

                // Create entries and controls programmatically
                var nameEntry = new Entry { Text = c.Name };
                var emailEntry = new Entry { Text = c.Email, Keyboard = Keyboard.Email };
                var passwordEntry = new Entry { IsPassword = true, Placeholder = "Leave blank to keep current" };
                var addressEntry = new Entry { Text = c.Address };
                var phoneEntry = new Entry { Text = c.PhoneNumber, Keyboard = Keyboard.Telephone };
                var activeSwitch = new Switch { IsToggled = c.IsActive };

                var saveButton = new Button { Text = "Save", BackgroundColor = Color.FromArgb("#4CAF50"), TextColor = Colors.White };
                var cancelButton = new Button { Text = "Cancel", BackgroundColor = Color.FromArgb("#E0E0E0") };

                var page = new ContentPage
                {
                    Title = "Edit Customer",
                    Content = new ScrollView
                    {
                        Content = new VerticalStackLayout
                        {
                            Padding = 12,
                            Spacing = 12,
                            Children =
                            {
                                new Label { Text = "Edit Customer", FontSize = 20, FontAttributes = FontAttributes.Bold },
                                new Label { Text = "Name" }, nameEntry,
                                new Label { Text = "Email" }, emailEntry,
                                new Label { Text = "Password (leave blank to keep current)" }, passwordEntry,
                                new Label { Text = "Address" }, addressEntry,
                                new Label { Text = "Phone" }, phoneEntry,
                                new HorizontalStackLayout { Spacing = 10, Children = { new Label { Text = "Active", VerticalOptions = LayoutOptions.Center }, activeSwitch } },
                                new HorizontalStackLayout { Spacing = 12, Children = { saveButton, cancelButton } }
                            }
                        }
                    }
                };

                saveButton.Clicked += async (s, e) =>
                {
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(nameEntry.Text)) c.Name = nameEntry.Text;
                        if (!string.IsNullOrWhiteSpace(emailEntry.Text)) c.Email = emailEntry.Text;
                        if (!string.IsNullOrWhiteSpace(passwordEntry.Text)) c.Password = passwordEntry.Text;
                        c.Address = addressEntry.Text;
                        c.PhoneNumber = phoneEntry.Text;
                        c.IsActive = activeSwitch.IsToggled;

                        _repository.UpdateCustomer(c);
                        await Application.Current.MainPage.DisplayAlert("Success", "Customer updated", "OK");
                        await Shell.Current.Navigation.PopAsync();
                        await LoadData();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Error saving customer from page", ex);
                        await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
                    }
                };

                cancelButton.Clicked += async (s, e) =>
                {
                    await Shell.Current.Navigation.PopAsync();
                };

                await Shell.Current.Navigation.PushAsync(page);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error opening edit customer page", ex);
            }
        }
    }
}
