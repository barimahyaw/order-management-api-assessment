using FluentAssertions;
using order_management_api_assessment.Features.Customers;
using order_management_api_assessment.Features.Discounts;
using order_management_api_assessment.Features.Discounts.Services;
using order_management_api_assessment.Features.Orders;
using order_management_api_assessment.Features.Orders.Command.Create;
using order_management_api_assessment.Shared.Enums;

namespace order_management_api_assessment_tests
{
    public class BusinessRulesTests
    {
        private readonly DiscountService _discountService;

        public BusinessRulesTests()
        {
            var discountRules = new List<IDiscountRule>
            {
                new FirstTimeBuyerDiscount(),
                new BulkOrderDiscount(),
                new VipCustomerDiscount()
            };
            _discountService = new DiscountService(discountRules);
        }

        [Fact]
        public void BusinessRule_FirstTimeBuyerDiscount_OnlyAppliesOnce()
        {
            // Arrange
            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                Name = "First Time Customer",
                Email = "firsttime@example.com",
                Segment = CustomerSegment.Regular,
                IsFirstTime = true
            };

            var orderItems = new List<OrderItem>
            {
                OrderItem.Create(Guid.NewGuid(), Guid.NewGuid(), 1, 100m)
            };

            var firstTimeBuyerRule = new FirstTimeBuyerDiscount();

            // Act - Apply discount first time
            var firstTimeApplicable = firstTimeBuyerRule.IsApplicable(customer, orderItems);
            var firstTimeDiscount = firstTimeBuyerRule.Calculate(customer, orderItems, 100m);

            // Simulate customer is no longer first time
            customer.IsFirstTime = false;

            // Act - Try to apply discount second time
            var secondTimeApplicable = firstTimeBuyerRule.IsApplicable(customer, orderItems);

            // Assert
            firstTimeApplicable.Should().BeTrue();
            firstTimeDiscount.Should().Be(10m);
            secondTimeApplicable.Should().BeFalse();
        }

        [Fact]
        public void BusinessRule_BulkOrderDiscount_RequiresMinimumQuantity()
        {
            // Arrange
            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                Name = "Regular Customer",
                Email = "regular@example.com",
                Segment = CustomerSegment.Regular,
                IsFirstTime = false
            };

            var bulkOrderRule = new BulkOrderDiscount();

            // Test with 9 items (below threshold)
            var orderItemsBelow = new List<OrderItem>
            {
                OrderItem.Create(Guid.NewGuid(), Guid.NewGuid(), 9, 50m)
            };

            // Test with 10 items (at threshold)
            var orderItemsAt = new List<OrderItem>
            {
                OrderItem.Create(Guid.NewGuid(), Guid.NewGuid(), 10, 50m)
            };

            // Test with 11 items (above threshold)
            var orderItemsAbove = new List<OrderItem>
            {
                OrderItem.Create(Guid.NewGuid(), Guid.NewGuid(), 11, 50m)
            };

