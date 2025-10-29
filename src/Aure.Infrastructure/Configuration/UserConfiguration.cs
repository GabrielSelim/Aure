using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Aure.Domain.Entities;

namespace Aure.Infrastructure.Configuration;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasFilter("is_deleted = false");

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(u => u.Role)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(u => u.AvatarUrl)
            .HasMaxLength(500);

        builder.Property(u => u.CPFEncrypted)
            .HasMaxLength(500);

        builder.HasIndex(u => u.CPFEncrypted)
            .IsUnique()
            .HasFilter("cpf_encrypted IS NOT NULL AND is_deleted = false");

        builder.Property(u => u.RGEncrypted)
            .HasMaxLength(500);

        builder.Property(u => u.TelefoneCelular)
            .HasMaxLength(20);

        builder.Property(u => u.TelefoneFixo)
            .HasMaxLength(20);

        builder.Property(u => u.Cargo)
            .HasMaxLength(100);

        builder.Property(u => u.EnderecoRua)
            .HasMaxLength(200);

        builder.Property(u => u.EnderecoNumero)
            .HasMaxLength(20);

        builder.Property(u => u.EnderecoComplemento)
            .HasMaxLength(100);

        builder.Property(u => u.EnderecoBairro)
            .HasMaxLength(100);

        builder.Property(u => u.EnderecoCidade)
            .HasMaxLength(100);

        builder.Property(u => u.EnderecoEstado)
            .HasMaxLength(2);

        builder.Property(u => u.EnderecoPais)
            .HasMaxLength(50);

        builder.Property(u => u.EnderecoCep)
            .HasMaxLength(10);

        builder.Property(u => u.VersaoTermosUsoAceita)
            .HasMaxLength(20);

        builder.Property(u => u.VersaoPoliticaPrivacidadeAceita)
            .HasMaxLength(20);

        builder.HasOne(u => u.Company)
            .WithMany()
            .HasForeignKey(u => u.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.NotificationPreferences)
            .WithOne()
            .HasForeignKey<NotificationPreferences>(np => np.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(u => u.CompanyId);
        builder.HasIndex(u => u.IsDeleted);
        builder.HasIndex(u => u.DataNascimento);
    }
}
