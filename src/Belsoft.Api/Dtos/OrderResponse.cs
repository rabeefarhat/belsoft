namespace Belsoft.Api.Dtos;

// Total = Subtotal − Discount. PricingRule tells the client WHY it costs that.
public sealed record OrderResponse(
    int Id,
    string CustomerName,
    string Status,
    decimal Subtotal,
    decimal Discount,
    string PricingRule,
    decimal Total,
    IReadOnlyList<OrderLineResponse> Lines);