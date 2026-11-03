using Belsoft.Api.Dtos;
using Belsoft.Api.Models;

namespace Belsoft.Api.Services;

public interface IOrderService
{
    Task<PagedResponse<OrderSummaryResponse>> GetPageAsync(OrderListQuery query, CancellationToken cancellationToken);

    Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<CreateOrderResult> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken);
}