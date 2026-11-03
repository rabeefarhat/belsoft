using Belsoft.Api.Dtos;
using Belsoft.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Belsoft.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class OrdersController(IOrderService orderService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedResponse<OrderSummaryResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponse<OrderSummaryResponse>>> GetAll(
        [FromQuery] OrderListQuery query,                                                       // ①
        CancellationToken cancellationToken)                                                    // ②
    {
        PagedResponse<OrderSummaryResponse> page = await orderService.GetPageAsync(query, cancellationToken);

        return Ok(page);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var order = await orderService.GetByIdAsync(id, cancellationToken);

        if (order is null)
        {
            return NotFound();
        }

        return Ok(order.ToResponse());
    }

    [HttpPost]
    [ProducesResponseType<OrderResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrderResponse>> Create(
        CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        CreateOrderResult result = await orderService.CreateAsync(request, cancellationToken);

        if (!result.Succeeded)
        {
            foreach (var (field, message) in result.Errors)
            {
                ModelState.AddModelError(field, message);
            }

            return ValidationProblem(ModelState);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Order.Id }, result.Order.ToResponse());
    }
}