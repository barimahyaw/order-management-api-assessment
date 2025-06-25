using MediatR;
using order_management_api_assessment.Shared.Models;

namespace order_management_api_assessment.Features.Orders.Query.GetOrder;

public record GetOrderQuery(Guid OrderId) : IRequest<ApiResponse<OrderResponse>>;

public record OrderResponse(
    Guid OrderId,
    Guid CustomerId,
    decimal TotalAmount,
    decimal DiscountedAmount,
    string Status,
    DateTime CreatedAt,
    DateTime? FulfilledAt,
    List<OrderItemResponse> OrderItems
);

public record OrderItemResponse(
    Guid Id,
    Guid ProductId,
    int Quantity,
    decimal Price
);
