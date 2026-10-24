using Belsoft.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Belsoft.Api.Controllers;

[ApiController]                                                     // ①
[Route("api/[controller]")]                                         // ②
public sealed class OrdersController : ControllerBase               // ③
{
    // Temporary in-memory data. L04 moves this behind IOrderStore; L07 replaces it with SQL Server.
    private static readonly List<Order> Orders =                    // ④
    [
        new Order(1, "Ada Lovelace", 49.90m),                       // ⑤
        new Order(2, "Alan Turing", 120.00m),
        new Order(3, "Grace Hopper", 15.25m),
    ];

    [HttpGet]                                                       // ⑥
    public ActionResult<IReadOnlyList<Order>> GetAll()              // ⑦
    {
        return Ok(Orders);                                          // ⑧
    }

    [HttpGet("{id:int}")]                                           // ⑨
    public ActionResult<Order> GetById(int id)                      // ⑩
    {
        Order? order = Orders.Find(o => o.Id == id);                // ⑪

        if (order is null)                                          // ⑫
        {
            return NotFound();
        }

        return Ok(order);
    }

    // Async twin of GetAll, for comparison in this lesson only (removed in Step 8b).
    [HttpGet("async")]                                              // ⑬
    public async Task<ActionResult<IReadOnlyList<Order>>> GetAllAsync()
    {
        await Task.Delay(TimeSpan.FromMilliseconds(300));           // ⑭
        return Ok(Orders);
    }
}