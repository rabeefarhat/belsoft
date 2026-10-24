using Belsoft.Api.Dtos;
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
        new(1, "Ada Lovelace", [new OrderLine("KB-01", 1, 49.90m)], OrderStatus.Pending),    // ①
        new(2, "Alan Turing", [new OrderLine("MON-27", 1, 120.00m)], OrderStatus.Paid),
        new(3, "Grace Hopper", [new OrderLine("MUG-01", 1, 15.25m)], OrderStatus.Pending),
    ];

    // Prices are decided by the server, never sent by the client. L13 replaces this with the catalog.
    private static readonly Dictionary<string, decimal> PriceList = new(StringComparer.OrdinalIgnoreCase) // ②
    {
        ["KB-01"] = 49.90m,
        ["MON-27"] = 120.00m,
        ["MUG-01"] = 15.25m,
    };

    // Requests run in parallel and share the static list; only one may touch it at a time.
    private static readonly Lock OrdersLock = new();                                        // ③

    private static int _lastId = 3;                                                         // ④

    [HttpGet]
    public ActionResult<IReadOnlyList<OrderResponse>> GetAll()
    {
        lock (OrdersLock)                                                                   // ⑤
        {
            return Ok(Orders.Select(order => order.ToResponse()).ToList());
        }
    }

    [HttpGet("{id:int}")]
    public ActionResult<OrderResponse> GetById(int id)
    {
        Order? order;
        lock (OrdersLock)
        {
            order = Orders.Find(o => o.Id == id);
        }

        if (order is null)
        {
            return NotFound();                                                              // ⑥
        }

        return Ok(order.ToResponse());
    }

    [HttpPost]                                                                              // ⑦
    [ProducesResponseType<OrderResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public ActionResult<OrderResponse> Create(CreateOrderRequest request)                   // ⑧
    {
        List<OrderLine> lines = [];

        for (int i = 0; i < request.Items.Count; i++)
        {
            CreateOrderItemRequest item = request.Items[i];

            if (item is null)
            {
                ModelState.AddModelError($"Items[{i}]", "Item is required.");
                continue;
            }

            if (!PriceList.TryGetValue(item.ProductCode, out decimal unitPrice))            // ⑨
            {
                ModelState.AddModelError($"Items[{i}].ProductCode", "Unknown product code.");
                continue;
            }

            lines.Add(new OrderLine(item.ProductCode.ToUpperInvariant(), item.Quantity, unitPrice));
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);                                           // ⑩
        }

        Order order;
        lock (OrdersLock)
        {
            order = new Order(++_lastId, request.CustomerName.Trim(), lines, OrderStatus.Pending); // ⑪
            Orders.Add(order);
        }

        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order.ToResponse()); // ⑫
    }
}