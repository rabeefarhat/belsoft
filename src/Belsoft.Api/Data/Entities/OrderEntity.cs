using Belsoft.Api.Models;

namespace Belsoft.Api.Data.Entities;

// One row of the Orders table. EF Core fills it from rows and writes it back; the API never returns it directly.
public sealed class OrderEntity
{
    public int Id { get; set; }                                   // ①

    public required string CustomerName { get; set; }             // ②

    public OrderStatus Status { get; set; }                       // ③

    public DateTimeOffset CreatedAt { get; set; }                 // ④

    public List<OrderLineEntity> Lines { get; set; } = [];        // ⑤
}