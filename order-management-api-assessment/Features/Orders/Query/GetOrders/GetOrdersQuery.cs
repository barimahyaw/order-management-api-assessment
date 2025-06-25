using MediatR;
using order_management_api_assessment.Shared.Models;

namespace order_management_api_assessment.Features.Orders.Query.GetOrders;

public record GetOrdersQuery(int Page = 1, int PageSize = 10, string? Status = null) : IRequest<ApiResponse<OrdersPagedResponse>>;

public record OrdersPagedResponse(
    List<OrderSummaryResponse> Orders,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);

public record OrderSummaryResponse(
    Guid OrderId,
    Guid CustomerId,
    decimal TotalAmount,
    decimal DiscountedAmount,
    string Status,
    DateTime CreatedAt,
    DateTime? FulfilledAt,
    int ItemCount
);
