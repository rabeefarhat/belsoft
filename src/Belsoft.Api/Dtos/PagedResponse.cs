namespace Belsoft.Api.Dtos;

// One page of any list. PageSize is the size actually used (after clamping), not the size requested.
public sealed record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    public int TotalPages => TotalCount == 0 ? 0 : (TotalCount + PageSize - 1) / PageSize;   // ①
}