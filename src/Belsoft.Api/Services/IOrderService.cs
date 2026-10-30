using Belsoft.Api.Dtos;
using Belsoft.Api.Models;

namespace Belsoft.Api.Services;

public interface IOrderService
{
    Task<IReadOnlyList<Order>> GetAllAsync();

    Task<Order?> GetByIdAsync(int id);

    Task<CreateOrderResult> CreateAsync(CreateOrderRequest request);
}