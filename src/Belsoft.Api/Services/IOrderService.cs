using Belsoft.Api.Dtos;
using Belsoft.Api.Models;

namespace Belsoft.Api.Services;

// Async because every implementation that matters does I/O. CancellationToken joins these signatures in L08.
public interface IOrderService
{
    Task<IReadOnlyList<Order>> GetAllAsync();

    Task<Order?> GetByIdAsync(int id);

    Task<CreateOrderResult> CreateAsync(CreateOrderRequest request);
}