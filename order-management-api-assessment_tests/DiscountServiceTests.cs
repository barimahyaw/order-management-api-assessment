using FluentAssertions;
using order_management_api_assessment.Features.Customers;
using order_management_api_assessment.Features.Discounts;
using order_management_api_assessment.Features.Discounts.Services;
using order_management_api_assessment.Features.Orders;
using order_management_api_assessment.Shared.Enums;

namespace order_management_api_assessment_tests
{
    public class DiscountServiceTests
    {
        private readonly FirstTimeBuyerDiscount _firstTimeBuyerDiscount;
        private readonly BulkOrderDiscount _bulkOrderDiscount;
        private readonly VipCustomerDiscount _vipCustomerDiscount;
        private readonly DiscountService _discountService;

        public DiscountServiceTests()
        {
            _firstTimeBuyerDiscount = new FirstTimeBuyerDiscount();
            _bulkOrderDiscount = new BulkOrderDiscount();
            _vipCustomerDiscount = new VipCustomerDiscount();

            var discountRules = new List<IDiscountRule>
            {
                _firstTimeBuyerDiscount,
                _bulkOrderDiscount,
                _vipCustomerDiscount
            };

            _discountService = new DiscountService(discountRules);
        }

        [Fact]
        public void FirstTimeBuyerDiscount_NewRegularCustomer_Returns10PercentDiscount()
        {
            // Arrange
            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                Name = "John Doe",
                Email = "john@example.com",
                Segment = CustomerSegment.Regular,
                IsFirstTime = true
            };

            var orderItems = new List<OrderItem>
            {
                OrderItem.Create(Guid.NewGuid(), Guid.NewGuid(), 2, 100m) // Total: 200
            };

            // Act
            var isApplicable = _firstTimeBuyerDiscount.IsApplicable(customer, orderItems);
            var discount = _firstTimeBuyerDiscount.Calculate(customer, orderItems, 200m);

            // Assert
            isApplicable.Should().BeTrue();
            discount.Should().Be(20m); // 10% of 200
        }

        [Fact]
        public void FirstTimeBuyerDiscount_ExistingCustomer_IsNotApplicable()
        {
            // Arrange
            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                Name = "Jane Doe",
                Email = "jane@example.com",
                Segment = CustomerSegment.Regular,
                IsFirstTime = false
            };

            var orderItems = new List<OrderItem>
            {
                OrderItem.Create(Guid.NewGuid(), Guid.NewGuid(), 2, 100m)
            };

            // Act
            var isApplicable = _firstTimeBuyerDiscount.IsApplicable(customer, orderItems);

