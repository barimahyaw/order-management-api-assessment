namespace order_management_api_assessment.Shared.Enums;

/// <summary>
/// Enumeration representing the various statuses an order can have.
/// </summary>
public enum OrderStatus
{
    /// <summary>
    /// Order is pending and has not yet been processed.
    /// </summary>
    Pending = 1,
    /// <summary>
    /// Order is currently being processed.
    /// </summary>
    Processing = 2,
    /// <summary>
    /// Order has been shipped to the customer.
    /// </summary>
    Shipped = 3,
    /// <summary>
    /// Order has been delivered to the customer.
    /// </summary>
    Delivered = 4,
    /// <summary>
    /// Order has been cancelled and will not be processed further.
    /// </summary>
    Cancelled = 5,
    /// <summary>
    /// Order has been returned by the customer and is being processed for return or refund.
    /// </summary>
    Returned = 6
}
