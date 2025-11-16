using Apache.Services;
using MySql.Data.MySqlClient;

namespace Apache.Data
{
    /// <summary>
    /// Handles MySQL database connection and initialization.
    /// </summary>
    public class DatabaseConfig
    {
        private static DatabaseConfig _instance;
        private static readonly object _instanceLock = new object();
        private readonly string _connectionString;
        private readonly LoggingService _logger;

        public static DatabaseConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_instanceLock)
                    {
                        if (_instance == null)
                        {
                            _instance = new DatabaseConfig();
                        }
                    }
                }
                return _instance;
            }
        }

        private DatabaseConfig()
        {
            _logger = LoggingService.Instance;
            // Connection string for local MySQL server
            _connectionString = "Server=localhost;Database=apache_db;Uid=root;Pwd=root;";
            InitializeDatabase();
        }

        public string ConnectionString => _connectionString;

        /// <summary>
        /// Initializes the database with required tables if they don't exist.
        /// </summary>
        private void InitializeDatabase()
        {
            try
            {
                CreateDatabaseIfNotExists();
                CreateTablesIfNotExist();
                SeedInitialData();
                _logger.LogInfo("Database initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error initializing database", ex);
                throw;
            }
        }

        /// <summary>
        /// Creates the database if it doesn't exist.
        /// </summary>
        private void CreateDatabaseIfNotExists()
        {
            string connectionStringWithoutDb = "Server=localhost;Uid=root;Pwd=root;";
            using (var connection = new MySqlConnection(connectionStringWithoutDb))
            {
                try
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "CREATE DATABASE IF NOT EXISTS apache_db;";
                        command.ExecuteNonQuery();
                        _logger.LogInfo("Database 'apache_db' ready");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error creating database", ex);
                    throw;
                }
            }
        }

        /// <summary>
        /// Creates required tables if they don't exist.
        /// </summary>
        private void CreateTablesIfNotExist()
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();

                    // Create Products table
                    string createProductsTable = @"
                        CREATE TABLE IF NOT EXISTS Products (
                            Id INT PRIMARY KEY AUTO_INCREMENT,
                            Name VARCHAR(255) NOT NULL,
                            Description TEXT,
                            Price DECIMAL(10, 2) NOT NULL,
                            Stock INT NOT NULL,
                            Category VARCHAR(100),
                            ImageUrl VARCHAR(255),
                            CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                        );";

                    // Create Customers table
                    string createCustomersTable = @"
                        CREATE TABLE IF NOT EXISTS Customers (
                            Id INT PRIMARY KEY AUTO_INCREMENT,
                            Name VARCHAR(255) NOT NULL,
                            Email VARCHAR(255) NOT NULL UNIQUE,
                            Password VARCHAR(255) NOT NULL,
                            Address VARCHAR(500),
                            PhoneNumber VARCHAR(20),
                            CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                            IsActive BOOLEAN DEFAULT TRUE
                        );";

                    // Create Admins table
                    string createAdminsTable = @"
                        CREATE TABLE IF NOT EXISTS Admins (
                            Id INT PRIMARY KEY AUTO_INCREMENT,
                            Name VARCHAR(255) NOT NULL,
                            Email VARCHAR(255) NOT NULL UNIQUE,
                            Password VARCHAR(255) NOT NULL,
                            Department VARCHAR(100),
                            CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                            IsActive BOOLEAN DEFAULT TRUE
                        );";

                    // Create Orders table
                    string createOrdersTable = @"
                        CREATE TABLE IF NOT EXISTS Orders (
                            Id INT PRIMARY KEY AUTO_INCREMENT,
                            CustomerId INT NOT NULL,
                            OrderDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                            Status VARCHAR(50) DEFAULT 'Pending',
                            FOREIGN KEY (CustomerId) REFERENCES Customers(Id)
                        );";

                    // Create OrderItems table
                    string createOrderItemsTable = @"
                        CREATE TABLE IF NOT EXISTS OrderItems (
                            Id INT PRIMARY KEY AUTO_INCREMENT,
                            OrderId INT NOT NULL,
                            ProductId INT NOT NULL,
                            ProductName VARCHAR(255),
                            UnitPrice DECIMAL(10, 2),
                            Quantity INT NOT NULL,
                            FOREIGN KEY (OrderId) REFERENCES Orders(Id),
                            FOREIGN KEY (ProductId) REFERENCES Products(Id)
                        );";

                    ExecuteCommand(connection, createProductsTable);
                    ExecuteCommand(connection, createCustomersTable);
                    ExecuteCommand(connection, createAdminsTable);
                    ExecuteCommand(connection, createOrdersTable);
                    ExecuteCommand(connection, createOrderItemsTable);

                    _logger.LogInfo("Database tables created/verified");
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error creating tables", ex);
                    throw;
                }
            }
        }

        /// <summary>
        /// Seeds initial data if tables are empty.
        /// </summary>
        private void SeedInitialData()
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();

                    // Check if products table is empty
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT COUNT(*) FROM Products;";
                        int productCount = (int)(long)command.ExecuteScalar();

                        if (productCount == 0)
                        {
                            SeedProducts(connection);
                            SeedCustomers(connection);
                            SeedAdmins(connection);
                            _logger.LogInfo("Initial data seeded successfully");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error seeding initial data", ex);
                    throw;
                }
            }
        }

        /// <summary>
        /// Seeds sample products.
        /// </summary>
        private void SeedProducts(MySqlConnection connection)
        {
            string[] insertStatements = new[]
            {
                "INSERT INTO Products (Name, Description, Price, Stock, Category, ImageUrl) VALUES ('Laptop Pro', 'High-performance laptop for professionals', 1299.99, 15, 'Electronics', 'laptop.png');",
                "INSERT INTO Products (Name, Description, Price, Stock, Category, ImageUrl) VALUES ('Wireless Mouse', 'Ergonomic wireless mouse with 3 buttons', 29.99, 50, 'Accessories', 'mouse.png');",
                "INSERT INTO Products (Name, Description, Price, Stock, Category, ImageUrl) VALUES ('USB-C Cable', 'Fast charging USB-C cable, 2 meters', 14.99, 100, 'Cables', 'cable.png');",
                "INSERT INTO Products (Name, Description, Price, Stock, Category, ImageUrl) VALUES ('Mechanical Keyboard', 'RGB mechanical keyboard with 104 keys', 129.99, 25, 'Accessories', 'keyboard.png');",
                "INSERT INTO Products (Name, Description, Price, Stock, Category, ImageUrl) VALUES ('4K Monitor', '32-inch 4K UltraHD monitor', 499.99, 10, 'Electronics', 'monitor.png');"
            };

            foreach (var statement in insertStatements)
            {
                ExecuteCommand(connection, statement);
            }
        }

        /// <summary>
        /// Seeds sample customers.
        /// </summary>
        private void SeedCustomers(MySqlConnection connection)
        {
            string[] insertStatements = new[]
            {
                "INSERT INTO Customers (Name, Email, Password, Address, PhoneNumber) VALUES ('John Doe', 'john@example.com', 'password123', '123 Main St, Springfield', '555-0100');",
                "INSERT INTO Customers (Name, Email, Password, Address, PhoneNumber) VALUES ('Jane Smith', 'jane@example.com', 'password123', '456 Oak Ave, Shelbyville', '555-0101');"
            };

            foreach (var statement in insertStatements)
            {
                ExecuteCommand(connection, statement);
            }
        }

        /// <summary>
        /// Seeds sample admin.
        /// </summary>
        private void SeedAdmins(MySqlConnection connection)
        {
            string insertStatement = "INSERT INTO Admins (Name, Email, Password, Department) VALUES ('Admin User', 'admin@apache.com', 'adminpass123', 'Management');";
            ExecuteCommand(connection, insertStatement);
        }

        /// <summary>
        /// Helper method to execute SQL commands.
        /// </summary>
        private void ExecuteCommand(MySqlConnection connection, string commandText)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = commandText;
                command.ExecuteNonQuery();
            }
        }
    }
}
