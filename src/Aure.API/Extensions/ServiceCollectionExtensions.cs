using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using FluentValidation;
using Aure.Infrastructure.Data;
using Aure.Domain.Interfaces;
using Aure.Infrastructure.Repositories;
using Aure.Application.Interfaces;
using Aure.Application.Services;
using Aure.Application.Mappings;

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

        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<ICnpjValidationService, CnpjValidationService>();

        // HttpClient básico
        services.AddHttpClient();

        // Health Checks básicos
        services.AddHealthChecks()
            .AddDbContextCheck<AureDbContext>("database");

        services.AddAutoMapper(typeof(ApplicationMappingProfile));

        services.AddValidatorsFromAssembly(typeof(ApplicationMappingProfile).Assembly);

        return services;
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        // TODO: Implementar autenticação JWT
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