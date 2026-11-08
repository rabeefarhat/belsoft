using Belsoft.Api.Configuration;
using Belsoft.Api.Data;
using Belsoft.Api.Dtos;
using Belsoft.Api.Models;
using Belsoft.Api.Pricing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Belsoft.Api.Services;

// Scoped: it holds the request's DbContext, so it must not outlive the request.
public sealed class EfOrderService(
    BelsoftDbContext db,
    IOptions<BelsoftOptions> options,
    TimeProvider timeProvider,
    IPricingStrategy pricing) : IOrderService                                                  // ①
{
    // Prices are decided by the server, never sent by the client. L13 replaces this with the catalog.
    private static readonly Dictionary<string, decimal> PriceList = new(StringComparer.OrdinalIgnoreCase)
    {
        ["KB-01"] = 49.90m,
        ["MON-27"] = 120.00m,
        ["MUG-01"] = 15.25m,
    };

    // Every list is bounded: a client may ASK for more, it never GETS more.
    private const int MaxPageSize = 50;

    private readonly BelsoftOptions _settings = options.Value;

    public async Task<PagedResponse<OrderSummaryResponse>> GetPageAsync(
        OrderListQuery query, CancellationToken cancellationToken)
    {
        int pageSize = Math.Min(query.PageSize, MaxPageSize);

        IQueryable<Order> orders = db.Orders
            .AsNoTracking()
            .TagWith("Orders list page");

        if (!string.IsNullOrWhiteSpace(query.Customer))
        {
            string customer = query.Customer.Trim();
            orders = orders.Where(order => order.CustomerName.Contains(customer));
        }

        if (query.Status is OrderStatus status)
        {
            orders = orders.WithStatus(status);
        }

        int totalCount = await orders.CountAsync(cancellationToken);

        IQueryable<Order> sorted = query.Sort switch
        {
            OrderSortBy.Oldest => orders.OrderBy(order => order.CreatedAt).ThenBy(order => order.Id),
            OrderSortBy.Customer => orders.OrderBy(order => order.CustomerName).ThenBy(order => order.Id),
            _ => orders.NewestFirst,
        };

        List<OrderSummaryResponse> items = await sorted
            .Paged(query.Page, pageSize)
            .Select(order => new OrderSummaryResponse(
                order.Id,
                order.CustomerName,
                order.Status.ToString(),
                order.CreatedAt,
                order.Lines.Count(),
                order.Lines.Sum(line => line.Quantity * line.UnitPrice) - order.Discount))      // ②
            .ToListAsync(cancellationToken);

        return new PagedResponse<OrderSummaryResponse>(items, query.Page, pageSize, totalCount);
    }

    public async Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        await db.Orders
            .AsNoTracking()
            .Include(order => order.Lines.OrderBy(line => line.Id))
            .FirstOrDefaultAsync(order => order.Id == id, cancellationToken);

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
        List<OrderLine> lines = [];

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

            lines.Add(new OrderLine(item.ProductCode, item.Quantity, unitPrice));
        }

        if (errors.Count > 0)
        {
            return CreateOrderResult.Failure(errors);
        }

        string customerName = request.CustomerName.Trim();

        // Loyalty needs history. Name matching is a placeholder until L21 gives orders an owner id.
        int previousOrderCount = await db.Orders
            .CountAsync(o => o.CustomerName == customerName && o.Status != OrderStatus.Cancelled, cancellationToken);

        // A fresh array: the strategy gets a snapshot nobody else can change.
        PricingInput input = new(
            [.. lines.Select(line => new PricingLine(line.ProductCode, line.Quantity, line.UnitPrice))],  // ③
            previousOrderCount);

        // The ONLY pricing line in checkout. Which rule runs is decided in Program.cs, not here.
        decimal discount = pricing.CalculateDiscount(input);                                    // ④

        var order = new Order(customerName, lines, discount, pricing.Name, timeProvider.GetUtcNow());

        db.Orders.Add(order);
        await db.SaveChangesAsync(cancellationToken);   // one transaction: 1 Orders row + N OrderLines rows

        return CreateOrderResult.Success(order);
    }

    public async Task<Order?> ChangeStatusAsync(
        int id,
        byte[] expectedRowVersion,
        Action<Order> transition,
        CancellationToken cancellationToken)
    {
        Order? order = await db.Orders
            .Include(o => o.Lines.OrderBy(line => line.Id))
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (order is null)
        {
            return null;
        }

        // Compare against the version the CLIENT saw, not the one we just loaded.
        db.Entry(order).Property(o => o.RowVersion).OriginalValue = expectedRowVersion;

        transition(order);

        await db.SaveChangesAsync(cancellationToken);

        return order;
    }
}