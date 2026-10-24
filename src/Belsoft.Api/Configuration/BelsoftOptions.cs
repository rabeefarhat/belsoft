using System.ComponentModel.DataAnnotations;

namespace Belsoft.Api.Configuration;

// Belsoft's business settings, bound from the "Belsoft" section and validated at startup.
public sealed class BelsoftOptions
{
    public const string SectionName = "Belsoft";

    // No default on purpose: a missing value binds as 0, fails the range, and the app refuses to start.
    // The upper bound matches the [MaxLength(50)] ceiling on CreateOrderRequest.Items.
    [Range(1, 50)]
    public int MaxItemsPerOrder { get; init; }
}