using Microsoft.EntityFrameworkCore;
using Aure.Domain.Entities;

namespace Aure.Infrastructure.Data;

public class AureDbContext : DbContext
{
    public AureDbContext(DbContextOptions<AureDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Company> Companies { get; set; }
    public DbSet<CompanyRelationship> CompanyRelationships { get; set; }
    public DbSet<UserInvite> UserInvites { get; set; }
    public DbSet<UserInvitation> UserInvitations { get; set; }
    public DbSet<Contract> Contracts { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Session> Sessions { get; set; }
    public DbSet<Signature> Signatures { get; set; }
    public DbSet<TokenizedAsset> TokenizedAssets { get; set; }
    public DbSet<SplitRule> SplitRules { get; set; }
    public DbSet<SplitExecution> SplitExecutions { get; set; }
    public DbSet<LedgerEntry> LedgerEntries { get; set; }
    public DbSet<KycRecord> KycRecords { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<InvoiceItem> InvoiceItems { get; set; }
    public DbSet<TaxCalculation> TaxCalculations { get; set; }
    public DbSet<NotificationPreferences> NotificationPreferences { get; set; }
    public DbSet<ContractTemplate> ContractTemplates { get; set; }
    public DbSet<ContractDocument> ContractDocuments { get; set; }
    public DbSet<ContractTemplateConfig> ContractTemplateConfigs { get; set; }
    public DbSet<NotificationHistory> NotificationHistories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AureDbContext).Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var tableName = entityType.GetTableName();
            if (tableName != null)
            {
                entityType.SetTableName(tableName.ToLowerInvariant());
            }

            foreach (var property in entityType.GetProperties())
            {
                property.SetColumnName(ToSnakeCase(property.Name));
            }
        }

        modelBuilder.HasPostgresExtension("uuid-ossp");

        // Configuração específica para CompanyRelationship
        modelBuilder.Entity<CompanyRelationship>(entity =>
        {
            entity.HasOne(cr => cr.ClientCompany)
                  .WithMany()
                  .HasForeignKey(cr => cr.ClientCompanyId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(cr => cr.ProviderCompany)
                  .WithMany()
                  .HasForeignKey(cr => cr.ProviderCompanyId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(cr => new { cr.ClientCompanyId, cr.ProviderCompanyId, cr.Type })
                  .IsUnique()
                  .HasFilter("is_deleted = false");
        });
    }

    private static string ToSnakeCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var result = new System.Text.StringBuilder();
        result.Append(char.ToLowerInvariant(input[0]));

        for (int i = 1; i < input.Length; i++)
        {
            if (char.IsUpper(input[i]))
            {
                result.Append('_');
                result.Append(char.ToLowerInvariant(input[i]));
            }
            else
            {
                result.Append(input[i]);
            }
        }

        return result.ToString();
    }
}