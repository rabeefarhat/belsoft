using Belsoft.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Belsoft.Api.Data;

// One instance per HTTP request (scoped): it tracks that request's objects and is not thread-safe.
public sealed class BelsoftDbContext(DbContextOptions<BelsoftDbContext> options) : DbContext(options)   // ①
{
    public DbSet<OrderEntity> Orders => Set<OrderEntity>();                                              // ②

    public DbSet<OrderLineEntity> OrderLines => Set<OrderLineEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BelsoftDbContext).Assembly);                   // ③
    }
}