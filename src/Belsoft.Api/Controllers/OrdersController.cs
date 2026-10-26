using Belsoft.Api.Dtos;
using Belsoft.Api.Models;
using Belsoft.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Belsoft.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class OrdersController(IOrderService orderService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OrderResponse>>> GetAll()                  // ①
    {
        IReadOnlyList<Order> orders = await orderService.GetAllAsync();

        return Ok(orders.Select(order => order.ToResponse()).ToList());
    }

    // Action names deliberately have no "Async" suffix: CreatedAtAction(nameof(GetById)) must match the route name.
    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderResponse>> GetById(int id)                          // ②
    {
        Order? order = await orderService.GetByIdAsync(id);

        if (order is null)
        {
            return NotFound();
        }

        return Ok(order.ToResponse());
    }

    [HttpPost]
    [ProducesResponseType<OrderResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrderResponse>> Create(CreateOrderRequest request)
    {
        CreateOrderResult result = await orderService.CreateAsync(request);

        if (!result.Succeeded)
        {
            foreach (var (field, message) in result.Errors)
            {
                ModelState.AddModelError(field, message);
            }

            return ValidationProblem(ModelState);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Order.Id }, result.Order.ToResponse()); // ③
    }
}