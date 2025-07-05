using Microsoft.EntityFrameworkCore;
using order_management_api_assessment.Data;
using order_management_api_assessment.DTOs;
using order_management_api_assessment.Models;

namespace order_management_api_assessment.Services;

public class OrderService(OrderContext context, DiscountService discountService, ILogger<OrderService> logger)
{
    public async Task<(bool Success, string Message, Guid? OrderId)> CreateOrderAsync(CreateOrderRequest request)
    {
        var customer = await context.Customers.FirstOrDefaultAsync(c => c.Id == request.CustomerId);
        if (customer == null)
        {
            logger.LogWarning("Customer with ID {CustomerId} not found", request.CustomerId);
            return (false, "Customer not found", null);
        }

        var order = Order.Create(customer.Id);
        order.AddOrderItems(request.OrderItems);

        var discountResult = discountService.CalculateBestDiscount(customer, [.. order.OrderItems]);
        order.ApplyDiscount(discountResult.Amount);

        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var message = discountResult.Amount > 0 && !string.IsNullOrEmpty(discountResult.DiscountType)
            ? $"Order created successfully with ID: {order.OrderId} with {discountResult.DiscountType}"
            : $"Order created successfully with ID: {order.OrderId}";

        logger.LogInformation("Order {OrderId} created for customer {CustomerId} with discount {DiscountAmount}",
            order.OrderId, customer.Id, discountResult.Amount);

        return (true, message, order.OrderId);
    }

    public async Task<(bool Success, string Message)> UpdateOrderStatusAsync(Guid orderId, OrderStatus newStatus)
    {
        var order = await context.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
        if (order == null)
        {
            return (false, "Order not found");
        }

        try
        {
            order.UpdateStatus(newStatus);
            await context.SaveChangesAsync();

            logger.LogInformation("Order {OrderId} status updated to {Status}", orderId, newStatus);
            return (true, $"Order status updated to {newStatus}");
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning("Invalid status transition for order {OrderId}: {Message}", orderId, ex.Message);
            return (false, ex.Message);
        }
    }

    public async Task<OrderResponse?> GetOrderAsync(Guid orderId)
    {
        var order = await context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);

        if (order == null) return null;

        return new OrderResponse(
            order.OrderId,
            order.CustomerId,
            order.TotalAmount,
            order.DiscountedAmount,
            order.Status.ToString(),
            order.CreatedAt,
            order.FulfilledAt,
            order.OrderItems.Select(oi => new OrderItemResponse(oi.Id, oi.ProductId, oi.Quantity, oi.Price)).ToList()
        );
    }

    public async Task<OrdersPagedResponse> GetOrdersAsync(int page, int pageSize, string? status)
    {
        page = Math.Max(1, page);
        pageSize = Math.Max(1, Math.Min(100, pageSize));

        var query = context.Orders.AsQueryable();

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<OrderStatus>(status, true, out var orderStatus))
        {
            query = query.Where(o => o.Status == orderStatus);
        }

        var totalCount = await query.CountAsync();
        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new OrderSummaryResponse(
                o.OrderId,
                o.CustomerId,
                o.TotalAmount,
                o.DiscountedAmount,
                o.Status.ToString(),
                o.CreatedAt,
                o.FulfilledAt))
            .ToListAsync();

        return new OrdersPagedResponse(orders, totalCount, page, pageSize);
    }

    public async Task<OrderAnalyticsResponse> GetOrderAnalyticsAsync()
    {
        var orders = await context.Orders.ToListAsync();

        var totalOrders = orders.Count;
        var totalRevenue = orders.Sum(o => o.DiscountedAmount);
        var averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;
        var ordersByStatus = orders
            .GroupBy(o => o.Status.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        return new OrderAnalyticsResponse(totalOrders, totalRevenue, averageOrderValue, ordersByStatus);
    }
}