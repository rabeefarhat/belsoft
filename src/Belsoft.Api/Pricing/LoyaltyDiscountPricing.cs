namespace Belsoft.Api.Pricing;

// 5% off for customers with 3 or more earlier, non-cancelled orders.
public sealed class LoyaltyDiscountPricing : IPricingStrategy
{
    public const string Key = "Loyalty";

    private const int MinimumPreviousOrders = 3;
    private const decimal Rate = 0.05m;

    public string Name => Key;

    public decimal CalculateDiscount(PricingInput input) =>
        input.PreviousOrderCount >= MinimumPreviousOrders
            ? decimal.Round(input.Subtotal * Rate, 2, MidpointRounding.AwayFromZero)
            : 0m;
}