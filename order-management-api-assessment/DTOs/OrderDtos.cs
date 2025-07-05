using System.ComponentModel.DataAnnotations;
using order_management_api_assessment.Models;

namespace order_management_api_assessment.DTOs;

// Request DTOs
public record CreateOrderRequest(
    [Required] Guid CustomerId,
    [Required, MinLength(1)] List<OrderItemDto> OrderItems);

public record UpdateOrderStatusRequest([Required] OrderStatus NewStatus);

public record OrderItemDto(
    [Required] Guid ProductId,
    [Required, Range(1, int.MaxValue)] int Quantity,
    [Required, Range(0.01, double.MaxValue)] decimal UnitPrice);

// Response DTOs
public record OrderResponse(
    Guid OrderId,
    Guid CustomerId,
    decimal TotalAmount,
    decimal DiscountedAmount,
    string Status,
    DateTime CreatedAt,
    DateTime? FulfilledAt,
    List<OrderItemResponse> OrderItems);

public record OrderItemResponse(
    Guid Id,
    Guid ProductId,
    int Quantity,
    decimal Price);

public record OrderSummaryResponse(
    Guid OrderId,
    Guid CustomerId,
    decimal TotalAmount,
    decimal DiscountedAmount,
    string Status,
    DateTime CreatedAt,
    DateTime? FulfilledAt);

public record OrdersPagedResponse(
    List<OrderSummaryResponse> Orders,
    int TotalCount,
    int Page,
    int PageSize);

public record OrderAnalyticsResponse(
    int TotalOrders,
    decimal TotalRevenue,
    decimal AverageOrderValue,
    Dictionary<string, int> OrdersByStatus);