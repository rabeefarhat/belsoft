namespace Belsoft.Api.Dtos;

public sealed record OrderResponse(
    int Id,
    string CustomerName,
    string Status,
    decimal Total,
    IReadOnlyList<OrderLineResponse> Lines);