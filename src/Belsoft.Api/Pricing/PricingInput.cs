namespace Belsoft.Api.Pricing;

// Everything a pricing rule may look at. Immutable: a rule reads the basket, it never changes it.
// Callers must pass a list nobody else holds (checkout passes a fresh array), because a record is
// only shallowly immutable.
public sealed record PricingInput(IReadOnlyList<PricingLine> Lines, int PreviousOrderCount)
{
    public decimal Subtotal => Lines.Sum(line => line.Quantity * line.UnitPrice);

    public int TotalQuantity => Lines.Sum(line => line.Quantity);
}