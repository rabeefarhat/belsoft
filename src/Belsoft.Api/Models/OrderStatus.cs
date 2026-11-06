namespace Belsoft.Api.Models;

// Stored as text (HasConversion<string>), so the ORDER of these members never reaches the database.
// "Pending" plays the role §2.5 calls "Draft": created, not yet submitted. Kept for .http history (L03, L08).
public enum OrderStatus
{
    Pending,
    Submitted,
    Paid,
    Shipped,
    Cancelled,
}