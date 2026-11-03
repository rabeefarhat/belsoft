using System.ComponentModel.DataAnnotations;
using Belsoft.Api.Models;

namespace Belsoft.Api.Dtos;

// Bound from the query string: GET /api/orders?customer=&status=&sort=&page=&pageSize=
public sealed class OrderListQuery
{
    [StringLength(100)]                                   // ①
    public string? Customer { get; set; }

    public OrderStatus? Status { get; set; }              // ②

    public OrderSortBy Sort { get; set; } = OrderSortBy.Newest;

    [Range(1, 10_000)]                                    // ③
    public int Page { get; set; } = 1;

    [Range(1, int.MaxValue)]                              // ④
    public int PageSize { get; set; } = 20;
}