using FluentAssertions;
using order_management_api_assessment.Models;
using order_management_api_assessment.Services;

namespace order_management_api_assessment_tests.Tests;

public class DiscountServiceTests
{
    private readonly DiscountService _discountService = new();

    [Fact]
    public void CalculateBestDiscount_FirstTimeBuyer_Returns10Percent()
    {
        // Arrange
        var customer = TestHelpers.Customers.FirstTime();
        var orderItems = TestHelpers.Orders.RegularOrder(quantity: 1, price: 100m);

        // Act
        var result = _discountService.CalculateBestDiscount(customer, orderItems);

        // Assert
        result.Amount.Should().Be(10m);
        result.DiscountType.Should().Contain("First-time buyer");
    }

    [Fact]
    public void CalculateBestDiscount_BulkOrder_Returns15Percent()
    {
        // Arrange
        var customer = TestHelpers.Customers.Regular();
        var orderItems = TestHelpers.Orders.BulkOrder(quantity: 10, price: 50m);

        // Act
        var result = _discountService.CalculateBestDiscount(customer, orderItems);

        // Assert
        result.Amount.Should().Be(75m); // 15% of 500
        result.DiscountType.Should().Contain("Bulk order");
    }

    [Fact]
    public void CalculateBestDiscount_VIPCustomer_Returns20Percent()
    {
        // Arrange
        var customer = TestHelpers.Customers.VIP();
        var orderItems = TestHelpers.Orders.RegularOrder(quantity: 1, price: 100m);

        // Act
        var result = _discountService.CalculateBestDiscount(customer, orderItems);

        // Assert
        result.Amount.Should().Be(20m);
        result.DiscountType.Should().Contain("VIP customer");
    }

    [Fact]
    public void CalculateBestDiscount_MultipleDiscountsApply_ReturnsHighest()
    {
        // Arrange - VIP first-time buyer with bulk order
        var customer = TestHelpers.Customers.VIPFirstTime();
        var orderItems = TestHelpers.Orders.BulkOrder(quantity: 15, price: 40m); // 600 total

        // Act
        var result = _discountService.CalculateBestDiscount(customer, orderItems);

        // Assert
        // VIP (20%) = 120, Bulk (15%) = 90, First-time (10%) = 60
        result.Amount.Should().Be(120m); // Highest discount (VIP)
        result.DiscountType.Should().Contain("VIP customer");
    }

    [Fact]
    public void CalculateBestDiscount_NoApplicableDiscounts_ReturnsZero()
    {
        // Arrange
        var customer = TestHelpers.Customers.Regular(); // Not first-time, not VIP
        var orderItems = TestHelpers.Orders.RegularOrder(quantity: 5, price: 50m); // Not bulk

        // Act
        var result = _discountService.CalculateBestDiscount(customer, orderItems);

        // Assert
        result.Amount.Should().Be(0m);
        result.DiscountType.Should().BeEmpty();
    }

    [Fact]
    public void CalculateBestDiscount_EmptyOrderItems_ReturnsZero()
    {
        // Arrange
        var customer = TestHelpers.Customers.VIP();
        var orderItems = new List<Order.OrderItem>();

        // Act
        var result = _discountService.CalculateBestDiscount(customer, orderItems);

        // Assert
        result.Amount.Should().Be(0m);
        result.DiscountType.Should().BeEmpty();
    }

    [Theory]
    [InlineData(CustomerSegment.Regular, true, 1, 100, 10)] // First-time: 10%
    [InlineData(CustomerSegment.Regular, false, 10, 50, 75)] // Bulk: 15% of 500
    [InlineData(CustomerSegment.VIP, false, 1, 100, 20)] // VIP: 20%
    [InlineData(CustomerSegment.Premium, false, 1, 100, 0)] // No discount
    public void CalculateBestDiscount_VariousScenarios_ReturnsCorrectAmount(
        CustomerSegment segment,
        bool isFirstTime,
        int quantity,
        decimal unitPrice,
        decimal expectedDiscount)
    {
        // Arrange
        var customer = TestHelpers.CreateCustomer(segment, isFirstTime);
        var orderItems = new List<Order.OrderItem>
        {
            TestHelpers.CreateOrderItem(quantity: quantity, price: unitPrice)
        };

        // Act
        var result = _discountService.CalculateBestDiscount(customer, orderItems);

        // Assert
        result.Amount.Should().Be(expectedDiscount);
    }
}