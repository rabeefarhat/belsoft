using Belsoft.Api.Data.Entities;
using Belsoft.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Belsoft.Api.Data.Configurations;

internal sealed class OrderEntityConfiguration : IEntityTypeConfiguration<OrderEntity>
{
    public void Configure(EntityTypeBuilder<OrderEntity> builder)
    {
        // Same table name and rules as the hand-written L06 schema: the database protects the data from every writer.
        builder.ToTable("Orders", table =>                                                    // ①
        {
            table.HasCheckConstraint("CK_Orders_CustomerName_NotBlank", "LTRIM(RTRIM([CustomerName])) <> N''");
            table.HasCheckConstraint("CK_Orders_Status", "[Status] IN (N'Pending', N'Paid', N'Shipped')");
        });

        builder.HasKey(order => order.Id);                                                   // ②

        builder.Property(order => order.CustomerName)
            .HasMaxLength(100)                                                               // ③
            .IsRequired();

        // Stored as text, not the default int: a row reads "Paid", and reordering the enum can't silently corrupt data.
        builder.Property(order => order.Status)
            .HasConversion<string>()                                                         // ④
            .HasMaxLength(20);

        builder.Property(order => order.CreatedAt)
            .IsRequired();

        builder.HasMany(order => order.Lines)                                                // ⑤
            .WithOne()
            .HasForeignKey(line => line.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Demo data, so the L01 .http requests (orders 1–3) still find something. Fixed values: seed data is part of a migration.
        builder.HasData(                                                                     // ⑥
            new { Id = 1, CustomerName = "Ada Lovelace", Status = OrderStatus.Pending, CreatedAt = SeedTime },
            new { Id = 2, CustomerName = "Alan Turing", Status = OrderStatus.Paid, CreatedAt = SeedTime },
            new { Id = 3, CustomerName = "Grace Hopper", Status = OrderStatus.Pending, CreatedAt = SeedTime });
    }

    private static readonly DateTimeOffset SeedTime = new(2026, 10, 1, 9, 0, 0, TimeSpan.Zero);  // ⑦
}