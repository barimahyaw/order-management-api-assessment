using order_management_api_assessment.Models;

namespace order_management_api_assessment.Data;

public static class SeedData
{
    public static async Task InitializeAsync(OrderContext context)
    {
        if (context.Customers.Any()) return; // Already seeded

        var customers = CreateCustomers();
        var orders = CreateOrders();
        var orderItems = CreateOrderItems();

        context.Customers.AddRange(customers);
        context.Orders.AddRange(orders);
        context.OrderItems.AddRange(orderItems);

        await context.SaveChangesAsync();
    }

    private static List<Customer> CreateCustomers()
    {
        return
        [
            new Customer
            {
                Id = new Guid("2585a176-1e69-4d3c-b174-9da5f5521505"),
                Name = "John Doe",
                Email = "john.doe@email.com",
                Segment = CustomerSegment.Regular,
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                IsFirstTime = false
            },
            new Customer
            {
                Id = new Guid("8f4e2c1d-5a7b-4e9f-a3c2-7b8d4e5f6a9c"),
                Name = "Jane Smith",
                Email = "jane.smith@email.com",
                Segment = CustomerSegment.Premium,
                CreatedAt = DateTime.UtcNow.AddDays(-60),
                IsFirstTime = false
            },
            new Customer
            {
                Id = new Guid("c7b3f8e2-9a4d-4c5e-b8f1-3e7a9b2c4d5f"),
                Name = "Alice Johnson",
                Email = "alice.johnson@email.com",
                Segment = CustomerSegment.VIP,
                CreatedAt = DateTime.UtcNow.AddDays(-90),
                IsFirstTime = false
            },
            new Customer
            {
                Id = new Guid("d4a8c2e7-3b5f-4e1d-9c7a-6f2b8e4a7c9d"),
                Name = "Bob Wilson",
                Email = "bob.wilson@email.com",
                Segment = CustomerSegment.Regular,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                IsFirstTime = true
            },
            new Customer
            {
                Id = new Guid("f9e3b7c1-4d6a-4f2e-8b5c-1a9f3c7e2b4d"),
                Name = "Emma Davis",
                Email = "emma.davis@email.com",
                Segment = CustomerSegment.Premium,
                CreatedAt = DateTime.UtcNow.AddDays(-15),
                IsFirstTime = false
            }
        ];
    }

    private static List<Order> CreateOrders()
    {
        var orders = new List<Order>();

        // Create orders using the factory method, then manually set properties for seeding
        var order1 = Order.Create(new Guid("2585a176-1e69-4d3c-b174-9da5f5521505"));
        SetOrderProperties(order1, new Guid("b8f2d4a7-3c5e-4b9f-a1d8-7e4c2f9b6a3d"), 299.99m, 299.99m, OrderStatus.Delivered, DateTime.UtcNow.AddDays(-25), DateTime.UtcNow.AddDays(-23));

        var order2 = Order.Create(new Guid("8f4e2c1d-5a7b-4e9f-a3c2-7b8d4e5f6a9c"));
        SetOrderProperties(order2, new Guid("e7c3a9d2-5f8b-4a1e-9c6d-2b7f4e8a1c5f"), 599.98m, 539.98m, OrderStatus.Shipped, DateTime.UtcNow.AddDays(-15));

        var order3 = Order.Create(new Guid("c7b3f8e2-9a4d-4c5e-b8f1-3e7a9b2c4d5f"));
        SetOrderProperties(order3, new Guid("a4d8f2b7-6e1c-4f3a-b9d5-8c2e7a4f1b6d"), 1299.97m, 1169.97m, OrderStatus.Processing, DateTime.UtcNow.AddDays(-10));

        var order4 = Order.Create(new Guid("d4a8c2e7-3b5f-4e1d-9c7a-6f2b8e4a7c9d"));
        SetOrderProperties(order4, new Guid("f1b6c9e4-2d7a-4e8f-c3b9-5f1d8a6e2c7b"), 149.99m, 134.99m, OrderStatus.Pending, DateTime.UtcNow.AddDays(-3));

        var order5 = Order.Create(new Guid("f9e3b7c1-4d6a-4f2e-8b5c-1a9f3c7e2b4d"));
        SetOrderProperties(order5, new Guid("c9e7b3f1-4a8d-4c2e-f7b4-1d9a5c8e3b6f"), 750.00m, 675.00m, OrderStatus.Cancelled, DateTime.UtcNow.AddDays(-8));

        orders.AddRange([order1, order2, order3, order4, order5]);
        return orders;
    }

    private static void SetOrderProperties(Order order, Guid orderId, decimal totalAmount, decimal discountedAmount, OrderStatus status, DateTime createdAt, DateTime? fulfilledAt = null)
    {
        // Use reflection to set private properties for seeding purposes
        var orderType = typeof(Order);
        orderType.GetProperty(nameof(Order.OrderId))?.SetValue(order, orderId);
        orderType.GetProperty(nameof(Order.TotalAmount))?.SetValue(order, totalAmount);
        orderType.GetProperty(nameof(Order.DiscountedAmount))?.SetValue(order, discountedAmount);
        orderType.GetProperty(nameof(Order.Status))?.SetValue(order, status);
        orderType.GetProperty(nameof(Order.CreatedAt))?.SetValue(order, createdAt);
        orderType.GetProperty(nameof(Order.FulfilledAt))?.SetValue(order, fulfilledAt);
    }

    private static List<Order.OrderItem> CreateOrderItems()
    {
        return
        [
            // Order 1 items
            Order.OrderItem.Create(new Guid("b8f2d4a7-3c5e-4b9f-a1d8-7e4c2f9b6a3d"), new Guid("a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d"), 2, 149.99m),
            Order.OrderItem.Create(new Guid("b8f2d4a7-3c5e-4b9f-a1d8-7e4c2f9b6a3d"), new Guid("b6c7d8e9-f0a1-4b2c-3d4e-5f6a7b8c9d0e"), 1, 299.99m),

            // Order 2 items
            Order.OrderItem.Create(new Guid("e7c3a9d2-5f8b-4a1e-9c6d-2b7f4e8a1c5f"), new Guid("a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d"), 3, 199.99m),
            Order.OrderItem.Create(new Guid("e7c3a9d2-5f8b-4a1e-9c6d-2b7f4e8a1c5f"), new Guid("f3e4d5c6-b7a8-4f9e-0d1c-2b3a4f5e6d7c"), 1, 99.99m),

            // Order 3 items
            Order.OrderItem.Create(new Guid("a4d8f2b7-6e1c-4f3a-b9d5-8c2e7a4f1b6d"), new Guid("e7f8a9b0-c1d2-4e3f-4a5b-6c7d8e9f0a1b"), 1, 1299.97m),

            // Order 4 items
            Order.OrderItem.Create(new Guid("f1b6c9e4-2d7a-4e8f-c3b9-5f1d8a6e2c7b"), new Guid("a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d"), 1, 149.99m),

            // Order 5 items
            Order.OrderItem.Create(new Guid("c9e7b3f1-4a8d-4c2e-f7b4-1d9a5c8e3b6f"), new Guid("b6c7d8e9-f0a1-4b2c-3d4e-5f6a7b8c9d0e"), 2, 375.00m)
        ];
    }
}