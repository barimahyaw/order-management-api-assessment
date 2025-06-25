using Microsoft.EntityFrameworkCore;
using order_management_api_assessment.Features.Customers;
using order_management_api_assessment.Features.Orders;
using order_management_api_assessment.Shared.Enums;

namespace order_management_api_assessment.Shared.Data;

public class OrderManagementDbContext(DbContextOptions<OrderManagementDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Customer> Customers { get; set; }
    //public DbSet<Product> Products { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure entity properties and relationships here
        ConfigureEntities(modelBuilder);
        
        // Seed initial data
        SeedData(modelBuilder);
    }

    private static void ConfigureEntities(ModelBuilder modelBuilder)
    {
        // Configure Order entity
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId);
            entity.Property(e => e.OrderId).ValueGeneratedNever();
            entity.Property(e => e.CustomerId).IsRequired();
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
            entity.Property(e => e.DiscountedAmount).HasPrecision(18, 2);
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();

            // Performance optimization: Add index on frequently queried columns
            entity.HasIndex(e => e.CustomerId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
        });

        // Configure OrderItem entity
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.OrderId).IsRequired();
            entity.Property(e => e.ProductId).IsRequired();
            entity.Property(e => e.Quantity).IsRequired();
            entity.Property(e => e.Price).HasPrecision(18, 2);

            // Performance optimization: Add index on OrderId for joins
            entity.HasIndex(e => e.OrderId);
        });

        // Configure Customer entity
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Segment).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();

            // Performance optimization: Add unique index on email
            entity.HasIndex(e => e.Email).IsUnique();
        });
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {        
        // Seed Customers
        var customers = new[]
        {
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
        };

        modelBuilder.Entity<Customer>().HasData(customers);        
        
        // Seed Orders
        var orders = new[]
        {
            new
            {
                OrderId = new Guid("b8f2d4a7-3c5e-4b9f-a1d8-7e4c2f9b6a3d"),
                CustomerId = new Guid("2585a176-1e69-4d3c-b174-9da5f5521505"),
                TotalAmount = 299.99m,
                DiscountedAmount = 299.99m,
                Status = OrderStatus.Delivered,
                CreatedAt = DateTime.UtcNow.AddDays(-25),
                FulfilledAt = DateTime.UtcNow.AddDays(-23) as DateTime?
            },
            new
            {
                OrderId = new Guid("e7c3a9d2-5f8b-4a1e-9c6d-2b7f4e8a1c5f"),
                CustomerId = new Guid("8f4e2c1d-5a7b-4e9f-a3c2-7b8d4e5f6a9c"),
                TotalAmount = 599.98m,
                DiscountedAmount = 539.98m, // 10% premium discount
                Status = OrderStatus.Shipped,
                CreatedAt = DateTime.UtcNow.AddDays(-15),
                FulfilledAt = null as DateTime?
            },
            new
            {
                OrderId = new Guid("a4d8f2b7-6e1c-4f3a-b9d5-8c2e7a4f1b6d"),
                CustomerId = new Guid("c7b3f8e2-9a4d-4c5e-b8f1-3e7a9b2c4d5f"),
                TotalAmount = 1299.97m,
                DiscountedAmount = 1169.97m, // 10% VIP discount
                Status = OrderStatus.Processing,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                FulfilledAt = null as DateTime?
            },
            new
            {
                OrderId = new Guid("f1b6c9e4-2d7a-4e8f-c3b9-5f1d8a6e2c7b"),
                CustomerId = new Guid("d4a8c2e7-3b5f-4e1d-9c7a-6f2b8e4a7c9d"),
                TotalAmount = 149.99m,
                DiscountedAmount = 134.99m, // 10% first-time buyer discount
                Status = OrderStatus.Pending,
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                FulfilledAt = null as DateTime?
            },
            new
            {
                OrderId = new Guid("c9e7b3f1-4a8d-4c2e-f7b4-1d9a5c8e3b6f"),
                CustomerId = new Guid("f9e3b7c1-4d6a-4f2e-8b5c-1a9f3c7e2b4d"),
                TotalAmount = 750.00m,
                DiscountedAmount = 675.00m, // 10% premium discount
                Status = OrderStatus.Cancelled,
                CreatedAt = DateTime.UtcNow.AddDays(-8),
                FulfilledAt = null as DateTime?
            }
        };

        modelBuilder.Entity<Order>().HasData(orders);        
        
        // Seed OrderItems
        var orderItems = new[]
        {
            // Order 1 items
            new
            {
                Id = new Guid("d8f4b2c7-1a6e-4d9f-b3c8-5e2a7b4d9f1c"),
                OrderId = new Guid("b8f2d4a7-3c5e-4b9f-a1d8-7e4c2f9b6a3d"),
                ProductId = new Guid("a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d"), // Mock product ID
                Quantity = 2,
                Price = 149.99m
            },
            new
            {
                Id = new Guid("f2c8e1b7-4d9a-4e6f-c2b8-7a4d1e8f2c9b"),
                OrderId = new Guid("b8f2d4a7-3c5e-4b9f-a1d8-7e4c2f9b6a3d"),
                ProductId = new Guid("b6c7d8e9-f0a1-4b2c-3d4e-5f6a7b8c9d0e"), // Mock product ID
                Quantity = 1,
                Price = 299.99m
            },
            
            // Order 2 items
            new
            {
                Id = new Guid("e9d6a3f2-7c1b-4e8d-a6f3-2c7b1e9d6a3f"),
                OrderId = new Guid("e7c3a9d2-5f8b-4a1e-9c6d-2b7f4e8a1c5f"),
                ProductId = new Guid("a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d"),
                Quantity = 3,
                Price = 199.99m
            },
            new
            {
                Id = new Guid("c4b7f1e9-8a5d-4f2c-b7e1-9d5a8c4b7f1e"),
                OrderId = new Guid("e7c3a9d2-5f8b-4a1e-9c6d-2b7f4e8a1c5f"),
                ProductId = new Guid("f3e4d5c6-b7a8-4f9e-0d1c-2b3a4f5e6d7c"), // Mock product ID
                Quantity = 1,
                Price = 99.99m
            },
            
            // Order 3 items
            new
            {
                Id = new Guid("b1e8c5f9-6d2a-4e7b-c5f9-1a6d2b1e8c5f"),
                OrderId = new Guid("a4d8f2b7-6e1c-4f3a-b9d5-8c2e7a4f1b6d"),
                ProductId = new Guid("e7f8a9b0-c1d2-4e3f-4a5b-6c7d8e9f0a1b"), // Mock product ID
                Quantity = 1,
                Price = 1299.97m
            },
            
            // Order 4 items
            new
            {
                Id = new Guid("a7c2f8e4-9b1d-4a6c-f8e4-2b9d1a7c2f8e"),
                OrderId = new Guid("f1b6c9e4-2d7a-4e8f-c3b9-5f1d8a6e2c7b"),
                ProductId = new Guid("a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d"),
                Quantity = 1,
                Price = 149.99m
            },
            
            // Order 5 items
            new
            {
                Id = new Guid("f5a9d3c7-2e6b-4f1a-d3c7-9e2b6f5a9d3c"),
                OrderId = new Guid("c9e7b3f1-4a8d-4c2e-f7b4-1d9a5c8e3b6f"),
                ProductId = new Guid("b6c7d8e9-f0a1-4b2c-3d4e-5f6a7b8c9d0e"),
                Quantity = 2,
                Price = 375.00m
            }
        };

        modelBuilder.Entity<OrderItem>().HasData(orderItems);
    }
}
