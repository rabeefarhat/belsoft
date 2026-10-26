using Belsoft.Api.Models;

namespace Belsoft.Api.Data.Entities;

// Storage shape of one dbo.Orders row. L09 merges this with the Order model into a real entity.
public sealed class OrderEntity
{
    public int Id { get; set; }

    public required string CustomerName { get; set; }

    public OrderStatus Status { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public List<OrderLineEntity> Lines { get; set; } = [];
}