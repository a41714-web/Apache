using System;

namespace Apache.Models
{
    /// <summary>
    /// Represents an order in the Apache system.
    /// Demonstrates composition and encapsulation.
    /// </summary>
    public class Order
    {
        private List<OrderItem> _items;

        public int Id { get; set; }
        public int CustomerId { get; set; }
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; }
        public List<OrderItem> Items
        {
            get => _items;
            private set => _items = value;
        }

        public Order()
        {
            OrderDate = DateTime.Now;
            Status = OrderStatus.Pending;
            Items = new List<OrderItem>();
        }

        /// <summary>
        /// Adds an item to the order.
        /// </summary>
        public void AddItem(Product product, int quantity)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));
            
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be greater than 0");

            var existingItem = Items.FirstOrDefault(i => i.ProductId == product.Id);
            
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                Items.Add(new OrderItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    UnitPrice = product.Price,
                    Quantity = quantity
                });
            }
        }

        /// <summary>
        /// Calculates the total order amount.
        /// </summary>
        public decimal GetTotal()
        {
            return Items.Sum(item => item.UnitPrice * item.Quantity);
        }

        /// <summary>
        /// Gets the number of items in the order.
        /// </summary>
        public int GetItemCount()
        {
            return Items.Sum(item => item.Quantity);
        }

        public override string ToString()
        {
            return $"Order #{Id} | Customer: {CustomerId} | Status: {Status} | Total: ${GetTotal():F2}";
        }
    }

    /// <summary>
    /// Represents a single item in an order.
    /// </summary>
    public class OrderItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }

        public decimal GetLineTotal()
        {
            return UnitPrice * Quantity;
        }
    }

    /// <summary>
    /// Enum representing possible order statuses.
    /// </summary>
    public enum OrderStatus
    {
        Pending,
        Confirmed,
        Shipped,
        Delivered,
        Cancelled
    }
}
