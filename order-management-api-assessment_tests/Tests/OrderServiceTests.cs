using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using order_management_api_assessment.Data;
using order_management_api_assessment.DTOs;
using order_management_api_assessment.Models;
using order_management_api_assessment.Services;

namespace order_management_api_assessment_tests.Tests;

public class OrderServiceTests : IDisposable
{
    private readonly OrderContext _context;
    private readonly OrderService _orderService;
    private readonly Mock<ILogger<OrderService>> _mockLogger;

    public OrderServiceTests()
    {
        var options = new DbContextOptionsBuilder<OrderContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new OrderContext(options);
        _mockLogger = new Mock<ILogger<OrderService>>();
        var discountService = new DiscountService();
        _orderService = new OrderService(_context, discountService, _mockLogger.Object);

        // Seed test customer
        _context.Customers.Add(TestHelpers.Customers.Regular());
        _context.SaveChanges();
    }

    [Fact]
    public async Task CreateOrderAsync_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var customer = TestHelpers.Customers.Regular();
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var request = TestHelpers.CreateOrderRequest(customer.Id, TestHelpers.CreateOrderItemDto(price: 100m));

        // Act
        var result = await _orderService.CreateOrderAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.OrderId.Should().NotBeNull();
        result.Message.Should().Contain("Order created successfully");
    }

    [Fact]
    public async Task CreateOrderAsync_CustomerNotFound_ReturnsFailure()
    {
        // Arrange
        var request = TestHelpers.CreateOrderRequest(Guid.NewGuid());

        // Act
        var result = await _orderService.CreateOrderAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Customer not found");
        result.OrderId.Should().BeNull();
    }

    [Fact]
    public async Task CreateOrderAsync_VIPCustomer_AppliesDiscount()
    {
        // Arrange
        var customer = TestHelpers.Customers.VIP();
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var request = TestHelpers.CreateOrderRequest(customer.Id, TestHelpers.CreateOrderItemDto(price: 100m));

        // Act
        var result = await _orderService.CreateOrderAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("20% discount applied (VIP customer)");
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_ValidTransition_ReturnsSuccess()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid());
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Act
        var result = await _orderService.UpdateOrderStatusAsync(order.OrderId, OrderStatus.Processing);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("Processing");
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_InvalidTransition_ReturnsFailure()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid());
        order.UpdateStatus(OrderStatus.Processing);
        order.UpdateStatus(OrderStatus.Shipped);
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Act
        var result = await _orderService.UpdateOrderStatusAsync(order.OrderId, OrderStatus.Pending);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Cannot transition");
    }

    [Fact]
    public async Task GetOrderAsync_ExistingOrder_ReturnsOrder()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid());
        order.AddOrderItems([TestHelpers.CreateOrderItemDto()]);
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Act
        var result = await _orderService.GetOrderAsync(order.OrderId);

        // Assert
        result.Should().NotBeNull();
        result!.OrderId.Should().Be(order.OrderId);
        result.OrderItems.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetOrdersAsync_WithStatusFilter_ReturnsFilteredOrders()
    {
        // Arrange
        var order1 = Order.Create(Guid.NewGuid());
        var order2 = Order.Create(Guid.NewGuid());
        order2.UpdateStatus(OrderStatus.Processing);

        _context.Orders.AddRange(order1, order2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _orderService.GetOrdersAsync(1, 10, "Processing");

        // Assert
        result.Orders.Should().HaveCount(1);
        result.Orders.First().Status.Should().Be("Processing");
    }

    [Fact]
    public async Task GetOrderAnalyticsAsync_ReturnsCorrectAnalytics()
    {
        // Arrange
        var order1 = Order.Create(Guid.NewGuid());
        var order2 = Order.Create(Guid.NewGuid());
        order2.UpdateStatus(OrderStatus.Processing);

        _context.Orders.AddRange(order1, order2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _orderService.GetOrderAnalyticsAsync();

        // Assert
        result.TotalOrders.Should().Be(2);
        result.OrdersByStatus.Should().ContainKey("Pending");
        result.OrdersByStatus.Should().ContainKey("Processing");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}