using System;

namespace Apache.Models
{
    /// <summary> 
    /// Interface definindo um produto
    /// </summary>
    public interface IProduct
    {
        int Id { get; set; }
        string Name { get; set; }
        string Description { get; set; }
        decimal Price { get; set; }
        int Stock { get; set; }
        string Category { get; set; }
        string ImageUrl { get; set; }
    }
    /// <summary>
    /// Classe que representa um produto
    /// </summary>
    public class Product : IProduct
    {
        private string _name = string.Empty;
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

        public string Description { get; set; } = string.Empty;
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

        public string Category { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;

        public Product()
        {
            Stock = 0;
        }

        /// <summary>
        /// Atualizar stock quando uma venda é realizada.
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
        /// Atualizar stock quando novos produtos chegam.
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
