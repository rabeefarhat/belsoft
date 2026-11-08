namespace Belsoft.Api.Pricing;

// List prices, no discount. The default, and what every order before L10 was priced with.
public sealed class StandardPricing : IPricingStrategy
{
    public const string Key = "Standard";

    public string Name => Key;

    public decimal CalculateDiscount(PricingInput input) => 0m;
}