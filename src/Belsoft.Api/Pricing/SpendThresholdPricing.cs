namespace Belsoft.Api.Pricing;

// A flat 10.00 off baskets of 100.00 or more. The threshold exceeds the amount, so the
// discount can never exceed the subtotal.
public sealed class SpendThresholdPricing : IPricingStrategy
{
    public const string Key = "SpendThreshold";

    private const decimal Threshold = 100.00m;
    private const decimal AmountOff = 10.00m;

    public string Name => Key;

    public decimal CalculateDiscount(PricingInput input) =>
        input.Subtotal >= Threshold ? AmountOff : 0m;
}