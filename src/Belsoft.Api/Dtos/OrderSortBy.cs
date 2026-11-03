namespace Belsoft.Api.Dtos;

// The only sort orders the list endpoint accepts. Each one ends with Id as a tie-breaker in the query,
// so page boundaries are stable.
public enum OrderSortBy
{
    Newest,
    Oldest,
    Customer,
}