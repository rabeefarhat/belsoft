using Belsoft.Api.Models;

namespace Belsoft.Api.Dtos;

public static class OrderMappings
{
    public static OrderResponse ToResponse(this Order order) =>
        new(
            order.Id,
            order.CustomerName,
            order.Status.ToString(),
            order.Total,
            [.. order.Lines.Select(line =>
                new OrderLineResponse(line.ProductCode, line.Quantity, line.UnitPrice, line.LineTotal))]);
}