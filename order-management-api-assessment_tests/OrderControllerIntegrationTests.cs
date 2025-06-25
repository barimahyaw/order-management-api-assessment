using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using order_management_api_assessment.Controllers;
using order_management_api_assessment.Features.Orders.Command.Create;
using order_management_api_assessment.Features.Orders.Query.GetOrder;
using order_management_api_assessment.Features.Orders.Query.GetOrderAnalytics;
using order_management_api_assessment.Features.Orders.Query.GetOrders;
using order_management_api_assessment.Shared.Data;
using order_management_api_assessment.Shared.Enums;
using order_management_api_assessment.Shared.Models;

namespace order_management_api_assessment_tests
{
    public class OrderControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _jsonOptions;

        public OrderControllerIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Ensure the database is created and seeded for tests
                    var serviceProvider = services.BuildServiceProvider();
                    using var scope = serviceProvider.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<OrderManagementDbContext>();
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
        public async Task CreateOrder_ValidRequest_ReturnsCreatedResult()
        {
            // Arrange
            var request = new CreateOrderRequest(
                CustomerId: new Guid("2585a176-1e69-4d3c-b174-9da5f5521505"), // Existing customer from seed data
                OrderItems: new List<OrderItemDto>
                {
                    new(Guid.NewGuid(), 2, 50.00m),
                    new(Guid.NewGuid(), 1, 100.00m)
                }
            );

            // Act
            var response = await _client.PostAsJsonAsync("/api/orders", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            
            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(content, _jsonOptions);
            
            apiResponse.Should().NotBeNull();
            apiResponse!.Success.Should().BeTrue();
            apiResponse.Message.Should().Contain("Order created successfully");
        }

        [Fact]
        public async Task CreateOrder_InvalidCustomerId_ReturnsBadRequest()
        {
            // Arrange
            var request = new CreateOrderRequest(
                CustomerId: Guid.Empty,
                OrderItems: new List<OrderItemDto>
                {
                    new(Guid.NewGuid(), 1, 50.00m)
                }
            );

            // Act
            var response = await _client.PostAsJsonAsync("/api/orders", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateOrder_EmptyOrderItems_ReturnsBadRequest()
        {
            // Arrange
            var request = new CreateOrderRequest(
                CustomerId: new Guid("2585a176-1e69-4d3c-b174-9da5f5521505"),
                OrderItems: new List<OrderItemDto>()
            );

            // Act
            var response = await _client.PostAsJsonAsync("/api/orders", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateOrder_FirstTimeBuyerDiscount_AppliesCorrectDiscount()
        {
            // Arrange - First-time buyer customer
            var request = new CreateOrderRequest(
                CustomerId: new Guid("d4a8c2e7-3b5f-4e1d-9c7a-6f2b8e4a7c9d"), // First-time buyer from seed data
                OrderItems: new List<OrderItemDto>
                {
                    new(Guid.NewGuid(), 1, 100.00m)
                }
            );

            // Act
            var response = await _client.PostAsJsonAsync("/api/orders", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            
            // Verify discount was applied in the response
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("10% discount applied");
        }

        [Fact]
        public async Task CreateOrder_VipCustomerDiscount_AppliesCorrectDiscount()
        {
            // Arrange - VIP customer
            var request = new CreateOrderRequest(
                CustomerId: new Guid("c7b3f8e2-9a4d-4c5e-b8f1-3e7a9b2c4d5f"), // VIP customer from seed data
                OrderItems: new List<OrderItemDto>
                {
                    new(Guid.NewGuid(), 1, 200.00m)
                }
            );

            // Act
            var response = await _client.PostAsJsonAsync("/api/orders", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            
            // Verify VIP discount was applied
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("20% discount applied");
        }

        [Fact]
        public async Task CreateOrder_BulkOrderDiscount_AppliesCorrectDiscount()
        {
            // Arrange - Bulk order (10+ items)
            var request = new CreateOrderRequest(
                CustomerId: new Guid("2585a176-1e69-4d3c-b174-9da5f5521505"),
                OrderItems: new List<OrderItemDto>
                {
                    new(Guid.NewGuid(), 15, 20.00m) // 15 items
                }
            );

            // Act
            var response = await _client.PostAsJsonAsync("/api/orders", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            
            // Verify bulk discount was applied
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("15% discount applied");
        }

        [Fact]
        public async Task GetOrder_ExistingOrder_ReturnsOrderDetails()
        {
            // Arrange - Use seeded order
            var orderId = new Guid("b8f2d4a7-3c5e-4b9f-a1d8-7e4c2f9b6a3d");

            // Act
            var response = await _client.GetAsync($"/api/orders/{orderId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<OrderResponse>>(content, _jsonOptions);
            
            apiResponse.Should().NotBeNull();
            apiResponse!.Success.Should().BeTrue();
            apiResponse.Data.Should().NotBeNull();
            apiResponse.Data!.OrderId.Should().Be(orderId);
            apiResponse.Data.Status.Should().Be("Delivered");
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
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<OrdersPagedResponse>>(content, _jsonOptions);
            
            apiResponse.Should().NotBeNull();
            apiResponse!.Success.Should().BeTrue();
            apiResponse.Data.Should().NotBeNull();
            apiResponse.Data!.Orders.Should().NotBeEmpty();
            apiResponse.Data.TotalCount.Should().BeGreaterThan(0);
            apiResponse.Data.Page.Should().Be(1);
            apiResponse.Data.PageSize.Should().Be(10);
        }

        [Fact]
        public async Task GetOrders_WithStatusFilter_ReturnsFilteredResults()
        {
            // Act
            var response = await _client.GetAsync("/api/orders?status=Delivered");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<OrdersPagedResponse>>(content, _jsonOptions);
            
            apiResponse.Should().NotBeNull();
            apiResponse!.Data.Should().NotBeNull();
            apiResponse.Data!.Orders.Should().OnlyContain(o => o.Status == "Delivered");
        }

        [Fact]
        public async Task GetOrders_WithPagination_ReturnsCorrectPage()
        {
            // Act
            var response = await _client.GetAsync("/api/orders?page=1&pageSize=2");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<OrdersPagedResponse>>(content, _jsonOptions);
            
            apiResponse.Should().NotBeNull();
            apiResponse!.Data.Should().NotBeNull();
            apiResponse.Data!.Orders.Should().HaveCountLessOrEqualTo(2);
            apiResponse.Data.Page.Should().Be(1);
            apiResponse.Data.PageSize.Should().Be(2);
        }

        [Fact]
        public async Task GetOrderAnalytics_ReturnsAnalyticsData()
        {
            // Act
            var response = await _client.GetAsync("/api/orders/analytics");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<OrderAnalyticsResponse>>(content, _jsonOptions);
            
            apiResponse.Should().NotBeNull();
            apiResponse!.Success.Should().BeTrue();
            apiResponse.Data.Should().NotBeNull();
            apiResponse.Data!.TotalOrders.Should().BeGreaterThan(0);
            apiResponse.Data.AverageOrderValue.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task UpdateOrderStatus_ValidTransition_ReturnsSuccess()
        {
            // Arrange - Create a new order first
            var createRequest = new CreateOrderRequest(
                CustomerId: new Guid("2585a176-1e69-4d3c-b174-9da5f5521505"),
                OrderItems: new List<OrderItemDto>
                {
                    new(Guid.NewGuid(), 1, 50.00m)
                }
            );

            var createResponse = await _client.PostAsJsonAsync("/api/orders", createRequest);
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            // Extract order ID from response
            var createContent = await createResponse.Content.ReadAsStringAsync();
            var createApiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(createContent, _jsonOptions);
            var orderId = ExtractOrderIdFromMessage(createApiResponse!.Message!);

            var updateRequest = new UpdateOrderStatusRequest(OrderStatus.Processing);

            // Act
            var response = await _client.PutAsJsonAsync($"/api/orders/{orderId}/status", updateRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(content, _jsonOptions);
            
            apiResponse.Should().NotBeNull();
            apiResponse!.Success.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateOrderStatus_InvalidTransition_ReturnsBadRequest()
        {
            // Arrange - Use existing pending order
            var orderId = new Guid("f1b6c9e4-2d7a-4e8f-c3b9-5f1d8a6e2c7b"); // Pending order from seed data
            var updateRequest = new UpdateOrderStatusRequest(OrderStatus.Delivered); // Invalid transition

            // Act
            var response = await _client.PutAsJsonAsync($"/api/orders/{orderId}/status", updateRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpdateOrderStatus_NonExistentOrder_ReturnsBadRequest()
        {
            // Arrange
            var nonExistentOrderId = Guid.NewGuid();
            var updateRequest = new UpdateOrderStatusRequest(OrderStatus.Processing);

            // Act
            var response = await _client.PutAsJsonAsync($"/api/orders/{nonExistentOrderId}/status", updateRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Theory]
        [InlineData("")]
        [InlineData("InvalidStatus")]
        [InlineData("999")]
        public async Task GetOrders_InvalidStatusFilter_ReturnsAllOrders(string statusFilter)
        {
            // Act
            var response = await _client.GetAsync($"/api/orders?status={statusFilter}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<OrdersPagedResponse>>(content, _jsonOptions);
            
            apiResponse.Should().NotBeNull();
            apiResponse!.Data.Should().NotBeNull();
        }

        [Theory]
        [InlineData(0, 10)] // Invalid page
        [InlineData(-1, 10)] // Negative page
        [InlineData(1, 0)] // Invalid page size
        [InlineData(1, -1)] // Negative page size
        public async Task GetOrders_InvalidPaginationParameters_HandlesGracefully(int page, int pageSize)
        {
            // Act
            var response = await _client.GetAsync($"/api/orders?page={page}&pageSize={pageSize}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<OrdersPagedResponse>>(content, _jsonOptions);
            
            apiResponse.Should().NotBeNull();
            apiResponse!.Data.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateOrder_DatabasePersistence_OrderIsStored()
        {
            // Arrange
            var request = new CreateOrderRequest(
                CustomerId: new Guid("2585a176-1e69-4d3c-b174-9da5f5521505"),
                OrderItems: new List<OrderItemDto>
                {
                    new(Guid.NewGuid(), 1, 75.00m)
                }
            );

            // Act
            var response = await _client.PostAsJsonAsync("/api/orders", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            // Verify order was persisted by querying the database
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<OrderManagementDbContext>();
            
            var orders = await dbContext.Orders
                .Where(o => o.CustomerId == request.CustomerId)
                .Include(o => o.OrderItems)
                .ToListAsync();
            
            orders.Should().NotBeEmpty();
            var latestOrder = orders.OrderByDescending(o => o.CreatedAt).First();
            latestOrder.OrderItems.Should().HaveCount(1);
            latestOrder.OrderItems.First().Price.Should().Be(75.00m);
        }

        [Fact]
        public async Task FullOrderLifecycle_IntegrationTest_WorksEndToEnd()
        {
            // Arrange & Act 1: Create Order
            var createRequest = new CreateOrderRequest(
                CustomerId: new Guid("2585a176-1e69-4d3c-b174-9da5f5521505"),
                OrderItems: new List<OrderItemDto>
                {
                    new(Guid.NewGuid(), 2, 100.00m)
                }
            );

            var createResponse = await _client.PostAsJsonAsync("/api/orders", createRequest);
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            var createContent = await createResponse.Content.ReadAsStringAsync();
            var createApiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(createContent, _jsonOptions);
            var orderId = ExtractOrderIdFromMessage(createApiResponse!.Message!);

            // Act 2: Get the created order
            var getResponse = await _client.GetAsync($"/api/orders/{orderId}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Act 3: Update order status to Processing
            var updateToProcessingRequest = new UpdateOrderStatusRequest(OrderStatus.Processing);
            var updateToProcessingResponse = await _client.PutAsJsonAsync($"/api/orders/{orderId}/status", updateToProcessingRequest);
            updateToProcessingResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Act 4: Update order status to Shipped
            var updateToShippedRequest = new UpdateOrderStatusRequest(OrderStatus.Shipped);
            var updateToShippedResponse = await _client.PutAsJsonAsync($"/api/orders/{orderId}/status", updateToShippedRequest);
            updateToShippedResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Act 5: Update order status to Delivered
            var updateToDeliveredRequest = new UpdateOrderStatusRequest(OrderStatus.Delivered);
            var updateToDeliveredResponse = await _client.PutAsJsonAsync($"/api/orders/{orderId}/status", updateToDeliveredRequest);
            updateToDeliveredResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Act 6: Verify final state
            var finalGetResponse = await _client.GetAsync($"/api/orders/{orderId}");
            finalGetResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var finalContent = await finalGetResponse.Content.ReadAsStringAsync();
            var finalApiResponse = JsonSerializer.Deserialize<ApiResponse<OrderResponse>>(finalContent, _jsonOptions);

            // Assert
            finalApiResponse.Should().NotBeNull();
            finalApiResponse!.Data.Should().NotBeNull();
            finalApiResponse.Data!.Status.Should().Be("Delivered");
            finalApiResponse.Data.FulfilledAt.Should().NotBeNull();
        }

        private static Guid ExtractOrderIdFromMessage(string message)
        {
            // Extract order ID from success message
            // Expected format: "Order created successfully with ID: {guid}"
            var parts = message.Split(": ");
            if (parts.Length > 1 && Guid.TryParse(parts[1], out var orderId))
            {
                return orderId;
            }
            throw new InvalidOperationException($"Could not extract order ID from message: {message}");
        }
    }
}