using FluentAssertions;
using order_management_api_assessment.Features.Orders;

namespace order_management_api_assessment_tests
{
    public class OrderItemTests
    {
        [Fact]
        public void Create_ValidParameters_CreatesOrderItemWithCorrectProperties()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var quantity = 5;
            var unitPrice = 100m;

            // Act
            var orderItem = OrderItem.Create(orderId, productId, quantity, unitPrice);

            // Assert
            orderItem.Should().NotBeNull();
            orderItem.Id.Should().NotBe(Guid.Empty);
            orderItem.OrderId.Should().Be(orderId);
            orderItem.ProductId.Should().Be(productId);
            orderItem.Quantity.Should().Be(quantity);
            orderItem.Price.Should().Be(unitPrice);
        }

        [Fact]
        public void Create_EmptyOrderId_ThrowsArgumentNullException()
        {
            // Arrange
            var orderId = Guid.Empty;
            var productId = Guid.NewGuid();
            var quantity = 1;
            var unitPrice = 50m;

            // Act & Assert
            var act = () => OrderItem.Create(orderId, productId, quantity, unitPrice);
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName(nameof(orderId));
        }

        [Fact]
        public void Create_EmptyProductId_ThrowsArgumentNullException()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var productId = Guid.Empty;
            var quantity = 1;
            var unitPrice = 50m;

            // Act & Assert
            var act = () => OrderItem.Create(orderId, productId, quantity, unitPrice);
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName(nameof(productId));
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-10)]
        [InlineData(int.MinValue)]
        public void Create_NegativeQuantity_ThrowsArgumentOutOfRangeException(int quantity)
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var unitPrice = 50m;

            // Act & Assert
            var act = () => OrderItem.Create(orderId, productId, quantity, unitPrice);
            act.Should().Throw<ArgumentOutOfRangeException>()
                .WithParameterName(nameof(quantity));
        }

        [Theory]
        [InlineData(-0.01)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void Create_NegativeUnitPrice_ThrowsArgumentOutOfRangeException(decimal unitPrice)
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var quantity = 1;

            // Act & Assert
            var act = () => OrderItem.Create(orderId, productId, quantity, unitPrice);
            act.Should().Throw<ArgumentOutOfRangeException>()
                .WithParameterName(nameof(unitPrice));
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 10.50)]
        [InlineData(5, 25.99)]
        [InlineData(100, 1.00)]
        [InlineData(1000, 999.99)]
        public void Create_ValidQuantityAndPrice_CreatesSuccessfully(int quantity, decimal unitPrice)
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var productId = Guid.NewGuid();

            // Act
            var orderItem = OrderItem.Create(orderId, productId, quantity, unitPrice);

            // Assert
            orderItem.Should().NotBeNull();
            orderItem.Quantity.Should().Be(quantity);
            orderItem.Price.Should().Be(unitPrice);
        }

        [Fact]
        public void Create_ZeroQuantity_CreatesSuccessfully()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var quantity = 0;
            var unitPrice = 50m;

            // Act
            var orderItem = OrderItem.Create(orderId, productId, quantity, unitPrice);

            // Assert
            orderItem.Should().NotBeNull();
            orderItem.Quantity.Should().Be(0);
        }

        [Fact]
        public void Create_ZeroUnitPrice_CreatesSuccessfully()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var quantity = 5;
            var unitPrice = 0m;

            // Act
            var orderItem = OrderItem.Create(orderId, productId, quantity, unitPrice);

            // Assert
            orderItem.Should().NotBeNull();
            orderItem.Price.Should().Be(0m);
        }

        [Fact]
        public void Create_MultipleOrderItems_HaveUniqueIds()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var quantity = 1;
            var unitPrice = 50m;

            // Act
            var orderItem1 = OrderItem.Create(orderId, productId, quantity, unitPrice);
            var orderItem2 = OrderItem.Create(orderId, productId, quantity, unitPrice);

            // Assert
            orderItem1.Id.Should().NotBe(orderItem2.Id);
        }

        [Theory]
        [InlineData(1, 10.00)]
        [InlineData(5, 20.50)]
        [InlineData(10, 99.99)]
        [InlineData(100, 0.01)]
        public void Create_CalculatesTotalValue_IsCorrect(int quantity, decimal unitPrice)
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var expectedTotal = quantity * unitPrice;

            // Act
            var orderItem = OrderItem.Create(orderId, productId, quantity, unitPrice);

            // Assert - While OrderItem doesn't have a Total property, we can verify the calculation logic
            var calculatedTotal = orderItem.Quantity * orderItem.Price;
            calculatedTotal.Should().Be(expectedTotal);
        }

        [Fact]
        public void Create_MaxValues_HandlesCorrectly()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var quantity = int.MaxValue;
            var unitPrice = decimal.MaxValue;

            // Act
            var orderItem = OrderItem.Create(orderId, productId, quantity, unitPrice);

            // Assert
            orderItem.Should().NotBeNull();
            orderItem.Quantity.Should().Be(int.MaxValue);
            orderItem.Price.Should().Be(decimal.MaxValue);
        }

        [Fact]
        public void Create_VerySmallPrice_HandlesCorrectly()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var quantity = 1;
            var unitPrice = 0.0001m; // Very small but positive price

            // Act
            var orderItem = OrderItem.Create(orderId, productId, quantity, unitPrice);

            // Assert
            orderItem.Should().NotBeNull();
            orderItem.Price.Should().Be(0.0001m);
        }

        [Theory]
        [InlineData(1, "1.234567890123456789012345678")] // More than 28 decimal places
        [InlineData(1, "99.999999999999999999999999999")] // High precision decimal
        public void Create_HighPrecisionDecimal_HandlesCorrectly(int quantity, string unitPriceString)
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var unitPrice = decimal.Parse(unitPriceString);

            // Act
            var orderItem = OrderItem.Create(orderId, productId, quantity, unitPrice);

            // Assert
            orderItem.Should().NotBeNull();
            orderItem.Price.Should().Be(unitPrice);
        }

        [Fact]
        public void OrderItem_Properties_AreReadOnly()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var quantity = 5;
            var unitPrice = 100m;

            // Act
            var orderItem = OrderItem.Create(orderId, productId, quantity, unitPrice);

            // Assert - Verify properties have private setters by checking they can't be modified
            var type = typeof(OrderItem);
            var properties = type.GetProperties();
            
            foreach (var property in properties)
            {
                var setter = property.GetSetMethod();
                if (setter != null)
                {
                    setter.IsPrivate.Should().BeTrue($"Property {property.Name} should have a private setter");
                }
            }
        }
    }
}