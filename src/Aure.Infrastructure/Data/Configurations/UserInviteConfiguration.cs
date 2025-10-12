using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Aure.Domain.Entities;

namespace Aure.Infrastructure.Data.Configurations;

public class UserInviteConfiguration : IEntityTypeConfiguration<UserInvite>
{
    public void Configure(EntityTypeBuilder<UserInvite> builder)
    {
        builder.HasKey(ui => ui.Id);

        builder.Property(ui => ui.InviterName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(ui => ui.InviteeEmail)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(ui => ui.InviteeName)
            .IsRequired()  
            .HasMaxLength(200);

        builder.Property(ui => ui.Role)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(ui => ui.BusinessModel)
            .HasConversion<int?>();

        builder.Property(ui => ui.CompanyName)
            .HasMaxLength(300);

        builder.Property(ui => ui.Cnpj)
            .HasMaxLength(18);

        builder.Property(ui => ui.CompanyType)
            .HasConversion<int?>();

        builder.Property(ui => ui.Token)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(ui => ui.InviteType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(ui => ui.ExpiresAt)
            .IsRequired();

        builder.Property(ui => ui.IsAccepted)
            .IsRequired()
            .HasDefaultValue(false);

        // Relationships
        builder.HasOne(ui => ui.Company)
            .WithMany()
            .HasForeignKey(ui => ui.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ui => ui.InvitedByUser)
            .WithMany()
            .HasForeignKey(ui => ui.InvitedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(ui => ui.Token)
            .IsUnique();

        builder.HasIndex(ui => ui.InviteeEmail);

        builder.HasIndex(ui => new { ui.CompanyId, ui.IsAccepted, ui.IsDeleted });

        // Global query filter
        builder.HasQueryFilter(ui => !ui.IsDeleted);
    }
}