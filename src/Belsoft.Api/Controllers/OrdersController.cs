using Belsoft.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Belsoft.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class OrdersController : ControllerBase
{
    // Temporary in-memory data. L04 moves this behind IOrderStore; L07 replaces it with SQL Server.
    // Static because a new controller instance is created for every request.
    private static readonly List<Order> Orders =
    [
        new Order(1, "Ada Lovelace", 49.90m),
        new Order(2, "Alan Turing", 120.00m),
        new Order(3, "Grace Hopper", 15.25m),
    ];

    [HttpGet]
    public ActionResult<IReadOnlyList<Order>> GetAll()
    {
        return Ok(Orders);
    }

    [HttpGet("{id:int}")]
    public ActionResult<Order> GetById(int id)
    {
        Order? order = Orders.Find(o => o.Id == id);

        if (order is null)
        {
            return NotFound();
        }

        return Ok(order);
    }
}