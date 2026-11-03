using Belsoft.Api.Data.Entities;
using Belsoft.Api.Models;

namespace Belsoft.Api.Data;

// Small query building blocks. Everything returns IQueryable, so it still runs as SQL.
public static class QueryableExtensions
{
    extension<T>(IQueryable<T> source)                                                       // ①
    {
        // Callers validate page (>= 1) and clamp pageSize BEFORE calling this.
        public IQueryable<T> Paged(int page, int pageSize) =>
            source.Skip((page - 1) * pageSize).Take(pageSize);                               // ②
    }

    extension(IQueryable<OrderEntity> orders)                                                // ③
    {
        // Extension PROPERTY: new in C# 14. Id breaks ties so every page boundary is stable.
        public IQueryable<OrderEntity> NewestFirst =>
            orders.OrderByDescending(order => order.CreatedAt)
                  .ThenByDescending(order => order.Id);                                      // ④

        public IQueryable<OrderEntity> WithStatus(params ReadOnlySpan<OrderStatus> statuses) // ⑤
        {
            if (statuses.IsEmpty)
            {
                return orders;
            }

            List<OrderStatus> wanted = [.. statuses];                                        // ⑥
            return orders.Where(order => wanted.Contains(order.Status));                     // ⑦
        }
    }
}