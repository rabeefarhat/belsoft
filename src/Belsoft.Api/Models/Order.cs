namespace Belsoft.Api.Models;

public sealed record Order(int Id, string CustomerName, IReadOnlyList<OrderLine> Lines, OrderStatus Status)
{
    public decimal Total => Lines.Sum(line => line.LineTotal);
}