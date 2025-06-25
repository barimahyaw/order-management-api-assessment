using MediatR;
using Microsoft.EntityFrameworkCore;
using order_management_api_assessment.Features.Discounts.Services;
using order_management_api_assessment.Shared.Data;
using order_management_api_assessment.Shared.Models;

namespace order_management_api_assessment.Features.Orders.Command.Create;

internal sealed class CreateOrderCommandHandler(
    OrderManagementDbContext dbContext,
    IDiscountService discountService,
    ILogger<CreateOrderCommandHandler> logger) : IRequestHandler<CreateOrderCommand, ApiResponse<object>>
{ 
    public async Task<ApiResponse<object>> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        var customer = await dbContext.Customers.FirstOrDefaultAsync(c => c.Id == command.CustomerId, cancellationToken);
        if (customer == null) 
        {
            logger.LogWarning("Customer with ID {CustomerId} not found when creating order", command.CustomerId);
            return ApiResponse.Error("Customer not found!");
        }

        var order = Order.Create(customer.Id);
        order.AddOrderItems([.. command.OrderItems]);

        var discountResult = discountService.CalculateBestDiscount(customer, [.. order.OrderItems]);
        order.ApplyDiscount(discountResult.Amount);

        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync(cancellationToken);        
        
        var discountMessage = discountResult.Amount > 0 && !string.IsNullOrEmpty(discountResult.DiscountType)
            ? $" with {discountResult.DiscountType}"
            : "";
        
        logger.LogInformation("Order {OrderId} successfully created for customer {CustomerId} with discount {DiscountAmount}", 
            order.OrderId, customer.Id, discountResult.Amount);
        
        return ApiResponse.Success<object>(new { order.OrderId }, 
            $"Order created successfully with ID: {order.OrderId}{discountMessage}");
    }
}