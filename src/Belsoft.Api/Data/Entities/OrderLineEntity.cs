namespace Belsoft.Api.Data.Entities;

// Storage shape of one dbo.OrderLines row. UnitPrice is the price snapshot taken when the order was placed.
public sealed class OrderLineEntity
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public required string ProductCode { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }
}