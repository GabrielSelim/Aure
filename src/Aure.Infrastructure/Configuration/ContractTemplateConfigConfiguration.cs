using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Aure.Domain.Entities;
using System.Text.Json;

namespace Aure.Infrastructure.Configuration
{
    public class ContractTemplateConfigConfiguration : IEntityTypeConfiguration<ContractTemplateConfig>
    {
        public void Configure(EntityTypeBuilder<ContractTemplateConfig> builder)
        {
            builder.ToTable("contract_template_configs");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.CompanyId)
                .IsRequired();

            builder.Property(c => c.NomeConfig)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.Categoria)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(c => c.TituloServico)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(c => c.DescricaoServico)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(c => c.LocalPrestacaoServico)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(c => c.DetalhamentoServicos)
                .IsRequired()
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
                )
                .HasColumnType("jsonb");

            builder.Property(c => c.ObrigacoesContratado)
                .IsRequired()
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
                )
                .HasColumnType("jsonb");

            builder.Property(c => c.ObrigacoesContratante)
                .IsRequired()
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
                )
                .HasColumnType("jsonb");

            builder.Property(c => c.ClausulaAjudaCusto)
                .HasMaxLength(2000);

            builder.Property(c => c.CreatedAt)
                .IsRequired();

            builder.Property(c => c.UpdatedAt)
                .IsRequired();

            builder.HasOne(c => c.Company)
                .WithMany()
                .HasForeignKey(c => c.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(c => c.CompanyId);

            builder.HasIndex(c => new { c.CompanyId, c.NomeConfig })
                .IsUnique();
        }
    }
}
