using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Aure.Domain.Entities;

namespace Aure.Infrastructure.Data.Configurations;

public class ContractConfiguration : IEntityTypeConfiguration<Contract>
{
    public void Configure(EntityTypeBuilder<Contract> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id)
            .HasDefaultValueSql("uuid_generate_v4()");

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.ValueTotal)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.IpfsCid)
            .HasMaxLength(100);

        builder.Property(x => x.Sha256Hash)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(x => x.UpdatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(x => x.Status)
            .HasDatabaseName("idx_contracts_status");

        builder.HasIndex(x => x.ClientId)
            .HasDatabaseName("idx_contracts_client_id");

        builder.HasIndex(x => x.ProviderId)
            .HasDatabaseName("idx_contracts_provider_id");

        builder.HasIndex(x => x.CreatedAt)
            .HasDatabaseName("idx_contracts_created_at");

        builder.HasIndex(x => x.Sha256Hash)
            .IsUnique()
            .HasDatabaseName("idx_contracts_sha256_hash");

        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasOne(x => x.Client)
            .WithMany(x => x.ClientContracts)
            .HasForeignKey(x => x.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Provider)
            .WithMany(x => x.ProviderContracts)
            .HasForeignKey(x => x.ProviderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.TokenizedAsset)
            .WithOne(x => x.Contract)
            .HasForeignKey<TokenizedAsset>(x => x.ContractId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Signatures)
            .WithOne(x => x.Contract)
            .HasForeignKey(x => x.ContractId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Payments)
            .WithOne(x => x.Contract)
            .HasForeignKey(x => x.ContractId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.SplitRules)
            .WithOne(x => x.Contract)
            .HasForeignKey(x => x.ContractId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}