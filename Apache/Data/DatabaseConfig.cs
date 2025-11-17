using Apache.Services;
using MySql.Data.MySqlClient;
using System.Configuration;
using System.Xml.Linq;
using System.IO;
using System;
using System.Linq;
using Microsoft.Maui.Storage;
using System.Threading.Tasks;
using Microsoft.Maui.Devices;

namespace Apache.Data
{
    /// <summary>
    /// Connecção e configuração do banco de dados MySQL.
    /// </summary>
    public class DatabaseConfig
    {
        private readonly string _connectionString;
        private readonly string _noDatabaseConnectionString;
        private readonly LoggingService _logger;
        private bool _isOnline = false;
        private const int MaxRetries = 3;
        private const int RetryDelayMs = 1000;

        // Construtor que aceita uma string de conexão.
        public DatabaseConfig(string connectionString, string noDatabaseConnectionString = null)
        {
            _logger = LoggingService.Instance;

            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _noDatabaseConnectionString = !string.IsNullOrWhiteSpace(noDatabaseConnectionString)
                ? noDatabaseConnectionString
                : DeriveNoDatabaseConnectionString(_connectionString);

            InitializeDatabase();
        }

        /// <summary>
        /// Fábrica para criar um DatabaseConfig usando arquivos de configuração (ConfigurationManager ou fallback Config.xml).
        /// Fábrica - É um padrão de projeto criacional que fornece uma interface para a criação de objetos em uma superclasse, 
        /// mas permite que as subclasses alterem o tipo de objetos que serão criados.
        /// </summary>
        public static DatabaseConfig CreateFromConfiguration()
        {
            var connectionString = GetConnectionString("Database")
                ?? throw new InvalidOperationException("Connection string 'Database' not found. Please add it to your configuration (Config.xml or app config).");

            var noDb = GetConnectionString("NoDatabase");

            return new DatabaseConfig(connectionString, noDb);
        }

        public string ConnectionString => _connectionString;
        
        /// <summary>
        /// Indica se a aplicação está em modo online (conectado ao banco de dados).
        /// </summary>
        public bool IsOnline => _isOnline;

        /// <summary>
        /// Initializes the database with required tables if they don't exist.
        /// </summary>
        private void InitializeDatabase()
        {
            try
            {
                // Tentar conectar com retry automático
                if (TryConnectWithRetry())
                {
                    _isOnline = true;
                    CreateDatabaseIfNotExists();
                    CreateTablesIfNotExist();
                    SeedInitialData();
                    _logger.LogInfo("Database initialized successfully");
                }
                else
                {
                    _isOnline = false;
                    _logger.LogWarning("Database initialization skipped - running in offline mode. The application will use local cache or mock data.");
                }
            }
            catch (Exception ex)
            {
                _isOnline = false;
                _logger.LogError("Error initializing database. Application will continue in offline mode.", ex);
                // Não relançar a exceção - permitir que a aplicação continue em modo offline
            }
        }

        /// <summary>
        /// Tenta conectar ao banco de dados com retry automático.
        /// </summary>
        private bool TryConnectWithRetry()
        {
            for (int attempt = 1; attempt <= MaxRetries; attempt++)
            {
                try
                {
                    _logger.LogDebug($"Attempting to connect to database (attempt {attempt}/{MaxRetries})");
                    
                    using (var connection = new MySqlConnection(_noDatabaseConnectionString))
                    {
                        connection.Open();
                        _logger.LogInfo("Successfully connected to database");
                        return true;
                    }
                }
                catch (MySqlException sqlEx)
                {
                    _logger.LogWarning($"Database connection attempt {attempt} failed: {sqlEx.Message}");
                    
                    if (attempt < MaxRetries)
                    {
                        System.Threading.Thread.Sleep(RetryDelayMs * attempt); // Backoff progressivo
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Unexpected error during database connection attempt {attempt}", ex);
                    if (attempt < MaxRetries)
                    {
                        System.Threading.Thread.Sleep(RetryDelayMs * attempt);
                    }
                }
            }

            _logger.LogError("Failed to connect to database after {0} attempts. The application will continue in offline mode.", null);
            return false;
        }

        /// <summary>
        /// Executar uma operação de banco de dados com tratamento de erro gracioso.
        /// </summary>
        private bool ExecuteWithFallback(Action<MySqlConnection> action)
        {
            if (!_isOnline)
                return false;

            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    action(connection);
                    return true;
                }
            }
            catch (MySqlException sqlEx)
            {
                _logger.LogWarning($"Database operation failed: {sqlEx.Message}. Switching to offline mode.");
                _isOnline = false;
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError("Unexpected error during database operation", ex);
                _isOnline = false;
                return false;
            }
        }

