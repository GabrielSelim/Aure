using Aure.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aure.Infrastructure.Configuration;

public class ContractTemplateConfiguration : IEntityTypeConfiguration<ContractTemplate>
{
    public void Configure(EntityTypeBuilder<ContractTemplate> builder)
    {
        builder.ToTable("ContractTemplates");

        builder.HasKey(ct => ct.Id);

        builder.Property(ct => ct.Nome)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(ct => ct.Descricao)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(ct => ct.Tipo)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(ct => ct.ConteudoHtml)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(ct => ct.ConteudoDocx)
            .HasColumnType("text");

        builder.Property(ct => ct.VariaveisDisponiveis)
            .HasColumnType("jsonb")
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>()
            );

        builder.Property(ct => ct.EhPadrao)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(ct => ct.Ativo)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(ct => ct.MotivoDesativacao)
            .HasMaxLength(500);

        builder.HasOne(ct => ct.Company)
            .WithMany()
            .HasForeignKey(ct => ct.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(ct => ct.CompanyId);
        builder.HasIndex(ct => new { ct.CompanyId, ct.Tipo, ct.EhPadrao });
        builder.HasIndex(ct => ct.Ativo);

        builder.HasQueryFilter(ct => !ct.IsDeleted);
    }
}
