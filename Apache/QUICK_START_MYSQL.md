# Quick Start Guide - MySQL Setup

## 5-Minute Setup

### Step 1: Install MySQL
1. Download: https://dev.mysql.com/downloads/mysql/
2. Run installer, use default settings
3. Set password: `root`

### Step 2: Verify Installation
```bash
mysql -u root -proot -h localhost
```
Type `exit` to quit.

### Step 3: Run Apache App
- App automatically creates `apache_db` database
- App automatically creates all tables
- App automatically seeds sample data
- **Done!** ??

## Login Credentials

**Customer**
- Email: `john@example.com`
- Password: `password123`

**Admin**
- Email: `admin@apache.com`
- Password: `adminpass123`

## If MySQL Isn't Working

### Issue: "Can't connect to MySQL server"

**Fix**: Check MySQL is running
```bash
mysql -u root -proot
```

If it fails, start MySQL service:
- **Windows**: Search "Services" ? Find "MySQL80" ? Right-click ? Start
- **Mac**: `brew services start mysql`
- **Linux**: `sudo systemctl start mysql`

### Issue: Wrong password

Edit `Apache/Data/DatabaseConfig.cs` line 47:
```csharp
_connectionString = "Server=localhost;Database=apache_db;Uid=root;Pwd=YOUR_PASSWORD;";
```

### Issue: "Database already exists with old data"

Delete and recreate:
```bash
mysql -u root -proot -e "DROP DATABASE apache_db;"
```

Then restart app.

## Files to Know

| File | Purpose |
|------|---------|
| `DatabaseConfig.cs` | Initializes MySQL, creates tables, seeds data |
| `DataRepository.cs` | All database queries (Products, Customers, Orders) |
| `DATABASE_SETUP.md` | Detailed setup guide |
| `MIGRATION_SUMMARY.md` | Technical details of changes |

## What's New

? Data persists after app closes
? Multiple app instances can use same database
? Automatic database initialization
? SQL injection protected (parameterized queries)
? Transaction support for orders

## Key Configuration

**Connection String**: `Apache/Data/DatabaseConfig.cs` line 47
```csharp
"Server=localhost;Database=apache_db;Uid=root;Pwd=root;"
```

Change any of these:
- `localhost` ? MySQL server address
- `apache_db` ? Database name
- `root` ? Username
- `root` ? Password

## Test It Works

1. Login to app
2. Add product to cart
3. Place order
4. Close app
5. Reopen app
6. Login again
7. View past orders ? Should show your order!

If you see your order, database is working! ?

---

**Need more help?** See `DATABASE_SETUP.md` for detailed troubleshooting.
