using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using order_management_api_assessment.Data;
using order_management_api_assessment.DTOs;
using order_management_api_assessment.Models;

namespace order_management_api_assessment_tests.Tests;

public class OrdersControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public OrdersControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var serviceProvider = services.BuildServiceProvider();
                using var scope = serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<OrderContext>();
                dbContext.Database.EnsureCreated();
            });
        });
        _client = _factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    [Fact]
    public async Task CreateOrder_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var request = TestHelpers.CreateOrderRequest(
            new Guid("2585a176-1e69-4d3c-b174-9da5f5521505"), // Seeded customer
            TestHelpers.CreateOrderItemDto(price: 100m));

        // Act
        var response = await _client.PostAsJsonAsync("/api/orders", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Order created successfully");
    }

    [Fact]
    public async Task CreateOrder_InvalidCustomerId_ReturnsBadRequest()
    {
        // Arrange
        var request = TestHelpers.CreateOrderRequest(Guid.NewGuid()); // Non-existent customer

        // Act
        var response = await _client.PostAsJsonAsync("/api/orders", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateOrder_VIPCustomer_AppliesDiscount()
    {
        // Arrange
        var request = TestHelpers.CreateOrderRequest(
            new Guid("c7b3f8e2-9a4d-4c5e-b8f1-3e7a9b2c4d5f"), // VIP customer from seed
            TestHelpers.CreateOrderItemDto(price: 200m));

        // Act
        var response = await _client.PostAsJsonAsync("/api/orders", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("20% discount applied");
    }

    [Fact]
    public async Task GetOrder_ExistingOrder_ReturnsOrder()
    {
        // Arrange - Use seeded order
        var orderId = new Guid("b8f2d4a7-3c5e-4b9f-a1d8-7e4c2f9b6a3d");

        // Act
        var response = await _client.GetAsync($"/api/orders/{orderId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var order = JsonSerializer.Deserialize<OrderResponse>(content, _jsonOptions);
        order!.OrderId.Should().Be(orderId);
    }

    [Fact]
    public async Task GetOrder_NonExistentOrder_ReturnsNotFound()
    {
        // Arrange
        var nonExistentOrderId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/orders/{nonExistentOrderId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetOrders_DefaultPagination_ReturnsPagedResults()
    {
        // Act
        var response = await _client.GetAsync("/api/orders");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<OrdersPagedResponse>(content, _jsonOptions);
        
        result.Should().NotBeNull();
        result!.Orders.Should().NotBeEmpty();
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task GetOrders_WithStatusFilter_ReturnsFilteredResults()
    {
        // Act
        var response = await _client.GetAsync("/api/orders?status=Delivered");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<OrdersPagedResponse>(content, _jsonOptions);
        
        result!.Orders.Should().OnlyContain(o => o.Status == "Delivered");
    }

    [Fact]
    public async Task UpdateOrderStatus_ValidTransition_ReturnsSuccess()
    {
        // Arrange - Create a new order first
        var createRequest = TestHelpers.CreateOrderRequest(
            new Guid("2585a176-1e69-4d3c-b174-9da5f5521505"),
            TestHelpers.CreateOrderItemDto());

        var createResponse = await _client.PostAsJsonAsync("/api/orders", createRequest);
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var orderId = ExtractOrderIdFromResponse(createContent);

        var updateRequest = new UpdateOrderStatusRequest(OrderStatus.Processing);

        // Act
        var response = await _client.PutAsJsonAsync($"/api/orders/{orderId}/status", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Processing");
    }

    [Fact]
    public async Task GetOrderAnalytics_ReturnsAnalyticsData()
    {
        // Act
        var response = await _client.GetAsync("/api/orders/analytics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var analytics = JsonSerializer.Deserialize<OrderAnalyticsResponse>(content, _jsonOptions);
        
        analytics.Should().NotBeNull();
        analytics!.TotalOrders.Should().BeGreaterThan(0);
        analytics.OrdersByStatus.Should().NotBeEmpty();
    }

    private static Guid ExtractOrderIdFromResponse(string content)
    {
        // Simple extraction - in production, would deserialize properly
        var parts = content.Split(": ");
        if (parts.Length > 1)
        {
            var guidPart = parts[1].Split('"')[0];
            if (Guid.TryParse(guidPart, out var orderId))
                return orderId;
        }
        throw new InvalidOperationException($"Could not extract order ID from: {content}");
    }
}