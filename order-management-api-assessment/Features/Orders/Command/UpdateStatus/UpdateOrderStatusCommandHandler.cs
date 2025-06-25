using MediatR;
using Microsoft.EntityFrameworkCore;
using order_management_api_assessment.Shared.Data;
using order_management_api_assessment.Shared.Models;

namespace order_management_api_assessment.Features.Orders.Command.UpdateStatus;

internal sealed class UpdateOrderStatusCommandHandler(
    OrderManagementDbContext dbContext,
    ILogger<UpdateOrderStatusCommandHandler> logger) 
    : IRequestHandler<UpdateOrderStatusCommand, ApiResponse<object>>
{
    public async Task<ApiResponse<object>> Handle(UpdateOrderStatusCommand command, CancellationToken cancellationToken)
    {
        var order = await dbContext.Orders.FirstOrDefaultAsync(o => o.OrderId == command.OrderId, cancellationToken);
        if (order == null) 
        {
            logger.LogWarning("Order with ID {OrderId} not found when updating status", command.OrderId);
            return ApiResponse.Error("Order not found!");
        }

        order.UpdateStatus(command.NewStatus);
        await dbContext.SaveChangesAsync(cancellationToken);
        
        logger.LogInformation("Order {OrderId} status successfully updated to {NewStatus}", command.OrderId, command.NewStatus);
        return ApiResponse.Success<object>(new { OrderId = command.OrderId, NewStatus = command.NewStatus }, 
            $"Order status successfully updated to {command.NewStatus}");
    }
}
