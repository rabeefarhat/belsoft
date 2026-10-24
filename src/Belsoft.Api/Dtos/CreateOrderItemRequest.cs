using System.ComponentModel.DataAnnotations;

namespace Belsoft.Api.Dtos;

public sealed record CreateOrderItemRequest
{
    [Required]
    [StringLength(20)]
    public required string ProductCode { get; init; }

    [Range(1, 1000)]
    public required int Quantity { get; init; }
}