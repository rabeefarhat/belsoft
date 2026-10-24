namespace Belsoft.Api.Dtos;

public sealed record OrderLineResponse(string ProductCode, int Quantity, decimal UnitPrice, decimal LineTotal);