            // Assert
            isApplicable.Should().BeFalse();
        }

        [Fact]
        public void BulkOrderDiscount_OrderWithTenOrMoreItems_Returns15PercentDiscount()
        {
            // Arrange
            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                Name = "John Doe",
                Email = "john@example.com",
                Segment = CustomerSegment.Regular,
                IsFirstTime = false
            };

            var orderItems = new List<OrderItem>
            {
                OrderItem.Create(Guid.NewGuid(), Guid.NewGuid(), 10, 50m) // Total quantity: 10, Total: 500
            };

            // Act
            var isApplicable = _bulkOrderDiscount.IsApplicable(customer, orderItems);
            var discount = _bulkOrderDiscount.Calculate(customer, orderItems, 500m);

            // Assert
            isApplicable.Should().BeTrue();
            discount.Should().Be(75m); // 15% of 500
        }

        [Fact]
        public void BulkOrderDiscount_OrderUnderTenItems_IsNotApplicable()
        {
            // Arrange
            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                Name = "John Doe",
                Email = "john@example.com",
                Segment = CustomerSegment.Regular,
                IsFirstTime = false
            };

            var orderItems = new List<OrderItem>
            {
                OrderItem.Create(Guid.NewGuid(), Guid.NewGuid(), 5, 100m) // Total quantity: 5
            };

            // Act
            var isApplicable = _bulkOrderDiscount.IsApplicable(customer, orderItems);

            // Assert
            isApplicable.Should().BeFalse();
        }

        [Fact]
        public void VipCustomerDiscount_VipCustomer_Returns20PercentDiscount()
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
                OrderItem.Create(Guid.NewGuid(), Guid.NewGuid(), 3, 100m) // Total: 300
            };

            // Act
            var isApplicable = _vipCustomerDiscount.IsApplicable(customer, orderItems);
            var discount = _vipCustomerDiscount.Calculate(customer, orderItems, 300m);

            // Assert
            isApplicable.Should().BeTrue();
            discount.Should().Be(60m); // 20% of 300
        }

        [Fact]
        public void VipCustomerDiscount_RegularCustomer_IsNotApplicable()
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

            var orderItems = new List<OrderItem>
            {
                OrderItem.Create(Guid.NewGuid(), Guid.NewGuid(), 2, 100m)
            };

            // Act
            var isApplicable = _vipCustomerDiscount.IsApplicable(customer, orderItems);

            // Assert
            isApplicable.Should().BeFalse();
        }

        [Fact]
        public void DiscountService_CalculateBestDiscount_ReturnsHighestApplicableDiscount()
        {
            // Arrange - VIP customer making first-time purchase with bulk quantity
            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                Name = "VIP First Timer",
                Email = "vipfirst@example.com",
                Segment = CustomerSegment.VIP,
                IsFirstTime = true
            };

            var orderItems = new List<OrderItem>
            {
                OrderItem.Create(Guid.NewGuid(), Guid.NewGuid(), 15, 40m) // Total: 600, Quantity: 15
            };

            // Act
            var bestDiscount = _discountService.CalculateBestDiscount(customer, orderItems);

            // Assert
            // All three discounts apply:
            // First-time buyer: 10% of 600 = 60
            // Bulk order: 15% of 600 = 90
            // VIP customer: 20% of 600 = 120
            // Best discount should be VIP (20%)
            bestDiscount.Amount.Should().Be(120m);
        }

        [Fact]
        public void DiscountService_NoApplicableDiscounts_ReturnsZero()
        {
            // Arrange - Regular customer, not first-time, small order
            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                Name = "Regular Customer",
                Email = "regular@example.com",
                Segment = CustomerSegment.Regular,
                IsFirstTime = false
            };

            var orderItems = new List<OrderItem>
            {
                OrderItem.Create(Guid.NewGuid(), Guid.NewGuid(), 2, 50m) // Total: 100, Quantity: 2
            };

            // Act
            var bestDiscount = _discountService.CalculateBestDiscount(customer, orderItems);

            // Assert
            bestDiscount.Amount.Should().Be(0m);
        }

        [Theory]
        [InlineData(CustomerSegment.Regular, true, 1, 100, 10)] // First-time buyer: 10%
        [InlineData(CustomerSegment.Regular, false, 10, 50, 75)] // Bulk order: 15% of 500
        [InlineData(CustomerSegment.VIP, false, 1, 100, 20)] // VIP customer: 20%
        [InlineData(CustomerSegment.Premium, false, 1, 100, 0)] // No applicable discount
        public void DiscountService_VariousScenarios_CalculatesCorrectDiscount(
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
            var discount = _discountService.CalculateBestDiscount(customer, orderItems);

            // Assert
            discount.Amount.Should().Be(expectedDiscount);
        }

        [Fact]
        public void DiscountService_EmptyOrderItems_ReturnsZero()
        {
            // Arrange
            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                Name = "Test Customer",
                Email = "test@example.com",
                Segment = CustomerSegment.VIP,
                IsFirstTime = true
            };

            var orderItems = new List<OrderItem>();

            // Act
            var discount = _discountService.CalculateBestDiscount(customer, orderItems);

            // Assert
            discount.Amount.Should().Be(0m);
        }

        [Fact]
        public void DiscountService_MultipleOrderItems_CalculatesCorrectTotal()
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
                OrderItem.Create(Guid.NewGuid(), Guid.NewGuid(), 5, 50m), // 250
                OrderItem.Create(Guid.NewGuid(), Guid.NewGuid(), 3, 100m), // 300
                OrderItem.Create(Guid.NewGuid(), Guid.NewGuid(), 2, 75m) // 150
                // Total: 700, VIP discount: 20% = 140
            };

            // Act
            var discount = _discountService.CalculateBestDiscount(customer, orderItems);

            // Assert
            discount.Amount.Should().Be(140m);
        }

        [Fact]
        public void BulkOrderDiscount_ExactlyTenItems_IsApplicable()
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

            var orderItems = new List<OrderItem>
            {
                OrderItem.Create(Guid.NewGuid(), Guid.NewGuid(), 10, 30m) // Exactly 10 items
            };

            // Act
            var isApplicable = _bulkOrderDiscount.IsApplicable(customer, orderItems);
            var discount = _bulkOrderDiscount.Calculate(customer, orderItems, 300m);

            // Assert
            isApplicable.Should().BeTrue();
            discount.Should().Be(45m); // 15% of 300
        }

        [Fact]
        public void BulkOrderDiscount_MultipleItemsWithTotalQuantityOverTen_IsApplicable()
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

            var orderItems = new List<OrderItem>
            {
                OrderItem.Create(Guid.NewGuid(), Guid.NewGuid(), 4, 25m), // 4 items
                OrderItem.Create(Guid.NewGuid(), Guid.NewGuid(), 3, 50m), // 3 items
                OrderItem.Create(Guid.NewGuid(), Guid.NewGuid(), 5, 40m)  // 5 items (total: 12)
            };

            // Act
            var isApplicable = _bulkOrderDiscount.IsApplicable(customer, orderItems);
            var discount = _bulkOrderDiscount.Calculate(customer, orderItems, 450m);

            // Assert
            isApplicable.Should().BeTrue();
            discount.Should().Be(67.5m); // 15% of 450
        }

        [Fact]
        public void DiscountService_VipFirstTimeBuyerWithBulkOrder_SelectsHighestDiscount()
        {
            // Arrange - Customer qualifies for all three discounts
            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                Name = "VIP First Timer",
                Email = "vipfirst@example.com",
                Segment = CustomerSegment.VIP,
                IsFirstTime = true
            };

            var orderItems = new List<OrderItem>
            {
                OrderItem.Create(Guid.NewGuid(), Guid.NewGuid(), 12, 100m) // 1200 total, 12 items
            };

            // Act
            var discount = _discountService.CalculateBestDiscount(customer, orderItems);

            // Assert
            // First-time: 10% of 1200 = 120
            // Bulk: 15% of 1200 = 180  
            // VIP: 20% of 1200 = 240 (highest)
            discount.Amount.Should().Be(240m);
        }

        [Theory]
        [InlineData(0.01)] // Very small amount
        [InlineData(9999.99)] // Large amount
        [InlineData(1000000)] // Very large amount
        public void DiscountService_VariousAmounts_CalculatesCorrectly(decimal unitPrice)
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
                OrderItem.Create(Guid.NewGuid(), Guid.NewGuid(), 1, unitPrice)
            };

            // Act
            var discount = _discountService.CalculateBestDiscount(customer, orderItems);

            // Assert
            discount.Amount.Should().Be(unitPrice * 0.2m); // 20% VIP discount
        }
    }
}
