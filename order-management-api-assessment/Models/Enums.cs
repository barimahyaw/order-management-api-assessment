namespace order_management_api_assessment.Models;

public enum OrderStatus
{
    Pending = 1,
    Processing = 2,
    Shipped = 3,
    Delivered = 4,
    Cancelled = 5,
    Returned = 6
}

public enum CustomerSegment
{
    Regular = 1,
    Premium,
    VIP
}

public static class OrderStatusExtensions
{
    private static readonly Dictionary<OrderStatus, List<OrderStatus>> _validTransitions = new()
    {
        { OrderStatus.Pending, [OrderStatus.Processing, OrderStatus.Cancelled] },
        { OrderStatus.Processing, [OrderStatus.Shipped, OrderStatus.Cancelled] },
        { OrderStatus.Shipped, [OrderStatus.Delivered, OrderStatus.Returned] },
        { OrderStatus.Delivered, [] },
        { OrderStatus.Cancelled, [] },
        { OrderStatus.Returned, [] }
    };

    public static bool CanTransitionTo(this OrderStatus currentStatus, OrderStatus newStatus)
        => _validTransitions.TryGetValue(currentStatus, out var validStatuses) && validStatuses.Contains(newStatus);

    public static List<OrderStatus> GetValidTransitions(this OrderStatus currentStatus)
        => _validTransitions.TryGetValue(currentStatus, out var validStatuses) ? validStatuses : [];
}