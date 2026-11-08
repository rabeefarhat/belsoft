namespace Belsoft.Api.Pricing;

// 10% off when the basket holds 10 or more units in total.
public sealed class VolumeDiscountPricing : IPricingStrategy
{
    public const string Key = "Volume";

    private const int MinimumUnits = 10;                                  // ①
    private const decimal Rate = 0.10m;

    public string Name => Key;

    public decimal CalculateDiscount(PricingInput input) =>               // ②
        input.TotalQuantity >= MinimumUnits
            ? decimal.Round(input.Subtotal * Rate, 2, MidpointRounding.AwayFromZero)  // ③
            : 0m;
}