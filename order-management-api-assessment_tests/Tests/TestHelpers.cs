using order_management_api_assessment.DTOs;
using order_management_api_assessment.Models;

namespace order_management_api_assessment_tests.Tests;

public static class TestHelpers
{
    public static Customer CreateCustomer(
        CustomerSegment segment = CustomerSegment.Regular,
        bool isFirstTime = false,
        string name = "Test Customer",
        string email = "test@example.com")
    {
        return new Customer
        {
            Id = Guid.NewGuid(),
            Name = name,
            Email = email,
            Segment = segment,
            IsFirstTime = isFirstTime,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static Order.OrderItem CreateOrderItem(
        Guid? orderId = null,
        Guid? productId = null,
        int quantity = 1,
        decimal price = 100m)
    {
        return Order.OrderItem.Create(
            orderId ?? Guid.NewGuid(),
            productId ?? Guid.NewGuid(),
            quantity,
            price);
    }

    public static OrderItemDto CreateOrderItemDto(
        Guid? productId = null,
        int quantity = 1,
        decimal price = 100m)
    {
        return new OrderItemDto(
            productId ?? Guid.NewGuid(),
            quantity,
            price);
    }

    public static CreateOrderRequest CreateOrderRequest(
        Guid? customerId = null,
        params OrderItemDto[] items)
    {
        return new CreateOrderRequest(
            customerId ?? Guid.NewGuid(),
            items.Any() ? items.ToList() : [CreateOrderItemDto()]);
    }

    public static class Customers
    {
        public static Customer Regular() => CreateCustomer(CustomerSegment.Regular);
        public static Customer VIP() => CreateCustomer(CustomerSegment.VIP);
        public static Customer FirstTime() => CreateCustomer(isFirstTime: true);
        public static Customer VIPFirstTime() => CreateCustomer(CustomerSegment.VIP, isFirstTime: true);
    }

    public static class Orders
    {
        public static List<Order.OrderItem> BulkOrder(int quantity = 10, decimal price = 50m)
        {
            return [CreateOrderItem(quantity: quantity, price: price)];
        }

        public static List<Order.OrderItem> RegularOrder(int quantity = 1, decimal price = 100m)
        {
            return [CreateOrderItem(quantity: quantity, price: price)];
        }
    }
}