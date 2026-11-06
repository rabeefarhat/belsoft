using Belsoft.Api.Dtos;
using Belsoft.Api.Http;
using Belsoft.Api.Models;
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
        [FromQuery] OrderListQuery query,
        CancellationToken cancellationToken)
    {
        PagedResponse<OrderSummaryResponse> page = await orderService.GetPageAsync(query, cancellationToken);

        return Ok(page);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<OrderResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        Order? order = await orderService.GetByIdAsync(id, cancellationToken);

        if (order is null)
        {
            return NotFound();
        }

        Response.Headers.ETag = OrderETag.From(order.RowVersion);                             // ①
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

        Response.Headers.ETag = OrderETag.From(result.Order.RowVersion);
        return CreatedAtAction(nameof(GetById), new { id = result.Order.Id }, result.Order.ToResponse());
    }

    [HttpPost("{id:int}/submit")]                                                             // ②
    [ProducesResponseType<OrderResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status428PreconditionRequired)]
    public Task<ActionResult<OrderResponse>> Submit(
        int id,
        [FromHeader(Name = "If-Match")] string? ifMatch,                                      // ③
        CancellationToken cancellationToken) =>
        ApplyTransitionAsync(id, ifMatch, order => order.Submit(), cancellationToken);       // ④

    [HttpPost("{id:int}/pay")]
    [ProducesResponseType<OrderResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status428PreconditionRequired)]
    public Task<ActionResult<OrderResponse>> Pay(
        int id,
        [FromHeader(Name = "If-Match")] string? ifMatch,
        CancellationToken cancellationToken) =>
        ApplyTransitionAsync(id, ifMatch, order => order.Pay(), cancellationToken);

    [HttpPost("{id:int}/ship")]
    [ProducesResponseType<OrderResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status428PreconditionRequired)]
    public Task<ActionResult<OrderResponse>> Ship(
        int id,
        [FromHeader(Name = "If-Match")] string? ifMatch,
        CancellationToken cancellationToken) =>
        ApplyTransitionAsync(id, ifMatch, order => order.Ship(), cancellationToken);

    [HttpPost("{id:int}/cancel")]
    [ProducesResponseType<OrderResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status428PreconditionRequired)]
    public Task<ActionResult<OrderResponse>> Cancel(
        int id,
        [FromHeader(Name = "If-Match")] string? ifMatch,
        CancellationToken cancellationToken) =>
        ApplyTransitionAsync(id, ifMatch, order => order.Cancel(), cancellationToken);

    // Not an action (private). The four transitions share every step except which method they call.
    private async Task<ActionResult<OrderResponse>> ApplyTransitionAsync(
        int id, string? ifMatch, Action<Order> transition, CancellationToken cancellationToken)
    {
        if (ifMatch is null)                                                                  // ⑤
        {
            return Problem(
                statusCode: StatusCodes.Status428PreconditionRequired,
                title: "If-Match header required.",
                detail: "Send the ETag from GET /api/orders/{id} in an If-Match header.");
        }

        if (!OrderETag.TryParse(ifMatch, out byte[]? rowVersion))                             // ⑥
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid If-Match header.",
                detail: "If-Match must be one strong ETag exactly as returned by this API.");
        }

        Order? order = await orderService.ChangeStatusAsync(id, rowVersion, transition, cancellationToken);

        if (order is null)
        {
            return NotFound();
        }

        Response.Headers.ETag = OrderETag.From(order.RowVersion);                             // ⑦
        return Ok(order.ToResponse());
    }
}