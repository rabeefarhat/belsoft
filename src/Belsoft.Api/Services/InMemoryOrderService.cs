using Belsoft.Api.Configuration;
using Belsoft.Api.Dtos;
using Belsoft.Api.Models;
using Microsoft.Extensions.Options;

namespace Belsoft.Api.Services;

// Registered as a singleton: this one instance holds the orders for the whole app. L07 replaces it with SQL Server.
// IOptions (not IOptionsSnapshot): a singleton must not hold a scoped service.
public sealed class InMemoryOrderService(IOptions<BelsoftOptions> options) : IOrderService  // ①
{
    // Prices are decided by the server, never sent by the client. L13 replaces this with the catalog.
    private static readonly Dictionary<string, decimal> PriceList = new(StringComparer.OrdinalIgnoreCase)
    {
        ["KB-01"] = 49.90m,
        ["MON-27"] = 120.00m,
        ["MUG-01"] = 15.25m,
    };

    // Read once: IOptions never changes after startup, so there is nothing to gain by re-reading it.
    private readonly BelsoftOptions _settings = options.Value;                                 // ②

    private readonly List<Order> _orders =
    [
        new(1, "Ada Lovelace", [new OrderLine("KB-01", 1, 49.90m)], OrderStatus.Pending),
        new(2, "Alan Turing", [new OrderLine("MON-27", 1, 120.00m)], OrderStatus.Paid),
        new(3, "Grace Hopper", [new OrderLine("MUG-01", 1, 15.25m)], OrderStatus.Pending),
    ];

    // A singleton is shared by parallel requests; only one may touch the list at a time.
    private readonly Lock _ordersLock = new();

    private int _lastId = 3;

    public IReadOnlyList<Order> GetAll()
    {
        lock (_ordersLock)
        {
            return _orders.ToList();
        }
    }

    public Order? GetById(int id)
    {
        lock (_ordersLock)
        {
            return _orders.Find(order => order.Id == id);
        }
    }

    public CreateOrderResult Create(CreateOrderRequest request)
    {
        if (request.Items.Count > _settings.MaxItemsPerOrder)                                   // ③
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

            lines.Add(new OrderLine(item.ProductCode.ToUpperInvariant(), item.Quantity, unitPrice));
        }

        if (errors.Count > 0)
        {
            return CreateOrderResult.Failure(errors);
        }

        Order order;
        lock (_ordersLock)
        {
            order = new Order(++_lastId, request.CustomerName.Trim(), lines, OrderStatus.Pending);
            _orders.Add(order);
        }

        return CreateOrderResult.Success(order);
    }
}