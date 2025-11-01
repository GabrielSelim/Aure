using Aure.Domain.Entities;
using Aure.Domain.Enums;
using Aure.Domain.Interfaces;
using System.Security.Claims;
using System.Text.Json;

namespace Aure.API.Middleware;

public class AuditMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditMiddleware> _logger;

    public AuditMiddleware(RequestDelegate next, ILogger<AuditMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IUnitOfWork unitOfWork)
    {
        var originalBodyStream = context.Response.Body;
        
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        var method = context.Request.Method;
        var path = context.Request.Path.Value ?? string.Empty;
        var startTime = DateTime.UtcNow;

        try
        {
            await _next(context);

            var statusCode = context.Response.StatusCode;
            var endTime = DateTime.UtcNow;
            var duration = endTime - startTime;

            if (ShouldAudit(method, path, statusCode))
            {
                await CreateAuditLogAsync(context, unitOfWork, method, path, statusCode, duration);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar requisição: {Method} {Path}", method, path);
            throw;
        }
        finally
        {
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
        }
    }

    private bool ShouldAudit(string method, string path, int statusCode)
    {
        if (method == "GET" && statusCode == 200)
            return false;

        if (path.StartsWith("/health") || path.StartsWith("/swagger"))
            return false;

        var criticalPaths = new[]
        {
            "/api/registration",
            "/api/auth/login",
            "/api/contracts",
            "/api/payments",
            "/api/users",
            "/api/userprofile",
            "/api/companyrelationships"
        };

        return criticalPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase));
    }

    private async Task CreateAuditLogAsync(
        HttpContext context,
        IUnitOfWork unitOfWork,
        string method,
        string path,
        int statusCode,
        TimeSpan duration)
    {
        try
        {
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userId = Guid.TryParse(userIdClaim, out var parsedUserId) ? parsedUserId : (Guid?)null;

            var entityInfo = ExtractEntityInfo(path);
            var action = MapHttpMethodToAuditAction(method, statusCode);

            var auditLog = new AuditLog
            {
                EntityName = entityInfo.EntityName,
                EntityId = entityInfo.EntityId,
                Action = action,
                PerformedBy = userId,
                PerformedByEmail = context.User.FindFirst(ClaimTypes.Email)?.Value,
                IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                UserAgent = context.Request.Headers["User-Agent"].ToString(),
                Timestamp = DateTime.UtcNow,
                HttpMethod = method,
                Path = path,
                StatusCode = statusCode,
                Duration = duration.TotalMilliseconds,
                Success = statusCode >= 200 && statusCode < 300
            };

            await unitOfWork.AuditLogs.AddAsync(auditLog);
            await unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Auditoria: {Action} em {Entity} por {User} - Status: {Status}",
                action, entityInfo.EntityName, userId, statusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar log de auditoria para {Path}", path);
        }
    }

    private (string EntityName, Guid? EntityId) ExtractEntityInfo(string path)
    {
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length < 2)
            return ("Unknown", null);

        var entityName = segments[1] switch
        {
            "registration" => "User",
            "auth" => "Authentication",
            "users" => "User",
            "userprofile" => "UserProfile",
            "contracts" => "Contract",
            "payments" => "Payment",
            "companyrelationships" => "CompanyRelationship",
            "audit" => "Audit",
            _ => segments[1]
        };

        Guid? entityId = null;
        if (segments.Length > 2 && Guid.TryParse(segments[2], out var parsedId))
        {
            entityId = parsedId;
        }

        return (entityName, entityId);
    }

    private AuditAction MapHttpMethodToAuditAction(string method, int statusCode)
    {
        if (statusCode >= 400)
            return AuditAction.Read;

        return method switch
        {
            "POST" => AuditAction.Create,
            "PUT" => AuditAction.Update,
            "PATCH" => AuditAction.Update,
            "DELETE" => AuditAction.Delete,
            _ => AuditAction.Read
        };
    }
}

public static class AuditMiddlewareExtensions
{
    public static IApplicationBuilder UseAuditMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<AuditMiddleware>();
    }
}
