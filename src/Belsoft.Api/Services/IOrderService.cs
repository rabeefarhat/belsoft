using Belsoft.Api.Dtos;
using Belsoft.Api.Models;

namespace Belsoft.Api.Services;

public interface IOrderService
{
    IReadOnlyList<Order> GetAll();

    Order? GetById(int id);

    CreateOrderResult Create(CreateOrderRequest request);
}