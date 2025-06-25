using order_management_api_assessment.Features.Orders.Command.Create;
using order_management_api_assessment.Shared.Enums;

namespace order_management_api_assessment.Features.Orders;

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

        var order = new Order(Guid.NewGuid(), customerId, 1, 1, OrderStatus.Pending);

        return order;
    }

    public void AddOrderItems(HashSet<OrderItemDto> items)
    {
        var orderItems = items.Select(o => OrderItem.Create(OrderId, o.ProductId, o.Quantity, o.UnitPrice));
        _orderItems.AddRange(orderItems);
    }

    public void UpdateStatus(OrderStatus newStatus)
    {
        if(!Status.CanTransitionTo(newStatus))
            throw new InvalidOperationException($"Cannot transition from {Status} to {newStatus}");

        Status = newStatus;

        if(newStatus  == OrderStatus.Delivered)
            FulfilledAt = DateTime.UtcNow;
    }

    public void ApplyDiscount(decimal discountAmount) => DiscountedAmount = discountAmount;
}
