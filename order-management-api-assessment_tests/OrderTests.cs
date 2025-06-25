using FluentAssertions;
using order_management_api_assessment.Features.Customers;
using order_management_api_assessment.Features.Orders;
using order_management_api_assessment.Features.Orders.Command.Create;
using order_management_api_assessment.Shared.Enums;

namespace order_management_api_assessment_tests
{
    public class OrderTests
    {
        [Fact]
        public void Create_ValidCustomerId_CreatesOrderWithCorrectProperties()
        {
            // Arrange
            var customerId = Guid.NewGuid();

            // Act
            var order = Order.Create(customerId);

            // Assert
            order.Should().NotBeNull();
            order.OrderId.Should().NotBe(Guid.Empty);
            order.CustomerId.Should().Be(customerId);
            order.Status.Should().Be(OrderStatus.Pending);
            order.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            order.FulfilledAt.Should().BeNull();
            order.OrderItems.Should().BeEmpty();
        }

        [Fact]
        public void Create_EmptyCustomerId_ThrowsArgumentNullException()
        {
            // Arrange
            var customerId = Guid.Empty;

            // Act & Assert
            var act = () => Order.Create(customerId);
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName(nameof(customerId));
        }

        [Fact]
        public void AddOrderItems_ValidItems_AddsItemsToOrder()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var order = Order.Create(customerId);
            var orderItems = new HashSet<OrderItemDto>
            {
                new(Guid.NewGuid(), 2, 100m),
                new(Guid.NewGuid(), 3, 50m)
            };

            // Act
            order.AddOrderItems(orderItems);

            // Assert
            order.OrderItems.Should().HaveCount(2);
            order.OrderItems.Should().OnlyContain(item => item.OrderId == order.OrderId);
        }

        [Fact]
        public void UpdateStatus_ValidTransition_UpdatesStatusSuccessfully()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var order = Order.Create(customerId);

            // Act
            order.UpdateStatus(OrderStatus.Processing);

