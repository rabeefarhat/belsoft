namespace Belsoft.Api.Dtos;

// One row of GET /api/orders. Built inside the SQL projection: LineCount and Total are computed by SQL Server.
public sealed record OrderSummaryResponse(
    int Id,
    string CustomerName,
    string Status,
    DateTimeOffset CreatedAt,
    int LineCount,
    decimal Total);