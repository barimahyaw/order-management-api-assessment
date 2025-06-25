using MediatR;
using order_management_api_assessment.Shared.Models;

namespace order_management_api_assessment.Features.Orders.Query.GetOrderAnalytics;

public record GetOrderAnalyticsQuery : IRequest<ApiResponse<OrderAnalyticsResponse>>;

public record OrderAnalyticsResponse(
    decimal AverageOrderValue,
    double AverageFulfillmentTimeHours,
    int TotalOrders,
    int PendingOrders,
    int ProcessingOrders,
    int ShippedOrders,
    int DeliveredOrders,
    int CancelledOrders,
    int ReturnedOrders
);
