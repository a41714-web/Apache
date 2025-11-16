# MySQL Database Setup Guide for Apache Application

## Overview
The Apache application has been refactored to use a local MySQL database instead of in-memory data initialization. All products, customers, admins, and orders are now persisted in MySQL.

## Prerequisites

### Required Software
- **MySQL Server 8.0+** - Community Edition is sufficient
- Download from: https://dev.mysql.com/downloads/mysql/

### Installation Steps (Windows)

#### 1. Install MySQL Server
1. Download MySQL Community Server from the official website
2. Run the installer and follow the setup wizard
3. During installation:
   - Choose "MySQL Server" for the setup type
   - Use port **3306** (default)
   - Configure MySQL as a Windows Service
   - Choose **Standard Configuration**

#### 2. Configure MySQL Root User
During installation, you'll be prompted to set credentials:
- **Username**: `root` (default)
- **Password**: `root` (as configured in the connection string)

**?? Important**: The current connection string uses:
```
Server=localhost;Database=apache_db;Uid=root;Pwd=root;
```

If you prefer different credentials, update `DatabaseConfig.cs` line 47.

#### 3. Verify MySQL Installation
Open Command Prompt and test the connection:
```bash
mysql -u root -proot -h localhost
```

You should see the MySQL prompt. Type `exit` to quit.

## Database Auto-Initialization

When you run the Apache application for the first time:

1. **DatabaseConfig.cs** automatically initializes:
   - Creates `apache_db` database if it doesn't exist
   - Creates all required tables:
     - `Products`
     - `Customers`
     - `Admins`
     - `Orders`
     - `OrderItems`
   - Seeds initial sample data (5 products, 2 customers, 1 admin)

2. **Subsequent runs** preserve all data and only verify table existence

## Database Schema

### Products Table
```sql
CREATE TABLE Products (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Name VARCHAR(255) NOT NULL,
    Description TEXT,
    Price DECIMAL(10, 2) NOT NULL,
    Stock INT NOT NULL,
    Category VARCHAR(100),
    ImageUrl VARCHAR(255),
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

### Customers Table
```sql
CREATE TABLE Customers (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Name VARCHAR(255) NOT NULL,
    Email VARCHAR(255) NOT NULL UNIQUE,
    Password VARCHAR(255) NOT NULL,
    Address VARCHAR(500),
    PhoneNumber VARCHAR(20),
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    IsActive BOOLEAN DEFAULT TRUE
);
```

### Admins Table
```sql
CREATE TABLE Admins (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Name VARCHAR(255) NOT NULL,
    Email VARCHAR(255) NOT NULL UNIQUE,
    Password VARCHAR(255) NOT NULL,
    Department VARCHAR(100),
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    IsActive BOOLEAN DEFAULT TRUE
);
```

### Orders Table
```sql
CREATE TABLE Orders (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    CustomerId INT NOT NULL,
    OrderDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    Status VARCHAR(50) DEFAULT 'Pending',
    FOREIGN KEY (CustomerId) REFERENCES Customers(Id)
);
```

### OrderItems Table
```sql
CREATE TABLE OrderItems (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    OrderId INT NOT NULL,
    ProductId INT NOT NULL,
    ProductName VARCHAR(255),
    UnitPrice DECIMAL(10, 2),
    Quantity INT NOT NULL,
    FOREIGN KEY (OrderId) REFERENCES Orders(Id),
    FOREIGN KEY (ProductId) REFERENCES Products(Id)
);
```

## Default Login Credentials

### Customer Accounts
| Email | Password |
|-------|----------|
| john@example.com | password123 |
| jane@example.com | password123 |

### Admin Account
| Email | Password |
|-------|----------|
| admin@apache.com | adminpass123 |

## Troubleshooting

### Connection String Issues

**Error**: "Unable to connect to MySQL server"

**Solutions**:
1. Verify MySQL is running:
   ```bash
   mysql -u root -proot
   ```
2. Check the connection string in `DatabaseConfig.cs`:
   - `Server=localhost` - MySQL server address
   - `Database=apache_db` - Database name
   - `Uid=root` - Username
   - `Pwd=root` - Password

3. If using different credentials, update `DatabaseConfig.cs`:
   ```csharp
   _connectionString = "Server=your_server;Database=apache_db;Uid=your_user;Pwd=your_password;";
   ```

### Database Already Exists

If `apache_db` already exists with different data:
- The app will **not** reinitialize existing data
- To reset, manually drop the database:
  ```sql
  DROP DATABASE apache_db;
  ```
- Then restart the application to recreate everything

### Password Reset

To change MySQL root password:
```bash
mysql -u root -proot -e "ALTER USER 'root'@'localhost' IDENTIFIED BY 'newpassword';"
```

Update `DatabaseConfig.cs` with the new password.

## Key Changes from In-Memory Data

### Before (In-Memory)
```csharp
private List<Product> _products;
private void InitializeData() { ... }
```

### After (MySQL)
```csharp
public IReadOnlyList<Product> GetAllProducts()
{
    using (var connection = new MySqlConnection(_connectionString))
    {
        connection.Open();
        // Query from MySQL database
    }
}
```

## Performance Considerations

- **Initial Load**: First database query may take slightly longer (connection pooling)
- **Concurrent Access**: MySQL handles multiple simultaneous connections safely
- **Data Persistence**: All changes are immediately persisted to disk
- **Transactions**: Order insertion uses transactions to ensure data consistency

## File Changes Summary

| File | Changes |
|------|---------|
| `Apache/Data/DatabaseConfig.cs` | **NEW** - Database initialization and table creation |
| `Apache/Data/DataRepository.cs` | Refactored from in-memory to MySQL queries |
| `Apache/Models/Product.cs` | Removed ID auto-generation (DB handles it) |
| `Apache/Models/Order.cs` | Removed ID auto-generation (DB handles it) |
| `Apache/Models/User.cs` | Removed ID auto-generation (DB handles it) |
| `Apache/MauiProgram.cs` | Added database initialization |
| `Apache/Apache.csproj` | Added MySql.Data v9.1.0 NuGet package |

## Next Steps

1. **Install MySQL Server** following the prerequisites section
2. **Start MySQL Service** (it should auto-start if configured)
3. **Run the Apache application** - database will auto-initialize
4. **Log in** with the default credentials above
5. **Test CRUD operations** - all changes are now persisted in MySQL

## Support

For MySQL-related issues:
- Official MySQL Documentation: https://dev.mysql.com/doc/
- MySQL Community: https://www.mysql.com/why-mysql/
