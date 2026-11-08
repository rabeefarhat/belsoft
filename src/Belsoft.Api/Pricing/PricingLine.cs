namespace Belsoft.Api.Pricing;

// A line as pricing sees it: numbers only, no entity, no database.
public sealed record PricingLine(string ProductCode, int Quantity, decimal UnitPrice);