using Belsoft.Api.Configuration;
using Belsoft.Api.Data;
using Belsoft.Api.Data.Entities;
using Belsoft.Api.Dtos;
using Belsoft.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Belsoft.Api.Services;

// Scoped: one instance per request, sharing that request's DbContext. No lock: nothing here is shared between requests.
public sealed class EfOrderService(
    BelsoftDbContext db,
    IOptions<BelsoftOptions> options,
    TimeProvider clock) : IOrderService                                                      // ①
{
    // Temporary safety cap until L08 adds real paging: a list endpoint must never return an unbounded table.
    private const int MaxOrdersReturned = 100;                                               // ②

    // Prices are decided by the server, never sent by the client. L13 replaces this with the catalog.
    private static readonly Dictionary<string, decimal> PriceList = new(StringComparer.OrdinalIgnoreCase)
    {
        ["KB-01"] = 49.90m,
        ["MON-27"] = 120.00m,
        ["MUG-01"] = 15.25m,
    };

    private readonly BelsoftOptions _settings = options.Value;

    public async Task<IReadOnlyList<Order>> GetAllAsync()
    {
        List<OrderEntity> orders = await db.Orders                                           // ③
            .AsNoTracking()                                                                  // ④
            .Include(order => order.Lines)                                                   // ⑤
            .OrderBy(order => order.Id)
            .Take(MaxOrdersReturned)
            .ToListAsync();                                                                  // ⑥

        return [.. orders.Select(ToModel)];
    }

    public async Task<Order?> GetByIdAsync(int id)
    {
        OrderEntity? order = await db.Orders
            .AsNoTracking()
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == id);                                           // ⑦

        return order is null ? null : ToModel(order);
    }

    public async Task<CreateOrderResult> CreateAsync(CreateOrderRequest request)
    {
        if (request.Items.Count > _settings.MaxItemsPerOrder)
        {
            return CreateOrderResult.Failure(new Dictionary<string, string>
            {
                ["Items"] = $"An order can have at most {_settings.MaxItemsPerOrder} items.",
            });
        }

        Dictionary<string, string> errors = new();
        List<OrderLineEntity> lines = [];

        for (int i = 0; i < request.Items.Count; i++)
        {
            CreateOrderItemRequest item = request.Items[i];

            if (item is null)
            {
                errors[$"Items[{i}]"] = "Item is required.";
                continue;
            }

            if (!PriceList.TryGetValue(item.ProductCode, out decimal unitPrice))
            {
                errors[$"Items[{i}].ProductCode"] = "Unknown product code.";
                continue;
            }

            lines.Add(new OrderLineEntity
            {
                ProductCode = item.ProductCode.ToUpperInvariant(),
                Quantity = item.Quantity,
                UnitPrice = unitPrice,
            });
        }

        // Validation failed: return before touching the database, so a bad request never costs a round trip.
        if (errors.Count > 0)                                                                // ⑧
        {
            return CreateOrderResult.Failure(errors);
        }

        OrderEntity order = new()
        {
            CustomerName = request.CustomerName.Trim(),
            Status = OrderStatus.Pending,
            CreatedAt = clock.GetUtcNow(),                                                   // ⑨
            Lines = lines,
        };

        db.Orders.Add(order);                                                                // ⑩
        await db.SaveChangesAsync();                                                         // ⑪

        return CreateOrderResult.Success(ToModel(order));
    }

    // Converts the storage shape into the API's Order record. L09 makes this unnecessary.
    private static Order ToModel(OrderEntity order) =>                                       // ⑫
        new(
            order.Id,
            order.CustomerName,
            [.. order.Lines
                .OrderBy(line => line.Id)
                .Select(line => new OrderLine(line.ProductCode, line.Quantity, line.UnitPrice))],
            order.Status);
}