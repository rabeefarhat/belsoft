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
    TimeProvider timeProvider) : IOrderService
{
    // Prices are decided by the server, never sent by the client. L13 replaces this with the catalog.
    private static readonly Dictionary<string, decimal> PriceList = new(StringComparer.OrdinalIgnoreCase)
    {
        ["KB-01"] = 49.90m,
        ["MON-27"] = 120.00m,
        ["MUG-01"] = 15.25m,
    };

    // Every list is bounded: a client may ASK for more, it never GETS more.
    private const int MaxPageSize = 50;                                                         // ①

    private readonly BelsoftOptions _settings = options.Value;

    public async Task<PagedResponse<OrderSummaryResponse>> GetPageAsync(
        OrderListQuery query, CancellationToken cancellationToken)
    {
        int pageSize = Math.Min(query.PageSize, MaxPageSize);                                   // ②

        IQueryable<OrderEntity> orders = db.Orders
            .AsNoTracking()                                                                     // ③
            .TagWith("Orders list page");                                                       // ④

        if (!string.IsNullOrWhiteSpace(query.Customer))
        {
            string customer = query.Customer.Trim();
            orders = orders.Where(order => order.CustomerName.Contains(customer));              // ⑤
        }

        if (query.Status is OrderStatus status)                                                 // ⑥
        {
            orders = orders.WithStatus(status);
        }

        int totalCount = await orders.CountAsync(cancellationToken);                            // ⑦

        IQueryable<OrderEntity> sorted = query.Sort switch                                      // ⑧
        {
            OrderSortBy.Oldest => orders.OrderBy(order => order.CreatedAt).ThenBy(order => order.Id),
            OrderSortBy.Customer => orders.OrderBy(order => order.CustomerName).ThenBy(order => order.Id),
            _ => orders.NewestFirst,
        };

        List<OrderSummaryResponse> items = await sorted
            .Paged(query.Page, pageSize)
            .Select(order => new OrderSummaryResponse(                                          // ⑨
                order.Id,
                order.CustomerName,
                order.Status.ToString(),
                order.CreatedAt,
                order.Lines.Count,
                order.Lines.Sum(line => line.Quantity * line.UnitPrice)))
            .ToListAsync(cancellationToken);                                                    // ⑩

        return new PagedResponse<OrderSummaryResponse>(items, query.Page, pageSize, totalCount);
    }

    public async Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        OrderEntity? entity = await db.Orders
            .AsNoTracking()                                                                     // ⑪
            .Include(order => order.Lines)
            .FirstOrDefaultAsync(order => order.Id == id, cancellationToken);

        return entity is null ? null : ToModel(entity);
    }

    public async Task<CreateOrderResult> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken)
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
            CreatedAt = timeProvider.GetUtcNow(),
            Lines = lines,
        };

        db.Orders.Add(entity);                                    // state: Added, temporary key
        await db.SaveChangesAsync(cancellationToken);             // INSERTs in one transaction; state: Unchanged  // ⑫

        return CreateOrderResult.Success(ToModel(entity));
    }

    // Persistence shape → the API's order record. Lines sorted by Id so the response order is stable.
    private static Order ToModel(OrderEntity entity) =>
        new(
            entity.Id,
            entity.CustomerName,
            [.. entity.Lines
                .OrderBy(line => line.Id)
                .Select(line => new OrderLine(line.ProductCode, line.Quantity, line.UnitPrice))],
            entity.Status);
}