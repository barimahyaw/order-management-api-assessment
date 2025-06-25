using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using order_management_api_assessment.Shared.Data;
using order_management_api_assessment.Shared.Enums;
using order_management_api_assessment.Shared.Models;

namespace order_management_api_assessment.Features.Orders.Query.GetOrderAnalytics;

internal sealed class GetOrderAnalyticsQueryHandler(
    OrderManagementDbContext dbContext,
    IMemoryCache cache,
    ILogger<GetOrderAnalyticsQueryHandler> logger)
    : IRequestHandler<GetOrderAnalyticsQuery, ApiResponse<OrderAnalyticsResponse>>
{
    private const string CACHE_KEY = "order_analytics";
    private readonly TimeSpan CACHE_DURATION = TimeSpan.FromMinutes(5);    

    public async Task<ApiResponse<OrderAnalyticsResponse>> Handle(GetOrderAnalyticsQuery request, CancellationToken cancellationToken)
    {
        // Try to get from cache first
        if (cache.TryGetValue(CACHE_KEY, out OrderAnalyticsResponse? cachedResult))
        {
            logger.LogInformation("Returning cached order analytics");
            return ApiResponse.Success(cachedResult!, "Order analytics retrieved from cache");
        }

        // If not in cache, calculate analytics
        logger.LogInformation("Cache miss - calculating fresh analytics");
        var orders = await dbContext.Orders.ToListAsync(cancellationToken);
        
        if (orders.Count == 0)
        {
            logger.LogInformation("No orders found for analytics calculation");
            return ApiResponse.Success(new OrderAnalyticsResponse(0, 0, 0, 0, 0, 0, 0, 0, 0), "Order analytics retrieved successfully");
        }

        var averageOrderValue = orders.Average(o => o.TotalAmount);
        
        var fulfilledOrders = orders.Where(o => o.FulfilledAt.HasValue).ToList();
        var averageFulfillmentTime = fulfilledOrders.Count != 0
            ? fulfilledOrders.Average(o => (o.FulfilledAt!.Value - o.CreatedAt).TotalHours)
            : 0;        
        
        var analytics = new OrderAnalyticsResponse(
            Math.Round(averageOrderValue, 2),
            Math.Round(averageFulfillmentTime, 2),
            orders.Count,
            orders.Count(o => o.Status == OrderStatus.Pending),
            orders.Count(o => o.Status == OrderStatus.Processing),
            orders.Count(o => o.Status == OrderStatus.Shipped),
            orders.Count(o => o.Status == OrderStatus.Delivered),
            orders.Count(o => o.Status == OrderStatus.Cancelled),
            orders.Count(o => o.Status == OrderStatus.Returned)
        );

        // Cache the result
        cache.Set(CACHE_KEY, analytics, CACHE_DURATION);
        logger.LogInformation("Order analytics calculated and cached for {CacheDurationMinutes} minutes", CACHE_DURATION.TotalMinutes);
        return ApiResponse.Success(analytics, "Order analytics retrieved successfully");
    }
}
