using Microsoft.AspNetCore.Http;

namespace Aure.API.Services;

public interface IAvatarService
{
    Task<string> UploadAvatarAsync(IFormFile file, Guid userId);
    Task DeleteAvatarAsync(Guid userId);
    string GetAvatarUrl(Guid userId);
    string GetAvatarUrlWithFallback(Guid userId, string userName);
}

public class AvatarUploadResponse
{
    public string AvatarUrl { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
}
