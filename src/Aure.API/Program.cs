using Aure.API.Extensions;
using Aure.API.Filters;
using Aure.API.Middleware;
using Aure.Application.Interfaces;
using Aure.Infrastructure.Data;
using Hangfire;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLoggingServices();
builder.Host.UseSerilog();

builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddCorsServices(builder.Configuration);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "Aure API", 
        Version = "v1",
        Description = "Api para o sistema Aure!"
    });

    // Configuração de autenticação JWT no Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Insira o token JWT no formato: Bearer {seu_token}"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
    
    // Habilitar anotações do Swagger
    c.EnableAnnotations();
    
    // Configuração para mostrar enums como strings no Swagger
    c.SchemaFilter<EnumSchemaFilter>();
    
    // Configuração para exemplo customizado do InviteUserRequest
    c.SchemaFilter<InviteUserRequestSchemaFilter>();
    
    // Configuração para suportar upload de arquivos (IFormFile)
    c.OperationFilter<FileUploadOperationFilter>();
});

builder.Services.AddHealthChecks()
    .AddDbContextCheck<AureDbContext>();

var app = builder.Build();

// Swagger sempre ativo (inclusive em produção)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Aure API V1");
    c.RoutePrefix = string.Empty; // Swagger na raiz do site
    c.DocumentTitle = "Aure API - Sistema Fintech";
    c.OAuthClientId("swagger-ui");
    c.OAuthAppName("Aure API");
});

// Hangfire apenas em desenvolvimento e Docker
if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Docker")
{
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = new[] { new HangfireAuthorizationFilter() }
    });
}

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

var wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
if (!Directory.Exists(wwwrootPath))
{
    Directory.CreateDirectory(wwwrootPath);
    Log.Information("Diretório wwwroot criado em: {Path}", wwwrootPath);
}

app.UseStaticFiles();

// CORS baseado no ambiente
var corsPolicy = app.Environment.IsProduction() ? "AllowSpecificOrigins" : "AllowAll";
app.UseCors(corsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.UseAuditMiddleware();

app.MapControllers();
app.MapHealthChecks("/health");

// Configurar jobs recorrentes do Hangfire
using (var scope = app.Services.CreateScope())
{
    var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
    await notificationService.ScheduleRecurringNotificationsAsync();
}

try
{
    Log.Information("Starting Aure API application");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
