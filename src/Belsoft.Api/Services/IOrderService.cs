using Belsoft.Api.Dtos;
using Belsoft.Api.Models;

namespace Belsoft.Api.Services;

public interface IOrderService
{
    Task<PagedResponse<OrderSummaryResponse>> GetPageAsync(OrderListQuery query, CancellationToken cancellationToken);

    Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<CreateOrderResult> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken);

    // Loads the order tracked, applies the transition, and saves only if the row is still at
    // expectedRowVersion. Returns null if the order doesn't exist.
    // Throws DomainException (illegal transition) or DbUpdateConcurrencyException (someone else saved first).
    Task<Order?> ChangeStatusAsync(
        int id,
        byte[] expectedRowVersion,
        Action<Order> transition,
        CancellationToken cancellationToken);
}