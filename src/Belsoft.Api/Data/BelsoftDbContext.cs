using Belsoft.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Belsoft.Api.Data;

// One instance per HTTP request (scoped): it tracks that request's objects and is not thread-safe.
public sealed class BelsoftDbContext(DbContextOptions<BelsoftDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OrderLine> OrderLines => Set<OrderLine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BelsoftDbContext).Assembly);
    }
}