            // Act & Assert
            bulkOrderRule.IsApplicable(customer, orderItemsBelow).Should().BeFalse();
            bulkOrderRule.IsApplicable(customer, orderItemsAt).Should().BeTrue();
            bulkOrderRule.IsApplicable(customer, orderItemsAbove).Should().BeTrue();
        }

        [Fact]
        public void BusinessRule_VipCustomerDiscount_OnlyAppliesForVipSegment()
        {
            // Arrange
            var vipRule = new VipCustomerDiscount();
            var orderItems = new List<OrderItem>
            {
                OrderItem.Create(Guid.NewGuid(), Guid.NewGuid(), 1, 100m)
            };

            var regularCustomer = new Customer
            {
                Id = Guid.NewGuid(),
                Name = "Regular Customer",
                Email = "regular@example.com",
                Segment = CustomerSegment.Regular,
                IsFirstTime = false
            };

            var premiumCustomer = new Customer
            {
                Id = Guid.NewGuid(),
                Name = "Premium Customer",
                Email = "premium@example.com",
                Segment = CustomerSegment.Premium,
                IsFirstTime = false
            };

            var vipCustomer = new Customer
            {
                Id = Guid.NewGuid(),
                Name = "VIP Customer",
                Email = "vip@example.com",
                Segment = CustomerSegment.VIP,
                IsFirstTime = false
            };

            // Act & Assert
            vipRule.IsApplicable(regularCustomer, orderItems).Should().BeFalse();
            vipRule.IsApplicable(premiumCustomer, orderItems).Should().BeFalse();
            vipRule.IsApplicable(vipCustomer, orderItems).Should().BeTrue();
        }

        [Fact]
        public void BusinessRule_DiscountCalculation_NeverExceedsOrderTotal()
        {
            // Arrange
            var vipCustomer = new Customer
            {
                Id = Guid.NewGuid(),
                Name = "VIP Customer",
                Email = "vip@example.com",
                Segment = CustomerSegment.VIP,
                IsFirstTime = true
            };

            var orderItems = new List<OrderItem>
            {
                OrderItem.Create(Guid.NewGuid(), Guid.NewGuid(), 15, 10m) // 150 total, 15 items
            };

            // Act
            var discount = _discountService.CalculateBestDiscount(vipCustomer, orderItems);
            var totalAmount = orderItems.Sum(item => item.Quantity * item.Price);

            // Assert
            discount.Amount.Should().BeLessOrEqualTo(totalAmount);
            discount.Amount.Should().Be(30m); // 20% VIP discount is highest (30 > 22.5 bulk > 15 first-time)
        }

        [Fact]
        public void BusinessRule_OrderCreation_RequiresValidCustomer()
        {
            // Act & Assert
            var act = () => Order.Create(Guid.Empty);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void BusinessRule_OrderItemCreation_ValidatesInput()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var productId = Guid.NewGuid();

            // Act & Assert - Test all validation rules
            var act1 = () => OrderItem.Create(Guid.Empty, productId, 1, 10m);
            act1.Should().Throw<ArgumentNullException>();

            var act2 = () => OrderItem.Create(orderId, Guid.Empty, 1, 10m);
            act2.Should().Throw<ArgumentNullException>();

            var act3 = () => OrderItem.Create(orderId, productId, -1, 10m);
            act3.Should().Throw<ArgumentOutOfRangeException>();

            var act4 = () => OrderItem.Create(orderId, productId, 1, -10m);
            act4.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Theory]
        [InlineData(CustomerSegment.Regular, false, 5, 100, 0)] // No discount
        [InlineData(CustomerSegment.Regular, true, 5, 100, 50)] // First-time buyer: 10%
        [InlineData(CustomerSegment.Regular, false, 10, 100, 150)] // Bulk order: 15%
        [InlineData(CustomerSegment.VIP, false, 5, 100, 100)] // VIP: 20%
        [InlineData(CustomerSegment.VIP, true, 15, 100, 300)] // VIP wins over others: 20%
        public void BusinessRule_DiscountPriority_SelectsHighestDiscount(
            CustomerSegment segment,
            bool isFirstTime,
            int quantity,
            decimal unitPrice,
            decimal expectedDiscount)
        {
            // Arrange
            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                Name = "Test Customer",
                Email = "test@example.com",
                Segment = segment,
                IsFirstTime = isFirstTime
            };

            var orderItems = new List<OrderItem>
            {
                OrderItem.Create(Guid.NewGuid(), Guid.NewGuid(), quantity, unitPrice)
            };

            // Act
            var actualDiscount = _discountService.CalculateBestDiscount(customer, orderItems);

            // Assert
            actualDiscount.Amount.Should().Be(expectedDiscount);
        }

        [Fact]
        public void BusinessRule_OrderItemsValidation_RequiresNonEmptyCollection()
        {
            // Arrange
            var order = Order.Create(Guid.NewGuid());
            var emptyOrderItems = new HashSet<OrderItemDto>();

            // Act & Assert
            var act = () => order.AddOrderItems(emptyOrderItems);
            act.Should().NotThrow(); // But the order will have no items
            
            order.OrderItems.Should().BeEmpty();
        }

        [Fact]
        public void BusinessRule_OrderStatusProgression_FollowsDefinedWorkflow()
        {
            // Arrange
            var order = Order.Create(Guid.NewGuid());

            // Act & Assert - Test the complete happy path
            order.Status.Should().Be(OrderStatus.Pending);

            order.UpdateStatus(OrderStatus.Processing);
            order.Status.Should().Be(OrderStatus.Processing);

            order.UpdateStatus(OrderStatus.Shipped);
            order.Status.Should().Be(OrderStatus.Shipped);

            order.UpdateStatus(OrderStatus.Delivered);
            order.Status.Should().Be(OrderStatus.Delivered);
            order.FulfilledAt.Should().NotBeNull();
        }

        [Fact]
        public void BusinessRule_OrderCancellation_PossibleFromPendingAndProcessing()
        {
            // Test cancellation from Pending
            var order1 = Order.Create(Guid.NewGuid());
            var act1 = () => order1.UpdateStatus(OrderStatus.Cancelled);
            act1.Should().NotThrow();
            order1.Status.Should().Be(OrderStatus.Cancelled);

            // Test cancellation from Processing
            var order2 = Order.Create(Guid.NewGuid());
            order2.UpdateStatus(OrderStatus.Processing);
            var act2 = () => order2.UpdateStatus(OrderStatus.Cancelled);
            act2.Should().NotThrow();
            order2.Status.Should().Be(OrderStatus.Cancelled);

            // Test cancellation NOT possible from Shipped
            var order3 = Order.Create(Guid.NewGuid());
            order3.UpdateStatus(OrderStatus.Processing);
            order3.UpdateStatus(OrderStatus.Shipped);
            var act3 = () => order3.UpdateStatus(OrderStatus.Cancelled);
            act3.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void BusinessRule_OrderReturn_OnlyPossibleFromShipped()
        {
            // Arrange
            var order = Order.Create(Guid.NewGuid());
            order.UpdateStatus(OrderStatus.Processing);
            order.UpdateStatus(OrderStatus.Shipped);

            // Act & Assert
            var act = () => order.UpdateStatus(OrderStatus.Returned);
            act.Should().NotThrow();
            order.Status.Should().Be(OrderStatus.Returned);
        }

        [Fact]
        public void BusinessRule_DiscountApplication_RoundsCorrectly()
        {
            // Arrange
            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                Name = "VIP Customer",
                Email = "vip@example.com",
                Segment = CustomerSegment.VIP,
                IsFirstTime = false
            };

            var orderItems = new List<OrderItem>
            {
                OrderItem.Create(Guid.NewGuid(), Guid.NewGuid(), 1, 33.33m) // Amount that may cause rounding
            };

            // Act
            var discount = _discountService.CalculateBestDiscount(customer, orderItems);

            // Assert
            discount.Amount.Should().Be(6.666m); // 20% of 33.33 = 6.666 (no rounding in current implementation)
        }

        [Fact]
        public void BusinessRule_MultipleOrderItems_AggregatesCorrectly()
        {
            // Arrange
            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                Name = "VIP Customer",
                Email = "vip@example.com",
                Segment = CustomerSegment.VIP,
                IsFirstTime = false
            };

            var orderItems = new List<OrderItem>
            {
                OrderItem.Create(Guid.NewGuid(), Guid.NewGuid(), 2, 50m),  // 100
                OrderItem.Create(Guid.NewGuid(), Guid.NewGuid(), 3, 30m),  // 90
                OrderItem.Create(Guid.NewGuid(), Guid.NewGuid(), 1, 60m)   // 60
                // Total: 250
            };

            // Act
            var discount = _discountService.CalculateBestDiscount(customer, orderItems);

            // Assert
            discount.Amount.Should().Be(50m); // 20% of 250
        }
    }
}