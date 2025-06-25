namespace order_management_api_assessment.Shared.Enums;

/// <summary>
/// Extension methods for the OrderStatus enum to handle valid transitions.
/// </summary>
public static class OrderStatusExtensions
{
    /// <summary>
    /// Dictionary to hold valid transitions for each order status.
    /// </summary>
    private static readonly Dictionary<OrderStatus, List<OrderStatus>> _validTransaitions = new()
    {
        { OrderStatus.Pending, new List<OrderStatus> { OrderStatus.Processing, OrderStatus.Cancelled } },
        { OrderStatus.Processing, new List<OrderStatus> { OrderStatus.Shipped, OrderStatus.Cancelled } },
        { OrderStatus.Shipped, new List<OrderStatus> { OrderStatus.Delivered, OrderStatus.Returned } },
        { OrderStatus.Delivered, new List<OrderStatus> { } },
        { OrderStatus.Cancelled, new List<OrderStatus> { } },
        { OrderStatus.Returned, new List<OrderStatus> { } }
    };

    /// <summary>
    /// Checks if a transition from the current status to the new status is valid.
    /// </summary>
    /// <param name="currentStatus"> Current order status.</param>
    /// <param name="newStatus"> New order status to transition to.</param>
    /// <returns> True if the transition is valid, otherwise false.</returns>
    public static bool CanTransitionTo(this OrderStatus currentStatus, OrderStatus newStatus)
        => _validTransaitions.TryGetValue(currentStatus, out var validStatuses) && validStatuses.Contains(newStatus);

    /// <summary>
    /// Gets a list of valid transitions from the current status.
    /// </summary>
    /// <param name="currentStatus"> Current order status.</param>
    /// <returns> List of valid statuses that can be transitioned to from the current status.</returns>
    public static List<OrderStatus> GetValidTransitions(this OrderStatus currentStatus) 
        => _validTransaitions.TryGetValue(currentStatus, out var validStatuses) ? validStatuses : new List<OrderStatus>();
}
