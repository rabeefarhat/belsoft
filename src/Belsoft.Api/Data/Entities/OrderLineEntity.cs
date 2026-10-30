namespace Belsoft.Api.Data.Entities;

// One row of the OrderLines table.
public sealed class OrderLineEntity
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public required string ProductCode { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }
}