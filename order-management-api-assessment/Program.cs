using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using order_management_api_assessment.Features.Discounts;
using order_management_api_assessment.Features.Discounts.Services;
using order_management_api_assessment.Shared.Behaviors;
using order_management_api_assessment.Shared.Data;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<OrderManagementDbContext>(options =>
    options.UseInMemoryDatabase("OrderManagementDb"));

// MediatR with behaviors (order matters - validation first, then exception handling)
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>));
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ExceptionHandlingPipelineBehavior<,>));
});

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Memory Cache
builder.Services.AddMemoryCache();

// Discount Services
builder.Services.AddScoped<IDiscountRule, FirstTimeBuyerDiscount>();
builder.Services.AddScoped<IDiscountRule, BulkOrderDiscount>();
builder.Services.AddScoped<IDiscountRule, VipCustomerDiscount>();
builder.Services.AddScoped<IDiscountService, DiscountService>();

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Order Management API", Version = "v1" });
});


var app = builder.Build();

// Initialize database and seed data
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<OrderManagementDbContext>();
    try
    {
        dbContext.Database.EnsureCreated();
        app.Logger.LogInformation("Database initialized successfully with {OrderCount} orders and {CustomerCount} customers", 
            dbContext.Orders.Count(), dbContext.Customers.Count());
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Failed to initialize database");
        throw;
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
