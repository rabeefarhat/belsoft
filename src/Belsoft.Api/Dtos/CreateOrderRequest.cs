using System.ComponentModel.DataAnnotations;

namespace Belsoft.Api.Dtos;

public sealed record CreateOrderRequest
{
    [Required]
    [StringLength(100)]
    public required string CustomerName { get; init; }

    [Required]
    [MinLength(1, ErrorMessage = "An order needs at least one item.")]
    [MaxLength(50, ErrorMessage = "An order can have at most 50 items.")]
    public required IReadOnlyList<CreateOrderItemRequest> Items { get; init; }
}