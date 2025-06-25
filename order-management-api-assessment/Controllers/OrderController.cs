using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using order_management_api_assessment.Features.Orders.Command.Create;
using order_management_api_assessment.Features.Orders.Command.UpdateStatus;
using order_management_api_assessment.Features.Orders.Query.GetOrder;
using order_management_api_assessment.Features.Orders.Query.GetOrderAnalytics;
using order_management_api_assessment.Features.Orders.Query.GetOrders;
using order_management_api_assessment.Shared.Enums;
using order_management_api_assessment.Shared.Models;

namespace order_management_api_assessment.Controllers
{
    [Route("api/orders")]
    [ApiController]
    [Produces("application/json")]
    public class OrderController(ISender sender) : ControllerBase
    {
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            //// Check ASP.NET Core model validation first
            //if (!ModelState.IsValid)
            //{
            //    var errors = ModelState
            //        .Where(x => x.Value?.Errors.Count > 0)
            //        .SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage))
            //        .ToList();
                
            //    return BadRequest(ApiResponse.Error(errors));
            //}

            var command = new CreateOrderCommand(request.CustomerId, request.OrderItems);
            var result = await sender.Send(command);

            return result.Success
                ? CreatedAtAction(nameof(CreateOrder), result)
                : BadRequest(result);
        }       
        
        [HttpPut("{orderId:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateOrderStatus(Guid orderId, [FromBody] UpdateOrderStatusRequest request)
        {
            var command = new UpdateOrderStatusCommand(orderId, request.NewStatus);
            var result = await sender.Send(command);

            return result.Success
                ? Ok(result)
                : BadRequest(result);
        }

        [HttpGet("{orderId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<OrderResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetOrder(Guid orderId)
        {
            var query = new GetOrderQuery(orderId);
            var result = await sender.Send(query);

            return result.Success
                ? Ok(result)
                : NotFound(result);
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<OrdersPagedResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? status = null)
        {
            var query = new GetOrdersQuery(page, pageSize, status);
            var result = await sender.Send(query);

            return Ok(result);
        }

        [HttpGet("analytics")]
        [ProducesResponseType(typeof(ApiResponse<OrderAnalyticsResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetOrderAnalytics()
        {
            var query = new GetOrderAnalyticsQuery();
            var result = await sender.Send(query);

            return Ok(result);
        }
    }

    // Request DTOs
    public record CreateOrderRequest(
        [Required] Guid CustomerId, 
        [Required, MinLength(1)] List<OrderItemDto> OrderItems);
    public record UpdateOrderStatusRequest([Required] OrderStatus NewStatus);
}
