using Belsoft.Api.Configuration;
using Belsoft.Api.Data;
using Belsoft.Api.Data.Entities;
using Belsoft.Api.Dtos;
using Belsoft.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Belsoft.Api.Services;

// Scoped: it holds the request's DbContext, so it must not outlive the request.
public sealed class EfOrderService(
    BelsoftDbContext db,
    IOptions<BelsoftOptions> options,
    TimeProvider timeProvider) : IOrderService                                                 // ①
{
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
        // Unbounded for now: L08 adds paging with a maximum page size.
        List<OrderEntity> entities = await db.Orders
            .Include(order => order.Lines)                                                      // ②
            .OrderBy(order => order.Id)
            .ToListAsync();                                                                     // ③

        return [.. entities.Select(ToModel)];
    }

    public async Task<Order?> GetByIdAsync(int id)
    {
        OrderEntity? entity = await db.Orders
            .Include(order => order.Lines)
            .FirstOrDefaultAsync(order => order.Id == id);                                     // ④

        return entity is null ? null : ToModel(entity);
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

        if (errors.Count > 0)
        {
            return CreateOrderResult.Failure(errors);
        }

        var entity = new OrderEntity
        {
            CustomerName = request.CustomerName.Trim(),
            Status = OrderStatus.Pending,
            CreatedAt = timeProvider.GetUtcNow(),                                               // ⑤
            Lines = lines,
        };

        db.Orders.Add(entity);                    // state: Added, entity.Id == 0                // ⑥
        await db.SaveChangesAsync();              // INSERTs in one transaction; state: Unchanged, Id filled  // ⑦

        return CreateOrderResult.Success(ToModel(entity));
    }

    // Persistence shape → the API's order record. Lines sorted by Id so the response order is stable.
    private static Order ToModel(OrderEntity entity) =>                                        // ⑧
        new(
            entity.Id,
            entity.CustomerName,
            [.. entity.Lines
                .OrderBy(line => line.Id)
                .Select(line => new OrderLine(line.ProductCode, line.Quantity, line.UnitPrice))],
            entity.Status);
}