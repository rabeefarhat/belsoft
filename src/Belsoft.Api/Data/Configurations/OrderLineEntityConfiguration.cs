using Belsoft.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Belsoft.Api.Data.Configurations;

public sealed class OrderLineEntityConfiguration : IEntityTypeConfiguration<OrderLineEntity>
{
    public void Configure(EntityTypeBuilder<OrderLineEntity> builder)
    {
        builder.ToTable("OrderLines", table =>
        {
            table.HasCheckConstraint("CK_OrderLines_Quantity", "[Quantity] BETWEEN 1 AND 1000");
            table.HasCheckConstraint("CK_OrderLines_UnitPrice", "[UnitPrice] >= 0");
            table.HasCheckConstraint("CK_OrderLines_ProductCode_NotBlank", "LEN([ProductCode]) > 0");
        });

        builder.HasKey(line => line.Id);

        builder.Property(line => line.ProductCode)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(line => line.UnitPrice)
            .HasPrecision(18, 2);                                   // ①

        builder.HasData(                                            // ②
            new { Id = 1, OrderId = 1, ProductCode = "KB-01", Quantity = 1, UnitPrice = 49.90m },
            new { Id = 2, OrderId = 2, ProductCode = "MON-27", Quantity = 1, UnitPrice = 120.00m },
            new { Id = 3, OrderId = 3, ProductCode = "MUG-01", Quantity = 1, UnitPrice = 15.25m });
    }
}