using Apache.Models;
using Apache.Services;
using MySql.Data.MySqlClient;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace Apache.Data
{
    /// <summary>
    /// Data repository for products, users, and orders using MySQL database.
    /// Implements repository pattern with database access.
    /// </summary>
    public class DataRepository
    {
        private static DataRepository? _instance;
        private static readonly object _instanceLock = new object();
        private readonly LoggingService _logger;
        private readonly string _connectionString;

        public static DataRepository Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_instanceLock)
                    {
                        if (_instance == null)
                        {
                            _instance = new DataRepository();
                        }
                    }
                }
                return _instance;
            }
        }

        private DataRepository()
        {
            _logger = LoggingService.Instance;
            _connectionString = DatabaseConfig.CreateFromConfiguration().ConnectionString;
        }

        // Métodos para Productos
        public IReadOnlyList<Product> GetAllProducts()
        {
            var products = new List<Product>();
            try
            {
                _logger.LogDebug("Fetching all products from database");
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT Id, Name, Description, Price, Stock, Category, ImageUrl FROM Products;";
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                products.Add(new Product
                                {
                                    Id = (int)reader["Id"],
                                    Name = (string)reader["Name"],
                                    Description = reader["Description"] != DBNull.Value ? (string)reader["Description"] : "",
                                    Price = (decimal)reader["Price"],
                                    Stock = (int)reader["Stock"],
                                    Category = reader["Category"] != DBNull.Value ? (string)reader["Category"] : "",
                                    ImageUrl = reader["ImageUrl"] != DBNull.Value ? (string)reader["ImageUrl"] : ""
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error fetching products", ex);
                throw;
            }
            return products.AsReadOnly();
        }

        public Product GetProductById(int id)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT Id, Name, Description, Price, Stock, Category, ImageUrl FROM Products WHERE Id = @Id;";
                        command.Parameters.AddWithValue("@Id", id);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new Product
                                {
                                    Id = (int)reader["Id"],
                                    Name = (string)reader["Name"],
                                    Description = reader["Description"] != DBNull.Value ? (string)reader["Description"] : "",
                                    Price = (decimal)reader["Price"],
                                    Stock = (int)reader["Stock"],
                                    Category = reader["Category"] != DBNull.Value ? (string)reader["Category"] : "",
                                    ImageUrl = reader["ImageUrl"] != DBNull.Value ? (string)reader["ImageUrl"] : ""
                                };
                            }
                        }
                    }
                }
                _logger.LogWarning($"Product with ID {id} not found");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching product {id}", ex);
                throw;
            }
        }

        public void AddProduct(Product product)
        {
            try
            {
                if (product == null)
                    throw new ArgumentNullException(nameof(product));

                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                            INSERT INTO Products (Name, Description, Price, Stock, Category, ImageUrl)
                            VALUES (@Name, @Description, @Price, @Stock, @Category, @ImageUrl);";
                        command.Parameters.AddWithValue("@Name", product.Name);
                        command.Parameters.AddWithValue("@Description", product.Description ?? "");
                        command.Parameters.AddWithValue("@Price", product.Price);
                        command.Parameters.AddWithValue("@Stock", product.Stock);
                        command.Parameters.AddWithValue("@Category", product.Category ?? "");
                        command.Parameters.AddWithValue("@ImageUrl", product.ImageUrl ?? "");
                        command.ExecuteNonQuery();
                    }
                }
                _logger.LogInfo($"Product added: {product.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error adding product", ex);
                throw;
            }
        }

        public void UpdateProduct(Product product)
        {
            try
            {
                if (product == null)
                    throw new ArgumentNullException(nameof(product));

                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                            UPDATE Products 
                            SET Name = @Name, Description = @Description, Price = @Price, Stock = @Stock, Category = @Category
                            WHERE Id = @Id;";
                        command.Parameters.AddWithValue("@Id", product.Id);
                        command.Parameters.AddWithValue("@Name", product.Name);
                        command.Parameters.AddWithValue("@Description", product.Description ?? "");
                        command.Parameters.AddWithValue("@Price", product.Price);
                        command.Parameters.AddWithValue("@Stock", product.Stock);
                        command.Parameters.AddWithValue("@Category", product.Category ?? "");
                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected == 0)
                            throw new InvalidOperationException($"Product with ID {product.Id} not found");
                    }
                }
                _logger.LogInfo($"Product updated: {product.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error updating product", ex);
                throw;
            }
        }

        public void DeleteProduct(int productId)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "DELETE FROM Products WHERE Id = @Id;";
                        command.Parameters.AddWithValue("@Id", productId);
                        int rows = command.ExecuteNonQuery();
                        if (rows == 0)
                            throw new InvalidOperationException($"Product {productId} not found");
                    }
                }
                _logger.LogInfo($"Product {productId} deleted");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting product {productId}", ex);
                throw;
            }
        }

        // Métodos para Clientes
        public IReadOnlyList<Customer> GetAllCustomers()
        {
            var customers = new List<Customer>();
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT Id, Name, Email, Password, Address, PhoneNumber, CreatedAt, IsActive FROM Customers;";
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                customers.Add(new Customer
                                {
                                    Id = (int)reader["Id"],
                                    Name = (string)reader["Name"],
                                    Email = (string)reader["Email"],
                                    Password = (string)reader["Password"],
                                    Address = reader["Address"] != DBNull.Value ? (string)reader["Address"] : "",
                                    PhoneNumber = reader["PhoneNumber"] != DBNull.Value ? (string)reader["PhoneNumber"] : "",
                                    CreatedAt = (DateTime)reader["CreatedAt"],
                                    IsActive = (bool)reader["IsActive"]
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error fetching customers", ex);
                throw;
            }
            return customers.AsReadOnly();
        }

        public Customer GetCustomerById(int id)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT Id, Name, Email, Password, Address, PhoneNumber, CreatedAt, IsActive FROM Customers WHERE Id = @Id;";
                        command.Parameters.AddWithValue("@Id", id);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new Customer
                                {
                                    Id = (int)reader["Id"],
                                    Name = (string)reader["Name"],
                                    Email = (string)reader["Email"],
                                    Password = (string)reader["Password"],
                                    Address = reader["Address"] != DBNull.Value ? (string)reader["Address"] : "",
                                    PhoneNumber = reader["PhoneNumber"] != DBNull.Value ? (string)reader["PhoneNumber"] : "",
                                    CreatedAt = (DateTime)reader["CreatedAt"],
                                    IsActive = (bool)reader["IsActive"]
                                };
                            }
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching customer {id}", ex);
                throw;
            }
        }

        public void AddCustomer(Customer customer)
        {
            try
            {
                if (customer == null)
                    throw new ArgumentNullException(nameof(customer));

                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    // Check if email already exists
                    using (var checkCommand = connection.CreateCommand())
                    {
                        checkCommand.CommandText = "SELECT COUNT(*) FROM Customers WHERE Email = @Email;";
                        checkCommand.Parameters.AddWithValue("@Email", customer.Email);
                        int count = (int)(long)checkCommand.ExecuteScalar();
                        if (count > 0)
                            throw new InvalidOperationException("Email already exists");
                    }

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                            INSERT INTO Customers (Name, Email, Password, Address, PhoneNumber, IsActive)
                            VALUES (@Name, @Email, @Password, @Address, @PhoneNumber, @IsActive);";
                        command.Parameters.AddWithValue("@Name", customer.Name);
                        command.Parameters.AddWithValue("@Email", customer.Email);
                        command.Parameters.AddWithValue("@Password", customer.Password);
                        command.Parameters.AddWithValue("@Address", customer.Address ?? "");
                        command.Parameters.AddWithValue("@PhoneNumber", customer.PhoneNumber ?? "");
                        command.Parameters.AddWithValue("@IsActive", customer.IsActive);
                        command.ExecuteNonQuery();
                    }
                }
                _logger.LogInfo($"Customer registered: {customer.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error adding customer", ex);
                throw;
            }
        }

        public void UpdateCustomer(Customer customer)
        {
            try
            {
                if (customer == null)
                    throw new ArgumentNullException(nameof(customer));

                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                            UPDATE Customers
                            SET Name = @Name, Email = @Email, Password = @Password, Address = @Address, PhoneNumber = @PhoneNumber, IsActive = @IsActive
                            WHERE Id = @Id;";
                        command.Parameters.AddWithValue("@Id", customer.Id);
                        command.Parameters.AddWithValue("@Name", customer.Name ?? "");
                        command.Parameters.AddWithValue("@Email", customer.Email);
                        command.Parameters.AddWithValue("@Password", customer.Password);
                        command.Parameters.AddWithValue("@Address", customer.Address ?? "");
                        command.Parameters.AddWithValue("@PhoneNumber", customer.PhoneNumber ?? "");
                        command.Parameters.AddWithValue("@IsActive", customer.IsActive);

                        int rows = command.ExecuteNonQuery();
                        if (rows == 0)
                            throw new InvalidOperationException($"Customer {customer.Id} not found");
                    }
                }
                _logger.LogInfo($"Customer updated: {customer.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error updating customer", ex);
                throw;
            }
        }

        public void DeleteCustomer(int customerId)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // delete order items and orders belonging to customer
                            using (var cmd = connection.CreateCommand())
                            {
                                cmd.Transaction = transaction;
                                cmd.CommandText = "SELECT Id FROM Orders WHERE CustomerId = @CustomerId;";
                                cmd.Parameters.AddWithValue("@CustomerId", customerId);
                                var orderIds = new List<int>();
                                using (var reader = cmd.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        orderIds.Add((int)reader["Id"]);
                                    }
                                }

                                foreach (var oid in orderIds)
                                {
                                    using (var delItems = connection.CreateCommand())
                                    {
                                        delItems.Transaction = transaction;
                                        delItems.CommandText = "DELETE FROM OrderItems WHERE OrderId = @OrderId;";
                                        delItems.Parameters.AddWithValue("@OrderId", oid);
                                        delItems.ExecuteNonQuery();
                                    }
                                }

                                using (var delOrders = connection.CreateCommand())
                                {
                                    delOrders.Transaction = transaction;
                                    delOrders.CommandText = "DELETE FROM Orders WHERE CustomerId = @CustomerId;";
                                    delOrders.Parameters.AddWithValue("@CustomerId", customerId);
                                    delOrders.ExecuteNonQuery();
                                }
                            }

                            using (var cmdDel = connection.CreateCommand())
                            {
                                cmdDel.Transaction = transaction;
                                cmdDel.CommandText = "DELETE FROM Customers WHERE Id = @Id;";
                                cmdDel.Parameters.AddWithValue("@Id", customerId);
                                int rows = cmdDel.ExecuteNonQuery();
                                if (rows == 0)
                                    throw new InvalidOperationException($"Customer {customerId} not found");
                            }

                            transaction.Commit();
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
                _logger.LogInfo($"Customer {customerId} deleted");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting customer {customerId}", ex);
                throw;
            }
        }

        // Métodos para Ordens
        public IReadOnlyList<Order> GetAllOrders()
        {
            var orders = new List<Order>();
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT Id, CustomerId, OrderDate, Status FROM Orders ORDER BY OrderDate DESC;";
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int orderId = (int)reader["Id"];
                                var order = new Order
                                {
                                    Id = orderId,
                                    CustomerId = (int)reader["CustomerId"],
                                    OrderDate = (DateTime)reader["OrderDate"],
                                    Status = Enum.Parse<OrderStatus>((string)reader["Status"])
                                };
                                // Fetch items using a separate connection to avoid nested readers
                                order.Items.AddRange(GetOrderItems(orderId));
                                orders.Add(order);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error fetching orders", ex);
                throw;
            }
            return orders.AsReadOnly();
        }

        public IReadOnlyList<Order> GetOrdersByCustomerId(int customerId)
        {
            var orders = new List<Order>();
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT Id, CustomerId, OrderDate, Status FROM Orders WHERE CustomerId = @CustomerId ORDER BY OrderDate DESC;";
                        command.Parameters.AddWithValue("@CustomerId", customerId);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int orderId = (int)reader["Id"];
                                var order = new Order
                                {
                                    Id = orderId,
                                    CustomerId = (int)reader["CustomerId"],
                                    OrderDate = (DateTime)reader["OrderDate"],
                                    Status = Enum.Parse<OrderStatus>((string)reader["Status"])
                                };
                                // Fetch items using a separate connection to avoid nested readers
                                order.Items.AddRange(GetOrderItems(orderId));
                                orders.Add(order);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error fetching customer orders", ex);
                throw;
            }
            return orders.AsReadOnly();
        }

        public void AddOrder(Order order)
        {
            try
            {
                if (order == null)
                    throw new ArgumentNullException(nameof(order));

                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            int orderId;
                            // Insert order
                            using (var command = connection.CreateCommand())
                            {
                                command.Transaction = transaction;
                                command.CommandText = @"
                                    INSERT INTO Orders (CustomerId, OrderDate, Status)
                                    VALUES (@CustomerId, @OrderDate, @Status);
                                    SELECT LAST_INSERT_ID();";
                                command.Parameters.AddWithValue("@CustomerId", order.CustomerId);
                                command.Parameters.AddWithValue("@OrderDate", order.OrderDate);
                                command.Parameters.AddWithValue("@Status", order.Status.ToString());
                                orderId = Convert.ToInt32(command.ExecuteScalar());
                            }

                            // Insert order items and update product stock atomically
                            foreach (var item in order.Items)
                            {
                                // decrement product stock ensuring there is enough stock
                                using (var stockCmd = connection.CreateCommand())
                                {
                                    stockCmd.Transaction = transaction;
                                    stockCmd.CommandText = @"
                                        UPDATE Products
                                        SET Stock = Stock - @Quantity
                                        WHERE Id = @ProductId AND Stock >= @Quantity;";
                                    stockCmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                                    stockCmd.Parameters.AddWithValue("@ProductId", item.ProductId);
                                    int rows = stockCmd.ExecuteNonQuery();
                                    if (rows == 0)
                                    {
                                        throw new InvalidOperationException($"Insufficient stock for product {item.ProductId}");
                                    }
                                }

                                using (var command = connection.CreateCommand())
                                {
                                    command.Transaction = transaction;
                                    command.CommandText = @"
                                        INSERT INTO OrderItems (OrderId, ProductId, ProductName, UnitPrice, Quantity)
                                        VALUES (@OrderId, @ProductId, @ProductName, @UnitPrice, @Quantity);";
                                    command.Parameters.AddWithValue("@OrderId", orderId);
                                    command.Parameters.AddWithValue("@ProductId", item.ProductId);
                                    command.Parameters.AddWithValue("@ProductName", item.ProductName);
                                    command.Parameters.AddWithValue("@UnitPrice", item.UnitPrice);
                                    command.Parameters.AddWithValue("@Quantity", item.Quantity);
                                    command.ExecuteNonQuery();
                                }
                            }

                            transaction.Commit();

                            // set the generated id back on the order instance so callers see it
                            order.Id = orderId;

                            _logger.LogInfo($"Order created: {orderId} for customer {order.CustomerId}");
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            _logger.LogError("Error adding order inside transaction", ex);
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error adding order", ex);
                throw;
            }
        }

        public void UpdateOrderStatus(int orderId, OrderStatus status)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "UPDATE Orders SET Status = @Status WHERE Id = @Id;";
                        command.Parameters.AddWithValue("@Id", orderId);
                        command.Parameters.AddWithValue("@Status", status.ToString());
                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected == 0)
                            throw new InvalidOperationException($"Order {orderId} not found");
                    }
                }
                _logger.LogInfo($"Order {orderId} status updated to {status}");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error updating order status", ex);
                throw;
            }
        }

        public void DeleteOrder(int orderId)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            using (var cmd = connection.CreateCommand())
                            {
                                cmd.Transaction = transaction;
                                cmd.CommandText = "DELETE FROM OrderItems WHERE OrderId = @OrderId;";
                                cmd.Parameters.AddWithValue("@OrderId", orderId);
                                cmd.ExecuteNonQuery();
                            }

                            using (var cmd = connection.CreateCommand())
                            {
                                cmd.Transaction = transaction;
                                cmd.CommandText = "DELETE FROM Orders WHERE Id = @Id;";
                                cmd.Parameters.AddWithValue("@Id", orderId);
                                int rows = cmd.ExecuteNonQuery();
                                if (rows == 0)
                                    throw new InvalidOperationException($"Order {orderId} not found");
                            }

                            transaction.Commit();
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
                _logger.LogInfo($"Order {orderId} deleted");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting order {orderId}", ex);
                throw;
            }
        }

        // Authentication
        public Customer AuthenticateCustomer(string email, string password)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT Id, Name, Email, Password, Address, PhoneNumber, CreatedAt, IsActive FROM Customers WHERE Email = @Email AND Password = @Password;";
                        command.Parameters.AddWithValue("@Email", email);
                        command.Parameters.AddWithValue("@Password", password);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var customer = new Customer
                                {
                                    Id = (int)reader["Id"],
                                    Name = (string)reader["Name"],
                                    Email = (string)reader["Email"],
                                    Password = (string)reader["Password"],
                                    Address = reader["Address"] != DBNull.Value ? (string)reader["Address"] : "",
                                    PhoneNumber = reader["PhoneNumber"] != DBNull.Value ? (string)reader["PhoneNumber"] : "",
                                    CreatedAt = (DateTime)reader["CreatedAt"],
                                    IsActive = (bool)reader["IsActive"]
                                };
                                _logger.LogInfo($"Customer authenticated: {email}");
                                return customer;
                            }
                        }
                    }
                }
                _logger.LogWarning($"Failed authentication attempt for email: {email}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error during customer authentication", ex);
                throw;
            }
        }

        public Admin AuthenticateAdmin(string email, string password)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT Id, Name, Email, Password, Department, CreatedAt, IsActive FROM Admins WHERE Email = @Email AND Password = @Password;";
                        command.Parameters.AddWithValue("@Email", email);
                        command.Parameters.AddWithValue("@Password", password);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var admin = new Admin
                                {
                                    Id = (int)reader["Id"],
                                    Name = (string)reader["Name"],
                                    Email = (string)reader["Email"],
                                    Password = (string)reader["Password"],
                                    Department = reader["Department"] != DBNull.Value ? (string)reader["Department"] : "",
                                    CreatedAt = (DateTime)reader["CreatedAt"],
                                    IsActive = (bool)reader["IsActive"]
                                };
                                _logger.LogInfo($"Admin authenticated: {email}");
                                return admin;
                            }
                        }
                    }
                }
                _logger.LogWarning($"Failed admin authentication attempt for email: {email}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error during admin authentication", ex);
                throw;
            }
        }

        /// <summary>
        /// Helper method to fetch order items from the database.
        /// This version opens its own connection to avoid nested DataReaders on the same connection.
        /// </summary>
        private List<OrderItem> GetOrderItems(int orderId)
        {
            var items = new List<OrderItem>();
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT ProductId, ProductName, UnitPrice, Quantity FROM OrderItems WHERE OrderId = @OrderId;";
                        command.Parameters.AddWithValue("@OrderId", orderId);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                items.Add(new OrderItem
                                {
                                    ProductId = (int)reader["ProductId"],
                                    ProductName = (string)reader["ProductName"],
                                    UnitPrice = (decimal)reader["UnitPrice"],
                                    Quantity = (int)reader["Quantity"]
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching order items for order {orderId}", ex);
                // return empty list on failure to allow callers to handle gracefully
            }
            return items;
        }
    }
}