            // Assert
            order.Status.Should().Be(OrderStatus.Processing);
            order.FulfilledAt.Should().BeNull();
        }

        [Fact]
        public void UpdateStatus_ToDelivered_UpdatesFulfilledAt()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var order = Order.Create(customerId);
            order.UpdateStatus(OrderStatus.Processing);
            order.UpdateStatus(OrderStatus.Shipped);

            // Act
            order.UpdateStatus(OrderStatus.Delivered);

            // Assert
            order.Status.Should().Be(OrderStatus.Delivered);
            order.FulfilledAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void UpdateStatus_InvalidTransition_ThrowsInvalidOperationException()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var order = Order.Create(customerId);

            // Act & Assert
            var act = () => order.UpdateStatus(OrderStatus.Shipped);
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Cannot transition from Pending to Shipped");
        }

        [Theory]
        [InlineData(OrderStatus.Pending, OrderStatus.Processing, true)]
        [InlineData(OrderStatus.Pending, OrderStatus.Cancelled, true)]
        [InlineData(OrderStatus.Pending, OrderStatus.Shipped, false)]
        [InlineData(OrderStatus.Processing, OrderStatus.Shipped, true)]
        [InlineData(OrderStatus.Processing, OrderStatus.Cancelled, true)]
        [InlineData(OrderStatus.Processing, OrderStatus.Delivered, false)]
        [InlineData(OrderStatus.Shipped, OrderStatus.Delivered, true)]
        [InlineData(OrderStatus.Shipped, OrderStatus.Returned, true)]
        [InlineData(OrderStatus.Shipped, OrderStatus.Cancelled, false)]
        [InlineData(OrderStatus.Delivered, OrderStatus.Returned, false)]
        [InlineData(OrderStatus.Cancelled, OrderStatus.Processing, false)]
        [InlineData(OrderStatus.Returned, OrderStatus.Delivered, false)]
        public void UpdateStatus_VariousTransitions_BehavesCorrectly(
            OrderStatus fromStatus, 
            OrderStatus toStatus, 
            bool shouldSucceed)
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var order = Order.Create(customerId);
            
            // Set up the initial status through valid transitions
            if (fromStatus != OrderStatus.Pending)
            {
                if (fromStatus == OrderStatus.Processing)
                {
                    order.UpdateStatus(OrderStatus.Processing);
                }
                else if (fromStatus == OrderStatus.Shipped)
                {
                    order.UpdateStatus(OrderStatus.Processing);
                    order.UpdateStatus(OrderStatus.Shipped);
                }
                else if (fromStatus == OrderStatus.Delivered)
                {
                    order.UpdateStatus(OrderStatus.Processing);
                    order.UpdateStatus(OrderStatus.Shipped);
                    order.UpdateStatus(OrderStatus.Delivered);
                }
                else if (fromStatus == OrderStatus.Cancelled)
                {
                    order.UpdateStatus(OrderStatus.Cancelled);
                }
                else if (fromStatus == OrderStatus.Returned)
                {
                    order.UpdateStatus(OrderStatus.Processing);
                    order.UpdateStatus(OrderStatus.Shipped);
                    order.UpdateStatus(OrderStatus.Returned);
                }
            }

            // Act & Assert
            if (shouldSucceed)
            {
                var act = () => order.UpdateStatus(toStatus);
                act.Should().NotThrow();
                order.Status.Should().Be(toStatus);
            }
            else
            {
                var act = () => order.UpdateStatus(toStatus);
                act.Should().Throw<InvalidOperationException>();
            }
        }

        [Fact]
        public void ApplyDiscount_ValidAmount_UpdatesDiscountedAmount()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var order = Order.Create(customerId);
            var discountAmount = 50m;

            // Act
            order.ApplyDiscount(discountAmount);

            // Assert
            order.DiscountedAmount.Should().Be(discountAmount);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(100.50)]
        [InlineData(999.99)]
        public void ApplyDiscount_VariousAmounts_UpdatesCorrectly(decimal discountAmount)
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var order = Order.Create(customerId);

            // Act
            order.ApplyDiscount(discountAmount);

            // Assert
            order.DiscountedAmount.Should().Be(discountAmount);
        }

        [Fact]
        public void ApplyDiscount_MultipleApplications_UpdatesToLatestAmount()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var order = Order.Create(customerId);

            // Act
            order.ApplyDiscount(25m);
            order.ApplyDiscount(75m);

            // Assert
            order.DiscountedAmount.Should().Be(75m);
        }

        [Fact]
        public void OrderItems_ReadOnlyCollection_CannotBeModifiedDirectly()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var order = Order.Create(customerId);

            // Act & Assert
            order.OrderItems.Should().BeAssignableTo<IReadOnlyList<OrderItem>>();
            
            // The property returns IReadOnlyList<OrderItem> which ensures read-only access
            var orderItemsProperty = typeof(Order).GetProperty(nameof(Order.OrderItems));
            orderItemsProperty.Should().NotBeNull();
            orderItemsProperty!.PropertyType.Should().Be<IReadOnlyList<OrderItem>>();
        }

        [Fact]
        public void Order_AfterCreation_HasUniqueOrderId()
        {
            // Arrange & Act
            var customerId = Guid.NewGuid();
            var order1 = Order.Create(customerId);
            var order2 = Order.Create(customerId);

            // Assert
            order1.OrderId.Should().NotBe(order2.OrderId);
        }

        [Fact]
        public void Order_CompleteWorkflow_ExecutesSuccessfully()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var order = Order.Create(customerId);
            var orderItems = new HashSet<OrderItemDto>
            {
                new(Guid.NewGuid(), 2, 100m)
            };

            // Act & Assert - Complete order lifecycle
            order.AddOrderItems(orderItems);
            order.OrderItems.Should().HaveCount(1);

            order.ApplyDiscount(20m);
            order.DiscountedAmount.Should().Be(20m);

            order.UpdateStatus(OrderStatus.Processing);
            order.Status.Should().Be(OrderStatus.Processing);

            order.UpdateStatus(OrderStatus.Shipped);
            order.Status.Should().Be(OrderStatus.Shipped);

            order.UpdateStatus(OrderStatus.Delivered);
            order.Status.Should().Be(OrderStatus.Delivered);
            order.FulfilledAt.Should().NotBeNull();
        }

        [Fact]
        public void Order_CancelledWorkflow_ExecutesSuccessfully()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var order = Order.Create(customerId);

            // Act & Assert - Cancelled order lifecycle
            order.UpdateStatus(OrderStatus.Processing);
            order.Status.Should().Be(OrderStatus.Processing);

            order.UpdateStatus(OrderStatus.Cancelled);
            order.Status.Should().Be(OrderStatus.Cancelled);
            order.FulfilledAt.Should().BeNull();
        }

        [Fact]
        public void Order_ReturnedWorkflow_ExecutesSuccessfully()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var order = Order.Create(customerId);

            // Act & Assert - Returned order lifecycle
            order.UpdateStatus(OrderStatus.Processing);
            order.UpdateStatus(OrderStatus.Shipped);
            order.UpdateStatus(OrderStatus.Returned);
            
            order.Status.Should().Be(OrderStatus.Returned);
            order.FulfilledAt.Should().BeNull();
        }
    }
}