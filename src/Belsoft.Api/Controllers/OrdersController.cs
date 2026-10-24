using Belsoft.Api.Dtos;
using Belsoft.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Belsoft.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class OrdersController(IOrderService orderService) : ControllerBase        // ①
{
    [HttpGet]
    public ActionResult<IReadOnlyList<OrderResponse>> GetAll()
    {
        return Ok(orderService.GetAll().Select(order => order.ToResponse()).ToList());   // ②
    }

    [HttpGet("{id:int}")]
    public ActionResult<OrderResponse> GetById(int id)
    {
        var order = orderService.GetById(id);                                             // ③

        if (order is null)
        {
            return NotFound();
        }

        return Ok(order.ToResponse());
    }

    [HttpPost]
    [ProducesResponseType<OrderResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public ActionResult<OrderResponse> Create(CreateOrderRequest request)
    {
        CreateOrderResult result = orderService.Create(request);

        if (!result.Succeeded)                                                            // ④
        {
            foreach (var (field, message) in result.Errors)                               // ⑤
            {
                ModelState.AddModelError(field, message);
            }

            return ValidationProblem(ModelState);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Order.Id }, result.Order.ToResponse()); // ⑥
    }
}