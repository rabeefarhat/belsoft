using Belsoft.Api.Data.Entities;
using Belsoft.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Belsoft.Api.Data.Configurations;

public sealed class OrderEntityConfiguration : IEntityTypeConfiguration<OrderEntity>
{
    // Seed rows need a fixed date: a "now" here would change on every build and produce a new migration each time.
    private static readonly DateTimeOffset SeedCreatedAt = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);   // ①

    public void Configure(EntityTypeBuilder<OrderEntity> builder)
    {
        builder.ToTable("Orders", table =>                                                         // ②
        {
            table.HasCheckConstraint("CK_Orders_Status", "[Status] IN (N'Pending', N'Paid', N'Shipped')");
            table.HasCheckConstraint("CK_Orders_CustomerName_NotBlank", "LEN([CustomerName]) > 0");
        });

        builder.HasKey(order => order.Id);                                                         // ③

        builder.Property(order => order.CustomerName)
            .HasMaxLength(100)                                                                      // ④
            .IsRequired();

        builder.Property(order => order.Status)
            .HasConversion<string>()                                                                // ⑤
            .HasMaxLength(20);

        builder.Property(order => order.CreatedAt);                                                 // ⑥

        builder.HasMany(order => order.Lines)                                                       // ⑦
            .WithOne()
            .HasForeignKey(line => line.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // The same three orders InMemoryOrderService started with, so the L01 .http requests still find ids 1–3.
        builder.HasData(                                                                            // ⑧
            new { Id = 1, CustomerName = "Ada Lovelace", Status = OrderStatus.Pending, CreatedAt = SeedCreatedAt },
            new { Id = 2, CustomerName = "Alan Turing", Status = OrderStatus.Paid, CreatedAt = SeedCreatedAt },
            new { Id = 3, CustomerName = "Grace Hopper", Status = OrderStatus.Pending, CreatedAt = SeedCreatedAt });
    }
}