namespace order_management_api_assessment.Features.Orders;

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

        var orderItem = new OrderItem(orderId, productId, quantity, unitPrice);
        return orderItem;
    }
}
