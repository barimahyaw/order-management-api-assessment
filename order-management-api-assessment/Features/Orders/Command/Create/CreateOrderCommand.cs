using MediatR;
using order_management_api_assessment.Shared.Models;

namespace order_management_api_assessment.Features.Orders.Command.Create;

public record CreateOrderCommand(Guid CustomerId, List<OrderItemDto> OrderItems) : IRequest<ApiResponse<object>>;

public record OrderItemDto(Guid ProductId, int Quantity, decimal UnitPrice);