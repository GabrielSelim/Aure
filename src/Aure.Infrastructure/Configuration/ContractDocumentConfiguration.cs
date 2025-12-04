using Aure.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aure.Infrastructure.Configuration;

public class ContractDocumentConfiguration : IEntityTypeConfiguration<ContractDocument>
{
    public void Configure(EntityTypeBuilder<ContractDocument> builder)
    {
        builder.ToTable("ContractDocuments");

        builder.HasKey(cd => cd.Id);

        builder.Property(cd => cd.ConteudoHtml)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(cd => cd.ConteudoPdf)
            .HasColumnType("text");

        builder.Property(cd => cd.ConteudoDocx)
            .HasColumnType("text");

        builder.Property(cd => cd.VersaoMajor)
            .IsRequired();

        builder.Property(cd => cd.VersaoMinor)
            .IsRequired();

        builder.Property(cd => cd.DadosPreenchidos)
            .HasColumnType("jsonb")
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<string, string>()
            );

        builder.Property(cd => cd.DataGeracao)
            .HasColumnName("data_geracao")
            .IsRequired();

        builder.Property(cd => cd.EhVersaoFinal)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(cd => cd.HashDocumento)
            .HasMaxLength(100);

        builder.HasOne(cd => cd.Contract)
            .WithMany()
            .HasForeignKey(cd => cd.ContractId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(cd => cd.Template)
            .WithMany()
            .HasForeignKey(cd => cd.TemplateId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(cd => cd.GeradoPorUsuario)
            .WithMany()
            .HasForeignKey(cd => cd.GeradoPorUsuarioId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(cd => cd.ContractId);
        builder.HasIndex(cd => cd.TemplateId);
        builder.HasIndex(cd => cd.EhVersaoFinal);
        builder.HasIndex(cd => new { cd.ContractId, cd.EhVersaoFinal });

        builder.HasQueryFilter(cd => !cd.IsDeleted);
    }
}
