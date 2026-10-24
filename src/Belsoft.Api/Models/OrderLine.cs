namespace Belsoft.Api.Models;

public sealed record OrderLine(string ProductCode, int Quantity, decimal UnitPrice)
{
    public decimal LineTotal => Quantity * UnitPrice;
}