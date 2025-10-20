using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Serilog;
using FluentValidation;
using Aure.Infrastructure.Data;
using Aure.Domain.Interfaces;
using Aure.Infrastructure.Repositories;
using Aure.Application.Interfaces;
using Aure.Application.Services;
using Aure.Application.Mappings;
using Aure.Infrastructure.Services;
using Aure.Infrastructure.Configuration;

namespace Aure.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AureDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"),
                npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorCodesToAdd: null)));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<ICompanyRelationshipRepository, CompanyRelationshipRepository>();
        services.AddScoped<IUserInviteRepository, UserInviteRepository>();
        services.AddScoped<IContractRepository, ContractRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<ISignatureRepository, SignatureRepository>();
        services.AddScoped<ISplitRuleRepository, SplitRuleRepository>();
        services.AddScoped<ISplitExecutionRepository, SplitExecutionRepository>();
        services.AddScoped<ILedgerEntryRepository, LedgerEntryRepository>();
        services.AddScoped<ITokenizedAssetRepository, TokenizedAssetRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<ITaxCalculationRepository, TaxCalculationRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IKycRecordRepository, KycRecordRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();

        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<ICnpjValidationService, CnpjValidationService>();
        services.AddScoped<ISefazService, SefazService>();
        services.AddScoped<IEmailService, EmailService>();

        // Configurações
        services.Configure<SefazSettings>(configuration.GetSection("SefazSettings"));
        services.Configure<InvoiceSettings>(configuration.GetSection("InvoiceSettings"));
        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

        // HttpClient para validação de CNPJ
        services.AddHttpClient<ICnpjValidationService, CnpjValidationService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.Add("User-Agent", "Aure-System/1.0");
        });

        // HttpClient para SEFAZ
        services.AddHttpClient<ISefazService, SefazService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("User-Agent", "Aure-System/1.0");
            client.DefaultRequestHeaders.Add("Content-Type", "application/soap+xml; charset=utf-8");
        });

        // Health Checks básicos
        services.AddHealthChecks()
            .AddDbContextCheck<AureDbContext>("database");

        services.AddAutoMapper(typeof(ApplicationMappingProfile));

        services.AddValidatorsFromAssembly(typeof(ApplicationMappingProfile).Assembly);

        return services;
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
        var key = Encoding.ASCII.GetBytes(secretKey);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false; // Para desenvolvimento
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidateAudience = true,
                ValidAudience = jwtSettings["Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero // Remove tolerance para expiração
            };

            // Log de eventos de autenticação para debug
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    Console.WriteLine($"Token validated for user: {context.Principal?.Identity?.Name}");
                    return Task.CompletedTask;
                }
            };
        });

        services.AddAuthorization();
        return services;
    }

    public static IServiceCollection AddLoggingServices(this IServiceCollection services)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("logs/aure-.log", rollingInterval: RollingInterval.Day)
            .Enrich.FromLogContext()
            .CreateLogger();

        services.AddSerilog();

        return services;
    }

    public static IServiceCollection AddCorsServices(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", builder =>
            {
                builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        return services;
    }
}