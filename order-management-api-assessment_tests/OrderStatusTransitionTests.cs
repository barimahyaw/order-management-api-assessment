using FluentAssertions;
using order_management_api_assessment.Shared.Enums;

namespace order_management_api_assessment_tests
{
    public class OrderStatusTransitionTests
    {
        [Theory]
        [InlineData(OrderStatus.Pending, OrderStatus.Processing, true)]
        [InlineData(OrderStatus.Pending, OrderStatus.Cancelled, true)]
        [InlineData(OrderStatus.Pending, OrderStatus.Shipped, false)]
        [InlineData(OrderStatus.Pending, OrderStatus.Delivered, false)]
        [InlineData(OrderStatus.Pending, OrderStatus.Returned, false)]
        public void PendingOrder_ValidateTransitions(OrderStatus from, OrderStatus to, bool expected)
        {
            // Act
            var canTransition = from.CanTransitionTo(to);

            // Assert
            canTransition.Should().Be(expected);
        }

        [Theory]
        [InlineData(OrderStatus.Processing, OrderStatus.Shipped, true)]
        [InlineData(OrderStatus.Processing, OrderStatus.Cancelled, true)]
        [InlineData(OrderStatus.Processing, OrderStatus.Pending, false)]
        [InlineData(OrderStatus.Processing, OrderStatus.Delivered, false)]
        [InlineData(OrderStatus.Processing, OrderStatus.Returned, false)]
        public void ProcessingOrder_ValidateTransitions(OrderStatus from, OrderStatus to, bool expected)
        {
            // Act
            var canTransition = from.CanTransitionTo(to);

            // Assert
            canTransition.Should().Be(expected);
        }

        [Theory]
        [InlineData(OrderStatus.Shipped, OrderStatus.Delivered, true)]
        [InlineData(OrderStatus.Shipped, OrderStatus.Returned, true)]
        [InlineData(OrderStatus.Shipped, OrderStatus.Pending, false)]
        [InlineData(OrderStatus.Shipped, OrderStatus.Processing, false)]
        [InlineData(OrderStatus.Shipped, OrderStatus.Cancelled, false)]
        public void ShippedOrder_ValidateTransitions(OrderStatus from, OrderStatus to, bool expected)
        {
            // Act
            var canTransition = from.CanTransitionTo(to);

            // Assert
            canTransition.Should().Be(expected);
        }

        [Theory]
        [InlineData(OrderStatus.Delivered, OrderStatus.Pending, false)]
        [InlineData(OrderStatus.Delivered, OrderStatus.Processing, false)]
        [InlineData(OrderStatus.Delivered, OrderStatus.Shipped, false)]
        [InlineData(OrderStatus.Delivered, OrderStatus.Cancelled, false)]
        [InlineData(OrderStatus.Delivered, OrderStatus.Returned, false)]
        public void DeliveredOrder_NoValidTransitions(OrderStatus from, OrderStatus to, bool expected)
        {
            // Act
            var canTransition = from.CanTransitionTo(to);

            // Assert
            canTransition.Should().Be(expected);
        }

        [Theory]
        [InlineData(OrderStatus.Cancelled, OrderStatus.Pending, false)]
        [InlineData(OrderStatus.Cancelled, OrderStatus.Processing, false)]
        [InlineData(OrderStatus.Cancelled, OrderStatus.Shipped, false)]
        [InlineData(OrderStatus.Cancelled, OrderStatus.Delivered, false)]
        [InlineData(OrderStatus.Cancelled, OrderStatus.Returned, false)]
        public void CancelledOrder_NoValidTransitions(OrderStatus from, OrderStatus to, bool expected)
        {
            // Act
            var canTransition = from.CanTransitionTo(to);

            // Assert
            canTransition.Should().Be(expected);
        }

        [Theory]
        [InlineData(OrderStatus.Returned, OrderStatus.Pending, false)]
        [InlineData(OrderStatus.Returned, OrderStatus.Processing, false)]
        [InlineData(OrderStatus.Returned, OrderStatus.Shipped, false)]
        [InlineData(OrderStatus.Returned, OrderStatus.Delivered, false)]
        [InlineData(OrderStatus.Returned, OrderStatus.Cancelled, false)]
        public void ReturnedOrder_NoValidTransitions(OrderStatus from, OrderStatus to, bool expected)
        {
            // Act
            var canTransition = from.CanTransitionTo(to);

            // Assert
            canTransition.Should().Be(expected);
        }

        [Fact]
        public void GetValidTransitions_PendingOrder_ReturnsCorrectOptions()
        {
            // Act
            var validTransitions = OrderStatus.Pending.GetValidTransitions();

            // Assert
            validTransitions.Should().HaveCount(2);
            validTransitions.Should().Contain(OrderStatus.Processing);
            validTransitions.Should().Contain(OrderStatus.Cancelled);
        }

        [Fact]
        public void GetValidTransitions_ProcessingOrder_ReturnsCorrectOptions()
        {
            // Act
            var validTransitions = OrderStatus.Processing.GetValidTransitions();

            // Assert
            validTransitions.Should().HaveCount(2);
            validTransitions.Should().Contain(OrderStatus.Shipped);
            validTransitions.Should().Contain(OrderStatus.Cancelled);
        }

        [Fact]
        public void GetValidTransitions_ShippedOrder_ReturnsCorrectOptions()
        {
            // Act
            var validTransitions = OrderStatus.Shipped.GetValidTransitions();

            // Assert
            validTransitions.Should().HaveCount(2);
            validTransitions.Should().Contain(OrderStatus.Delivered);
            validTransitions.Should().Contain(OrderStatus.Returned);
        }

        [Fact]
        public void GetValidTransitions_FinalStates_ReturnEmptyList()
        {
            // Act & Assert
            OrderStatus.Delivered.GetValidTransitions().Should().BeEmpty();
            OrderStatus.Cancelled.GetValidTransitions().Should().BeEmpty();
            OrderStatus.Returned.GetValidTransitions().Should().BeEmpty();
        }

        [Fact]
        public void CanTransitionTo_SameStatus_ReturnsFalse()
        {
            // Arrange
            var allStatuses = Enum.GetValues<OrderStatus>();

            // Act & Assert
            foreach (var status in allStatuses)
            {
                status.CanTransitionTo(status).Should().BeFalse(
                    $"Status {status} should not be able to transition to itself");
            }
        }

        [Fact]
        public void AllOrderStatuses_HaveDefinedTransitionRules()
        {
            // Arrange
            var allStatuses = Enum.GetValues<OrderStatus>();

            // Act & Assert
            foreach (var status in allStatuses)
            {
                var act = () => status.GetValidTransitions();
                act.Should().NotThrow($"Status {status} should have defined transition rules");
            }
        }
    }
}