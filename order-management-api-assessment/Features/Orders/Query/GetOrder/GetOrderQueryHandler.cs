using MediatR;
using Microsoft.EntityFrameworkCore;
using order_management_api_assessment.Shared.Data;
using order_management_api_assessment.Shared.Models;

namespace order_management_api_assessment.Features.Orders.Query.GetOrder;

internal sealed class GetOrderQueryHandler(
    OrderManagementDbContext dbContext,
    ILogger<GetOrderQueryHandler> logger)
    : IRequestHandler<GetOrderQuery, ApiResponse<OrderResponse>>
{
    public async Task<ApiResponse<OrderResponse>> Handle(GetOrderQuery request, CancellationToken cancellationToken)
    {
        var order = await dbContext.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.OrderId == request.OrderId, cancellationToken);
        if (order == null)
        {
            logger.LogWarning("Order with ID {OrderId} not found", request.OrderId);
            return ApiResponse.Error<OrderResponse>("Order not found");
        }

        var orderResponse = new OrderResponse(
            order.OrderId,
            order.CustomerId,
            order.TotalAmount,
            order.DiscountedAmount,
            order.Status.ToString(),
            order.CreatedAt,
            order.FulfilledAt,
            [.. order.OrderItems.Select(oi => new OrderItemResponse(
                oi.Id,
                oi.ProductId,
                oi.Quantity,
                oi.Price
            ))]
        );

        logger.LogInformation("Order {OrderId} retrieved successfully", request.OrderId);
        return ApiResponse.Success(orderResponse, "Order retrieved successfully");
    }
}
