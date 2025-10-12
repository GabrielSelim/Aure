using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Aure.Domain.Entities;

namespace Aure.Infrastructure.Data.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id)
            .HasDefaultValueSql("uuid_generate_v4()");

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Cnpj)
            .IsRequired()
            .HasMaxLength(18);

        builder.Property(x => x.Type)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.KycStatus)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(x => x.UpdatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(x => x.Cnpj)
            .IsUnique()
            .HasDatabaseName("idx_companies_cnpj");

        builder.HasIndex(x => x.Type)
            .HasDatabaseName("idx_companies_type");

        builder.HasIndex(x => x.KycStatus)
            .HasDatabaseName("idx_companies_kyc_status");

        builder.HasIndex(x => x.CreatedAt)
            .HasDatabaseName("idx_companies_created_at");

        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasMany(x => x.ClientContracts)
            .WithOne(x => x.Client)
            .HasForeignKey(x => x.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.ProviderContracts)
            .WithOne(x => x.Provider)
            .HasForeignKey(x => x.ProviderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.KycRecords)
            .WithOne(x => x.Company)
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}