        /// <summary>
        /// Criar o banco de dados se ele não existir.
        /// </summary>
        private void CreateDatabaseIfNotExists()
        {
            ExecuteWithFallback(connection =>
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "CREATE DATABASE IF NOT EXISTS apache_db;";
                    command.ExecuteNonQuery();
                    _logger.LogInfo("Database 'apache_db' ready");
                }
            });
        }

        /// <summary>
        /// Criar tabelas necessárias se elas não existirem.
        /// </summary>
        private void CreateTablesIfNotExist()
        {
            ExecuteWithFallback(connection =>
            {
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
                        FOREIGN KEY (CustomerId) REFERENCES Customers(Id) ON DELETE CASCADE
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
                        FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE CASCADE,
                        FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE
                    );";

                ExecuteCommand(connection, createProductsTable);
                ExecuteCommand(connection, createCustomersTable);
                ExecuteCommand(connection, createAdminsTable);
                ExecuteCommand(connection, createOrdersTable);
                ExecuteCommand(connection, createOrderItemsTable);

                _logger.LogInfo("Database tables created/verified");
            });
        }

        /// <summary>
        /// Adiciona dados iniciais se as tabelas estiverem vazias.
        /// </summary>
        private void SeedInitialData()
        {
            ExecuteWithFallback(connection =>
            {
                // Check if products table is empty
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT COUNT(*) FROM Products;";
                    int productCount = command.ExecuteScalar() is long count ? (int)count : 0;

                    if (productCount == 0)
                    {
                        SeedProducts(connection);
                        SeedCustomers(connection);
                        SeedAdmins(connection);
                        _logger.LogInfo("Initial data seeded successfully");
                    }
                }
            });
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
        /// Exemplo de clientes iniciais.
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
        /// Exemplo de administrador inicial.
        /// </summary>
        private void SeedAdmins(MySqlConnection connection)
        {
            string insertStatement = "INSERT INTO Admins (Name, Email, Password, Department) VALUES ('Admin User', 'admin@apache.com', 'adminpass123', 'Management');";
            ExecuteCommand(connection, insertStatement);
        }

        /// <summary>
        /// Método auxiliar para executar comandos SQL.
        /// </summary>
        private void ExecuteCommand(MySqlConnection connection, string commandText)
        {
            try
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = commandText;
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error executing SQL command: {ex.Message}");
                // Continuar mesmo se um comando individual falhar
            }
        }

        private static string GetConnectionString(string name)
        {
            try
            {
                var fromConfigManager = ConfigurationManager.ConnectionStrings[name]?.ConnectionString;
                if (!string.IsNullOrWhiteSpace(fromConfigManager))
                    return AdjustForPlatform(fromConfigManager);

                // For Android/MAUI apps, try to load from assembly resources or app context
                if (TryLoadFromAsset(name, out var assetConnectionString))
                    return AdjustForPlatform(assetConnectionString);

                // Tentar encontrar um Config.xml na pasta do aplicativo ou em pastas pai
                var baseDir = AppContext.BaseDirectory;
                var dir = new DirectoryInfo(baseDir);
                while (dir != null)
                {
                    var candidate = Path.Combine(dir.FullName, "Config.xml");
                    if (File.Exists(candidate))
                    {
                        try
                        {
                            var doc = XDocument.Load(candidate);
                            var add = doc.Root?
                                        .Element("connectionStrings")?
                                        .Elements("add")
                                        .FirstOrDefault(e => (string)e.Attribute("name") == name);
                            var cs = (string?)add?.Attribute("connectionString");
                            if (!string.IsNullOrWhiteSpace(cs))
                                return AdjustForPlatform(cs);
                        }
                        catch
                        {
                            // ignore malformed xml and keep searching
                        }
                    }
                    dir = dir.Parent;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        private static bool TryLoadFromAsset(string connectionStringName, out string connectionString)
        {
            connectionString = null;
            try
            {
                // Try to load Config.xml from app context or files directory
                var configPath = Path.Combine(FileSystem.AppDataDirectory, "Config.xml");
                if (File.Exists(configPath))
                {
                    var doc = XDocument.Load(configPath);
                    var add = doc.Root?
                                .Element("connectionStrings")?
                                .Elements("add")
                                .FirstOrDefault(e => (string)e.Attribute("name") == connectionStringName);
                    var cs = (string?)add?.Attribute("connectionString");
                    if (!string.IsNullOrWhiteSpace(cs))
                    {
                        connectionString = AdjustForPlatform(cs);
                        return true;
                    }
                }
            }
            catch
            {
                // Ignore errors
            }

            return false;
        }

        private static string AdjustForPlatform(string connectionString)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(connectionString))
                    return connectionString;

                // If running on Android emulator, "localhost" refers to the emulator itself.
                // Common emulator host mapping: 10.0.2.2 (Android emulator), 10.0.3.2 (Genymotion)
                if (DeviceInfo.Platform == DevicePlatform.Android)
                {
                    // Prefer 10.0.2.2 for the default Android emulator
                    if (connectionString.IndexOf("localhost", StringComparison.OrdinalIgnoreCase) >= 0
                        || connectionString.IndexOf("127.0.0.1", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        var replaced = connectionString
                            .Replace("localhost", "10.0.2.2", StringComparison.OrdinalIgnoreCase)
                            .Replace("127.0.0.1", "10.0.2.2", StringComparison.OrdinalIgnoreCase);

                        try
                        {
                            LoggingService.Instance?.LogWarning($"Adjusted connection string for Android emulator: '{connectionString}' => '{replaced}'");
                        }
                        catch
                        {
                            // ignore logging errors
                        }

                        return replaced;
                    }
                }

                return connectionString;
            }
            catch
            {
                return connectionString;
            }
        }

        private static string DeriveNoDatabaseConnectionString(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                return connectionString;

            var parts = connectionString.Split(';')
                .Select(p => p.Trim())
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Where(p => !p.StartsWith("Database=", StringComparison.OrdinalIgnoreCase) && !p.StartsWith("Initial Catalog=", StringComparison.OrdinalIgnoreCase))
                .ToList();

            return string.Join(";", parts) + (parts.Count > 0 ? ";" : string.Empty);
        }
    }
}