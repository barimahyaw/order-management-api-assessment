using order_management_api_assessment.DTOs;

namespace order_management_api_assessment.Models;

public class Order
{
    public Guid OrderId { get; private set; }
    public Guid CustomerId { get; private set; }
    public decimal TotalAmount { get; private set; }
    public decimal DiscountedAmount { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? FulfilledAt { get; private set; }

    private readonly List<OrderItem> _orderItems = [];
    public IReadOnlyList<OrderItem> OrderItems => _orderItems;

    private Order() { }

    private Order(Guid orderId, Guid customerId, decimal totalAmount, decimal discountedAmount, OrderStatus status)
    {
        OrderId = orderId;
        CustomerId = customerId;
        TotalAmount = totalAmount;
        DiscountedAmount = discountedAmount;
        Status = status;
        CreatedAt = DateTime.UtcNow;
    }

    public static Order Create(Guid customerId)
    {
        if (customerId == Guid.Empty) throw new ArgumentNullException(nameof(customerId));
        return new Order(Guid.NewGuid(), customerId, 0, 0, OrderStatus.Pending);
    }

    public void AddOrderItems(IEnumerable<OrderItemDto> items)
    {
        var orderItems = items.Select(o => OrderItem.Create(OrderId, o.ProductId, o.Quantity, o.UnitPrice));
        _orderItems.AddRange(orderItems);
        UpdateTotalAmount();
    }

    public void UpdateStatus(OrderStatus newStatus)
    {
        if (!Status.CanTransitionTo(newStatus))
            throw new InvalidOperationException($"Cannot transition from {Status} to {newStatus}");

        Status = newStatus;

        if (newStatus == OrderStatus.Delivered)
            FulfilledAt = DateTime.UtcNow;
    }

    public void ApplyDiscount(decimal discountAmount)
    {
        DiscountedAmount = TotalAmount - discountAmount;
    }

    private void UpdateTotalAmount()
    {
        TotalAmount = _orderItems.Sum(item => item.Quantity * item.Price);
        DiscountedAmount = TotalAmount; // Default to no discount
    }

    public class OrderItem
    {
        public Guid Id { get; private set; }
        public Guid OrderId { get; private set; }
        public Guid ProductId { get; private set; }
        public int Quantity { get; private set; }
        public decimal Price { get; private set; }

        private OrderItem() { }

        private OrderItem(Guid orderId, Guid productId, int quantity, decimal price)
        {
            Id = Guid.NewGuid();
            OrderId = orderId;
            ProductId = productId;
            Quantity = quantity;
            Price = price;
        }

        public static OrderItem Create(Guid orderId, Guid productId, int quantity, decimal unitPrice)
        {
            if (orderId == Guid.Empty) throw new ArgumentNullException(nameof(orderId));
            if (productId == Guid.Empty) throw new ArgumentNullException(nameof(productId));
            ArgumentOutOfRangeException.ThrowIfNegative(quantity);
            ArgumentOutOfRangeException.ThrowIfNegative(unitPrice);

            return new OrderItem(orderId, productId, quantity, unitPrice);
        }
    }
}