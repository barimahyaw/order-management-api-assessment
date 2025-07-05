using Microsoft.EntityFrameworkCore;
using order_management_api_assessment.Data;
using order_management_api_assessment.Services;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<OrderContext>(options =>
    options.UseInMemoryDatabase("OrderManagementDb"));

// Services
builder.Services.AddScoped<DiscountService>();
builder.Services.AddScoped<OrderService>();

// Controllers
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
    var context = scope.ServiceProvider.GetRequiredService<OrderContext>();
    try
    {
        context.Database.EnsureCreated();
        await SeedData.InitializeAsync(context);
        app.Logger.LogInformation("Database initialized successfully with {OrderCount} orders and {CustomerCount} customers",
            context.Orders.Count(), context.Customers.Count());
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Failed to initialize database");
        throw;
    }
}

// Configure the HTTP request pipeline
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