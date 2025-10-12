using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Aure.Domain.Entities;

namespace Aure.Infrastructure.Data.Configurations;

public class SignatureConfiguration : IEntityTypeConfiguration<Signature>
{
    public void Configure(EntityTypeBuilder<Signature> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id)
            .HasDefaultValueSql("uuid_generate_v4()");

        builder.Property(x => x.SignatureHash)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.Method)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.SignedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(x => x.UpdatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(x => x.ContractId)
            .HasDatabaseName("idx_signatures_contract_id");

        builder.HasIndex(x => x.UserId)
            .HasDatabaseName("idx_signatures_user_id");

        builder.HasIndex(x => x.SignedAt)
            .HasDatabaseName("idx_signatures_signed_at");

        builder.HasIndex(x => new { x.ContractId, x.UserId })
            .IsUnique()
            .HasDatabaseName("idx_signatures_contract_user_unique");

        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasOne(x => x.Contract)
            .WithMany(x => x.Signatures)
            .HasForeignKey(x => x.ContractId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.User)
            .WithMany(x => x.Signatures)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}