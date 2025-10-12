using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Aure.Domain.Entities;

namespace Aure.Infrastructure.Data.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id)
            .HasDefaultValueSql("uuid_generate_v4()");

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.Method)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.PaymentDate)
            .HasColumnType("timestamp with time zone");

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(x => x.UpdatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(x => x.Status)
            .HasDatabaseName("idx_payments_status");

        builder.HasIndex(x => x.Method)
            .HasDatabaseName("idx_payments_method");

        builder.HasIndex(x => x.PaymentDate)
            .HasDatabaseName("idx_payments_payment_date");

        builder.HasIndex(x => x.ContractId)
            .HasDatabaseName("idx_payments_contract_id");

        builder.HasIndex(x => x.CreatedAt)
            .HasDatabaseName("idx_payments_created_at");

        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasOne(x => x.Contract)
            .WithMany(x => x.Payments)
            .HasForeignKey(x => x.ContractId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.SplitExecutions)
            .WithOne(x => x.Payment)
            .HasForeignKey(x => x.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.LedgerEntries)
            .WithOne(x => x.Payment)
            .HasForeignKey(x => x.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}