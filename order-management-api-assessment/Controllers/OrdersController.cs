using Microsoft.AspNetCore.Mvc;
using order_management_api_assessment.DTOs;
using order_management_api_assessment.Services;

namespace order_management_api_assessment.Controllers;

[Route("api/orders")]
[ApiController]
[Produces("application/json")]
public class OrdersController(OrderService orderService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var (Success, Message, OrderId) = await orderService.CreateOrderAsync(request);

        if (!Success)
        {
            return BadRequest(Message);
        }

        return CreatedAtAction(nameof(GetOrder), new { orderId = OrderId }, new { message = Message });
    }

    [HttpPut("{orderId:guid}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateOrderStatus(Guid orderId, [FromBody] UpdateOrderStatusRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var (Success, Message) = await orderService.UpdateOrderStatusAsync(orderId, request.NewStatus);

        if (!Success)
        {
            return BadRequest(Message);
        }

        return Ok(new { message = Message });
    }

    [HttpGet("{orderId:guid}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOrder(Guid orderId)
    {
        var order = await orderService.GetOrderAsync(orderId);

        if (order == null)
        {
            return NotFound(new { message = "Order not found" });
        }

        return Ok(order);
    }

    [HttpGet]
    [ProducesResponseType(typeof(OrdersPagedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? status = null)
    {
        var result = await orderService.GetOrdersAsync(page, pageSize, status);
        return Ok(result);
    }

    [HttpGet("analytics")]
    [ProducesResponseType(typeof(OrderAnalyticsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOrderAnalytics()
    {
        var result = await orderService.GetOrderAnalyticsAsync();
        return Ok(result);
    }
}