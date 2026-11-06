namespace Belsoft.Api.Models;

// One product line of an order. Created only through the constructor, which refuses invalid lines.
// EF Core also uses this constructor when it loads a row (constructor binding), so its rules must
// match the CHECK constraints: the database can never hold a row that would make loading fail.
public sealed class OrderLine
{
    public OrderLine(string productCode, int quantity, decimal unitPrice)          // ①
    {
        if (string.IsNullOrWhiteSpace(productCode))
        {
            throw new DomainException("A line needs a product code.");
        }

        if (quantity is < 1 or > 1000)                                             // ②
        {
            throw new DomainException("Quantity must be between 1 and 1000.");
        }

        if (unitPrice < 0m)
        {
            throw new DomainException("Unit price cannot be negative.");
        }

        ProductCode = productCode.Trim().ToUpperInvariant();                        // ③
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public int Id { get; private set; }                                             // ④

    public string ProductCode { get; private set; }

    public int Quantity { get; private set; }

    public decimal UnitPrice { get; private set; }

    public decimal LineTotal => Quantity * UnitPrice;                               // ⑤
}