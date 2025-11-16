using System;

namespace Apache.Models
{
    /// <summary>
    /// Represents a product in the Apache marketplace.
    /// Demonstrates OOP encapsulation and data validation.
    /// </summary>
    public class Product
    {
        private string _name;
        private decimal _price;
        private int _stock;

        public int Id { get; set; }

        public string Name
        {
            get => _name;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Product name cannot be empty");
                _name = value;
            }
        }

        public string Description { get; set; }

        public decimal Price
        {
            get => _price;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Price cannot be negative");
                _price = value;
            }
        }

        public int Stock
        {
            get => _stock;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Stock cannot be negative");
                _stock = value;
            }
        }

        public string Category { get; set; }
        public string ImageUrl { get; set; }

        public Product()
        {
            // Auto-ID generation removed, as IDs are managed by the database
            Stock = 0;
        }

        /// <summary>
        /// Reduces stock when a purchase is made.
        /// </summary>
        public void ReduceStock(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be greater than 0");
            
            if (quantity > Stock)
                throw new InvalidOperationException($"Insufficient stock. Available: {Stock}");
            
            Stock -= quantity;
        }

        /// <summary>
        /// Increases stock when new inventory arrives.
        /// </summary>
        public void AddStock(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be greater than 0");
            
            Stock += quantity;
        }

        public override string ToString()
        {
            return $"Product: {Name} | Price: ${Price} | Stock: {Stock}";
        }
    }
}
