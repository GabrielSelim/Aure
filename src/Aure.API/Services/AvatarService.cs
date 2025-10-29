using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace Aure.API.Services;

public class AvatarService : IAvatarService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<AvatarService> _logger;
    private const string AvatarsFolder = "uploads/avatars";
    private const int OriginalSize = 400;
    private const int ThumbnailSize = 80;
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;

    public AvatarService(
        IWebHostEnvironment environment,
        ILogger<AvatarService> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public async Task<string> UploadAvatarAsync(IFormFile file, Guid userId)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("Arquivo inválido");

        if (file.Length > MaxFileSizeBytes)
            throw new InvalidOperationException($"Arquivo muito grande. Máximo permitido: 5MB");

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        
        if (!allowedExtensions.Contains(extension))
            throw new InvalidOperationException("Formato de arquivo não permitido. Use JPG ou PNG");

        var uploadsPath = Path.Combine(_environment.WebRootPath, AvatarsFolder);
        Directory.CreateDirectory(uploadsPath);

        await DeleteAvatarAsync(userId);

        var originalFileName = $"{userId}.jpg";
        var thumbnailFileName = $"{userId}_thumb.jpg";
        var originalPath = Path.Combine(uploadsPath, originalFileName);
        var thumbnailPath = Path.Combine(uploadsPath, thumbnailFileName);

        try
        {
            using (var image = await Image.LoadAsync(file.OpenReadStream()))
            {
                image.Mutate(x => x
                    .Resize(new ResizeOptions
                    {
                        Size = new Size(OriginalSize, OriginalSize),
                        Mode = ResizeMode.Crop
                    }));

                await image.SaveAsJpegAsync(originalPath, new JpegEncoder { Quality = 85 });
            }

            using (var image = await Image.LoadAsync(file.OpenReadStream()))
            {
                image.Mutate(x => x
                    .Resize(new ResizeOptions
                    {
                        Size = new Size(ThumbnailSize, ThumbnailSize),
                        Mode = ResizeMode.Crop
                    }));

                await image.SaveAsJpegAsync(thumbnailPath, new JpegEncoder { Quality = 75 });
            }

            _logger.LogInformation("Avatar do usuário {UserId} salvo com sucesso", userId);

            return $"/{AvatarsFolder}/{originalFileName}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar avatar do usuário {UserId}", userId);
            throw new InvalidOperationException("Erro ao processar imagem", ex);
        }
    }

    public Task DeleteAvatarAsync(Guid userId)
    {
        try
        {
            var uploadsPath = Path.Combine(_environment.WebRootPath, AvatarsFolder);
            
            var originalPath = Path.Combine(uploadsPath, $"{userId}.jpg");
            if (File.Exists(originalPath))
            {
                File.Delete(originalPath);
                _logger.LogInformation("Avatar original do usuário {UserId} deletado", userId);
            }

            var thumbnailPath = Path.Combine(uploadsPath, $"{userId}_thumb.jpg");
            if (File.Exists(thumbnailPath))
            {
                File.Delete(thumbnailPath);
                _logger.LogInformation("Thumbnail do usuário {UserId} deletado", userId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar avatar do usuário {UserId}", userId);
        }

        return Task.CompletedTask;
    }

    public string GetAvatarUrl(Guid userId)
    {
        var uploadsPath = Path.Combine(_environment.WebRootPath, AvatarsFolder);
        var avatarPath = Path.Combine(uploadsPath, $"{userId}.jpg");

        if (File.Exists(avatarPath))
        {
            return $"/{AvatarsFolder}/{userId}.jpg";
        }

        return string.Empty;
    }

    public string GetAvatarUrlWithFallback(Guid userId, string userName)
    {
        var avatarUrl = GetAvatarUrl(userId);
        
        if (!string.IsNullOrEmpty(avatarUrl))
            return avatarUrl;

        return GenerateInitialsAvatar(userName);
    }

    private string GenerateInitialsAvatar(string userName)
    {
        if (string.IsNullOrEmpty(userName))
            return string.Empty;

        var parts = userName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var initials = parts.Length >= 2 
            ? $"{parts[0][0]}{parts[^1][0]}" 
            : parts[0].Substring(0, Math.Min(2, parts[0].Length));

        return $"data:initials,{initials.ToUpper()}";
    }
}
