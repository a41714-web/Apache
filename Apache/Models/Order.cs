using System;

namespace Apache.Models
{
    /// <summary>
    /// Inteface definindo um pedido
    /// </summary>
    public interface IOrder
    {
        int Id { get; set; }
        int CustomerId { get; set; }
        DateTime OrderDate { get; set; }
        OrderStatus Status { get; set; }
    }
    /// <summary>
    /// Classe que representa um pedido
    /// </summary>
    public class Order : IOrder
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
            _items = new List<OrderItem>();
            OrderDate = DateTime.Now;
            Status = OrderStatus.Pending;
            Items = _items;
        }

        /// <summary>
        /// Addiciona um item ao pedido
        /// </summary>
        public void AddItem(Product product, int quantity)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));
            
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be greater than 0");

            // Verifica se o item já existe no pedido
            // i => i.ProductId == product.Id -- expressão lambda para encontrar o item
            var existingItem = Items.FirstOrDefault(i => i.ProductId == product.Id);

            // Se o item já existe, atualiza a quantidade
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            // Se o item não existe, adiciona um novo item
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
        /// Calcula o total do pedido.
        /// </summary>
        public decimal GetTotal()
        {
            return Items.Sum(item => item.UnitPrice * item.Quantity);
        }

        /// <summary>
        /// Numero total de itens no pedido.
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
    /// Um item dentro de um pedido
    /// </summary>
    public class OrderItem
    {
        public int ProductId { get; set; }
        public required string ProductName { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }

        public decimal GetLineTotal()
        {
            return UnitPrice * Quantity;
        }
    }

    /// <summary>
    /// Enumeração representando o status do pedido
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
