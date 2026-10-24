namespace Belsoft.Api.Dtos;

// An explicit allow-list of settings safe to show. Never return the options class or IConfiguration itself.
public sealed record SettingsResponse(string Environment, int MaxItemsPerOrder);