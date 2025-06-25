using MediatR;
using order_management_api_assessment.Shared.Enums;
using order_management_api_assessment.Shared.Models;

namespace order_management_api_assessment.Features.Orders.Command.UpdateStatus;

public record UpdateOrderStatusCommand(Guid OrderId, OrderStatus NewStatus) : IRequest<ApiResponse<object>>;
