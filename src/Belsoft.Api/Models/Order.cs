namespace Belsoft.Api.Models;

// The order and its rules. State changes only through Submit/Pay/Ship/Cancel.
// EF Core maps this class directly: private setters, the _lines backing field,
// and the private constructor below (constructor binding).
public sealed class Order
{
    private readonly List<OrderLine> _lines = [];                                       // ①

    // For new orders. Refuses an order that could never be valid.
    public Order(string customerName, IEnumerable<OrderLine> lines, DateTimeOffset createdAt)
    {
        ArgumentNullException.ThrowIfNull(lines);                                       // ②

        CustomerName = customerName;                                                    // ③
        _lines.AddRange(lines);

        if (_lines.Count == 0)
        {
            throw new DomainException("An order needs at least one line.");
        }

        Status = OrderStatus.Pending;                                                   // ④
        CreatedAt = createdAt;
    }

    // For EF Core only: it calls this when loading a row. Lines and RowVersion are filled afterwards.
    private Order(string customerName, OrderStatus status, DateTimeOffset createdAt)    // ⑤
    {
        CustomerName = customerName;
        Status = status;
        CreatedAt = createdAt;
    }

    public int Id { get; private set; }

    public string CustomerName                                                          // ⑥
    {
        get;
        private set => field = string.IsNullOrWhiteSpace(value)
            ? throw new DomainException("An order needs a customer name.")
            : value.Trim();
    }

    public OrderStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    // Changed by SQL Server on every UPDATE. Read it, compare it, never set it in business code.
    public byte[] RowVersion { get; private set; } = [];                                // ⑦

    public IReadOnlyList<OrderLine> Lines => _lines;                                    // ⑧

    public decimal Total => _lines.Sum(line => line.LineTotal);

    public void Submit() => MoveTo(OrderStatus.Submitted);                              // ⑨

    public void Pay() => MoveTo(OrderStatus.Paid);

    public void Ship() => MoveTo(OrderStatus.Shipped);

    public void Cancel() => MoveTo(OrderStatus.Cancelled);

    // The state machine (§2.5). Anything not listed is refused.
    private void MoveTo(OrderStatus target)
    {
        bool allowed = (this, target) switch                                            // ⑩
        {
            ({ Status: OrderStatus.Pending, Lines.Count: > 0 }, OrderStatus.Submitted) => true,
            ({ Status: OrderStatus.Submitted }, OrderStatus.Paid) => true,
            ({ Status: OrderStatus.Paid }, OrderStatus.Shipped) => true,
            ({ Status: OrderStatus.Pending or OrderStatus.Submitted }, OrderStatus.Cancelled) => true,
            _ => false,                                                                 // ⑪
        };

        if (!allowed)
        {
            throw new DomainException($"An order that is {Status} cannot become {target}.");
        }

        Status = target;
    }
}