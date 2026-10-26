using Belsoft.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Belsoft.Api.Data;

// One instance per HTTP request (scoped). Not thread-safe: never share it, never make it a singleton.
public sealed class BelsoftDbContext(DbContextOptions<BelsoftDbContext> options) : DbContext(options)
{
    public DbSet<OrderEntity> Orders => Set<OrderEntity>();

    public DbSet<OrderLineEntity> OrderLines => Set<OrderLineEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Picks up every IEntityTypeConfiguration<T> in this project, so adding an entity never means editing this class.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BelsoftDbContext).Assembly);
    }
}