using Belsoft.Api.Models;

namespace Belsoft.Api.Data;

// Small query building blocks. Everything returns IQueryable, so it still runs as SQL.
public static class QueryableExtensions
{
    extension<T>(IQueryable<T> source)
    {
        // Callers validate page (>= 1) and clamp pageSize BEFORE calling this.
        public IQueryable<T> Paged(int page, int pageSize) =>
            source.Skip((page - 1) * pageSize).Take(pageSize);
    }

    extension(IQueryable<Order> orders)
    {
        // Extension PROPERTY: new in C# 14. Id breaks ties so every page boundary is stable.
        public IQueryable<Order> NewestFirst =>
            orders.OrderByDescending(order => order.CreatedAt)
                  .ThenByDescending(order => order.Id);

        public IQueryable<Order> WithStatus(params ReadOnlySpan<OrderStatus> statuses)
        {
            if (statuses.IsEmpty)
            {
                return orders;
            }

            List<OrderStatus> wanted = [.. statuses];
            return orders.Where(order => wanted.Contains(order.Status));
        }
    }
}