namespace Belsoft.Api.Pricing;

// One way of pricing an order. Implementations must be pure: same input, same answer,
// no I/O, no clock, and a result between 0 and input.Subtotal (Order refuses anything else).
public interface IPricingStrategy
{
    // Stored on the order as its PricingRule, and used as the DI key.
    string Name { get; }

    decimal CalculateDiscount(PricingInput input);
}