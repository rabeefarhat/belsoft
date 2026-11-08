namespace Belsoft.Api.Models;

// The order and its rules. State changes only through Submit/Pay/Ship/Cancel.
// EF Core maps this class directly: private setters, the _lines backing field,
// and the private constructor below (constructor binding).
public sealed class Order
{
    private readonly List<OrderLine> _lines = [];

    // For new orders. Refuses an order that could never be valid.
    // The discount is decided BEFORE the order exists (by the pricing rule) and is frozen here.
    public Order(
        string customerName,
        IEnumerable<OrderLine> lines,
        decimal discount,
        string pricingRule,
        DateTimeOffset createdAt)
    {
        ArgumentNullException.ThrowIfNull(lines);
        ArgumentException.ThrowIfNullOrWhiteSpace(pricingRule);                         // ①

        CustomerName = customerName;
        _lines.AddRange(lines);

        if (_lines.Count == 0)
        {
            throw new DomainException("An order needs at least one line.");
        }

        // A broken pricing rule is a BUG, not a business refusal: 500, not 409.        // ②
        if (discount < 0m || discount > Subtotal)
        {
            throw new ArgumentOutOfRangeException(
                nameof(discount), discount, "A discount must be between zero and the order's subtotal.");
        }

        Discount = discount;
        PricingRule = pricingRule;
        Status = OrderStatus.Pending;
        CreatedAt = createdAt;
    }

    // For EF Core only: it calls this when loading a row. Lines and RowVersion are filled afterwards.
    private Order(string customerName, OrderStatus status, DateTimeOffset createdAt, decimal discount, string pricingRule) // ③
    {
        CustomerName = customerName;
        Status = status;
        CreatedAt = createdAt;
        Discount = discount;
        PricingRule = pricingRule;
    }

    public int Id { get; private set; }

    public string CustomerName
    {
        get;
        private set => field = string.IsNullOrWhiteSpace(value)
            ? throw new DomainException("An order needs a customer name.")
            : value.Trim();
    }

    public OrderStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    // Changed by SQL Server on every UPDATE. Read it, compare it, never set it in business code.
    public byte[] RowVersion { get; private set; } = [];

    // Frozen at creation: changing the configured rule later never re-prices this order.  // ④
    public decimal Discount { get; private set; }

    // Which rule produced Discount ("Standard", "Volume", …): the receipt's "why".
    public string PricingRule { get; private set; }

    public IReadOnlyList<OrderLine> Lines => _lines;

    public decimal Subtotal => _lines.Sum(line => line.LineTotal);                      // ⑤

    public decimal Total => Subtotal - Discount;

    public void Submit() => MoveTo(OrderStatus.Submitted);

    public void Pay() => MoveTo(OrderStatus.Paid);

    public void Ship() => MoveTo(OrderStatus.Shipped);

    public void Cancel() => MoveTo(OrderStatus.Cancelled);

    // The state machine (§2.5). Anything not listed is refused.
    private void MoveTo(OrderStatus target)
    {
        bool allowed = (this, target) switch
        {
            ({ Status: OrderStatus.Pending, Lines.Count: > 0 }, OrderStatus.Submitted) => true,
            ({ Status: OrderStatus.Submitted }, OrderStatus.Paid) => true,
            ({ Status: OrderStatus.Paid }, OrderStatus.Shipped) => true,
            ({ Status: OrderStatus.Pending or OrderStatus.Submitted }, OrderStatus.Cancelled) => true,
            _ => false,
        };

        if (!allowed)
        {
            throw new DomainException($"An order that is {Status} cannot become {target}.");
        }

        Status = target;
    }
}