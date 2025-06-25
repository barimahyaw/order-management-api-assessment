using MediatR;
using Microsoft.EntityFrameworkCore;
using order_management_api_assessment.Shared.Data;
using order_management_api_assessment.Shared.Enums;
using order_management_api_assessment.Shared.Models;

namespace order_management_api_assessment.Features.Orders.Query.GetOrders;

internal sealed class GetOrdersQueryHandler(
    OrderManagementDbContext dbContext,
    ILogger<GetOrdersQueryHandler> logger)
    : IRequestHandler<GetOrdersQuery, ApiResponse<OrdersPagedResponse>>
{
    public async Task<ApiResponse<OrdersPagedResponse>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var query = dbContext.Orders.Include(o => o.OrderItems).AsQueryable();

        // Apply status filter if provided
        if (!string.IsNullOrEmpty(request.Status) && Enum.TryParse<OrderStatus>(request.Status, true, out var statusFilter))
        {
            query = query.Where(o => o.Status == statusFilter);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);        
        
        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(o => new OrderSummaryResponse(
                o.OrderId,
                o.CustomerId,
                o.TotalAmount,
                o.DiscountedAmount,
                o.Status.ToString(),
                o.CreatedAt,
                o.FulfilledAt,
                o.OrderItems.Count
            ))
            .ToListAsync(cancellationToken);        
        
        var response = new OrdersPagedResponse(
            orders,
            totalCount,
            request.Page,
            request.PageSize,
            totalPages
        );

        logger.LogInformation("Retrieved {Count} orders (page {Page} of {TotalPages})", 
            orders.Count, request.Page, totalPages);

        return ApiResponse.Success(response, "Orders retrieved successfully");
    }
}
