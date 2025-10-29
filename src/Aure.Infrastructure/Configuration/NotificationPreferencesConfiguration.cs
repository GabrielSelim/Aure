using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Aure.Domain.Entities;

namespace Aure.Infrastructure.Configuration;

public class NotificationPreferencesConfiguration : IEntityTypeConfiguration<NotificationPreferences>
{
    public void Configure(EntityTypeBuilder<NotificationPreferences> builder)
    {
        builder.HasKey(np => np.Id);

        builder.Property(np => np.UserId)
            .IsRequired();

        builder.Property(np => np.ReceberEmailNovoContrato)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(np => np.ReceberEmailContratoAssinado)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(np => np.ReceberEmailContratoVencendo)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(np => np.ReceberEmailPagamentoProcessado)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(np => np.ReceberEmailPagamentoRecebido)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(np => np.ReceberEmailNovoFuncionario)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(np => np.ReceberEmailAlertasFinanceiros)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(np => np.ReceberEmailAtualizacoesSistema)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasOne<User>()
            .WithOne(u => u.NotificationPreferences)
            .HasForeignKey<NotificationPreferences>(np => np.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(np => np.UserId)
            .IsUnique()
            .HasFilter("is_deleted = false");

        builder.HasIndex(np => np.IsDeleted);
    }
}
