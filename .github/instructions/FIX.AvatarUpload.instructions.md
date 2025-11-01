# 🔧 Correção: Upload de Avatar

**Problema**: Erro 400 "Value cannot be null. (Parameter 'path1')"  
**Causa**: WebRootPath não configurado no ambiente Docker  
**Prioridade**: CRÍTICA

---

## 📝 Mudanças Necessárias

### 1️⃣ Modificar AvatarService.cs

**Arquivo**: `src/Aure.Application/Services/AvatarService.cs`

**Problema na linha 43:**
```csharp
var uploadsPath = Path.Combine(_environment.WebRootPath, AvatarsFolder);
```

**Solução: Usar caminho absoluto baseado em ContentRootPath**

```csharp
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;

namespace Aure.Application.Services
{
    public class AvatarService : IAvatarService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<AvatarService> _logger;
        private readonly string _uploadsPath;
        private const string AvatarsFolder = "uploads/avatars";

        public AvatarService(
            IWebHostEnvironment environment,
            ILogger<AvatarService> logger)
        {
            _environment = environment;
            _logger = logger;

            var contentRoot = environment.ContentRootPath;
            var wwwrootPath = Path.Combine(contentRoot, "wwwroot");
            _uploadsPath = Path.Combine(wwwrootPath, AvatarsFolder);

            if (!Directory.Exists(_uploadsPath))
            {
                Directory.CreateDirectory(_uploadsPath);
                _logger.LogInformation("Diretório de avatares criado em: {Path}", _uploadsPath);
            }

            _logger.LogInformation("AvatarService inicializado. Uploads path: {Path}", _uploadsPath);
        }

        public async Task<string> UploadAvatarAsync(IFormFile file, Guid userId)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Arquivo inválido");

            if (file.Length > 5 * 1024 * 1024)
                throw new ArgumentException("Arquivo excede o tamanho máximo de 5MB");

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            
            if (!allowedExtensions.Contains(fileExtension))
                throw new ArgumentException("Apenas arquivos JPG e PNG são permitidos");

            await DeleteAvatarAsync(userId);

            var originalFileName = $"{userId}.jpg";
            var thumbnailFileName = $"{userId}_thumb.jpg";
            
            var originalPath = Path.Combine(_uploadsPath, originalFileName);
            var thumbnailPath = Path.Combine(_uploadsPath, thumbnailFileName);

            using (var image = await Image.LoadAsync(file.OpenReadStream()))
            {
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(400, 400),
                    Mode = ResizeMode.Crop
                }));

                await image.SaveAsJpegAsync(originalPath, new JpegEncoder { Quality = 90 });

                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(80, 80),
                    Mode = ResizeMode.Crop
                }));

                await image.SaveAsJpegAsync(thumbnailPath, new JpegEncoder { Quality = 85 });
            }

            _logger.LogInformation("Avatar salvo com sucesso para usuário {UserId} em {Path}", userId, originalPath);

            return $"/{AvatarsFolder}/{originalFileName}";
        }

        public Task<bool> DeleteAvatarAsync(Guid userId)
        {
            var originalFileName = $"{userId}.jpg";
            var thumbnailFileName = $"{userId}_thumb.jpg";
            
            var originalPath = Path.Combine(_uploadsPath, originalFileName);
            var thumbnailPath = Path.Combine(_uploadsPath, thumbnailFileName);

            var deleted = false;

            if (File.Exists(originalPath))
            {
                File.Delete(originalPath);
                deleted = true;
                _logger.LogInformation("Avatar original deletado: {Path}", originalPath);
            }

            if (File.Exists(thumbnailPath))
            {
                File.Delete(thumbnailPath);
                deleted = true;
                _logger.LogInformation("Avatar thumbnail deletado: {Path}", thumbnailPath);
            }

            return Task.FromResult(deleted);
        }

        public Task<string> GetAvatarUrlAsync(Guid userId)
        {
            var fileName = $"{userId}.jpg";
            var filePath = Path.Combine(_uploadsPath, fileName);

            if (File.Exists(filePath))
            {
                return Task.FromResult($"/{AvatarsFolder}/{fileName}");
            }

            return Task.FromResult(string.Empty);
        }

        public Task<string> GetAvatarUrlWithFallbackAsync(Guid userId, string userName)
        {
            var fileName = $"{userId}.jpg";
            var filePath = Path.Combine(_uploadsPath, fileName);

            if (File.Exists(filePath))
            {
                return Task.FromResult($"/{AvatarsFolder}/{fileName}");
            }

            var initials = GetInitials(userName);
            return Task.FromResult($"https://ui-avatars.com/api/?name={Uri.EscapeDataString(initials)}&size=400&background=random");
        }

        private string GetInitials(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "?";

            var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1)
                return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpperInvariant();

            return $"{parts[0][0]}{parts[^1][0]}".ToUpperInvariant();
        }
    }
}
```

---

### 2️⃣ Modificar Program.cs

**Arquivo**: `src/Aure.API/Program.cs`

**Adicionar após `app.UseHttpsRedirection()` e antes de `app.UseAuthentication()`:**

```csharp
app.UseHttpsRedirection();

var wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
if (!Directory.Exists(wwwrootPath))
{
    Directory.CreateDirectory(wwwrootPath);
    Console.WriteLine($"Diretório wwwroot criado em: {wwwrootPath}");
}

app.UseStaticFiles();

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
```

---

### 3️⃣ Modificar Dockerfile

**Arquivo**: `Dockerfile`

**Adicionar após `WORKDIR /app`:**

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

RUN mkdir -p /app/wwwroot/uploads/avatars && \
    chmod -R 755 /app/wwwroot

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/Aure.API/Aure.API.csproj", "src/Aure.API/"]
COPY ["src/Aure.Application/Aure.Application.csproj", "src/Aure.Application/"]
COPY ["src/Aure.Domain/Aure.Domain.csproj", "src/Aure.Domain/"]
COPY ["src/Aure.Infrastructure/Aure.Infrastructure.csproj", "src/Aure.Infrastructure/"]

RUN dotnet restore "src/Aure.API/Aure.API.csproj"

COPY . .

WORKDIR "/src/src/Aure.API"
RUN dotnet build "Aure.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Aure.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

RUN mkdir -p /app/wwwroot/uploads/avatars && \
    chmod -R 755 /app/wwwroot

ENTRYPOINT ["dotnet", "Aure.API.dll"]
```

---

### 4️⃣ Adicionar .gitignore para uploads

**Arquivo**: `.gitignore` (adicionar ao final)

```gitignore
# Uploads de avatares (não versionar)
**/wwwroot/uploads/avatars/*
!**/wwwroot/uploads/avatars/.gitkeep
```

**Criar arquivo vazio** `.gitkeep` em `src/Aure.API/wwwroot/uploads/avatars/.gitkeep`

---

## 🚀 Deploy

### Comandos:

```powershell
# 1. Commit das mudanças
git add .
git commit -m "fix: corrigir upload de avatar - WebRootPath null"
git push origin main

# 2. Deploy em produção
ssh root@5.189.174.61

cd /root/Aure
git pull

# 3. Rebuild com Docker
docker-compose down
docker-compose up -d --build

# 4. Verificar logs
docker logs -f aure-api-aure-gabriel

# 5. Verificar diretório criado
docker exec -it aure-api-aure-gabriel ls -la /app/wwwroot/uploads/avatars/
```

---

## ✅ Testes

### 1. Testar Upload

```bash
POST https://aureapi.gabrielsanztech.com.br/api/UserProfile/avatar
Headers: { 
  Authorization: "Bearer {token}",
  Content-Type: multipart/form-data 
}
Body: file=avatar.jpg (imagem < 5MB, JPG ou PNG)

Esperado: 200
{
  "avatarUrl": "/uploads/avatars/{userId}.jpg",
  "thumbnailUrl": "/uploads/avatars/{userId}_thumb.jpg"
}
```

### 2. Verificar Perfil Atualizado

```bash
GET https://aureapi.gabrielsanztech.com.br/api/UserProfile/perfil-completo
Headers: { Authorization: "Bearer {token}" }

Esperado: 200
{
  "avatarUrl": "/uploads/avatars/{userId}.jpg",
  ...
}
```

### 3. Acessar Avatar pelo Navegador

```
https://aureapi.gabrielsanztech.com.br/uploads/avatars/{userId}.jpg
```

**Esperado**: Imagem exibida no navegador

---

## 📋 Checklist

- [ ] Modificar AvatarService.cs (usar ContentRootPath)
- [ ] Modificar Program.cs (adicionar UseStaticFiles)
- [ ] Modificar Dockerfile (criar wwwroot/uploads/avatars)
- [ ] Adicionar .gitignore para uploads
- [ ] Criar .gitkeep em wwwroot/uploads/avatars
- [ ] Commit e push
- [ ] Deploy em produção
- [ ] Verificar logs
- [ ] Testar upload de avatar
- [ ] Verificar arquivo salvo no servidor
- [ ] Testar acesso via URL
- [ ] Testar delete de avatar
- [ ] Verificar thumbnail gerado

---

## ⚠️ Observações

1. **Permissões**: O diretório `/app/wwwroot/uploads/avatars` precisa ter permissão 755
2. **Volume Docker**: Considerar criar volume persistente para uploads (opcional):
   ```yaml
   volumes:
     - ./uploads:/app/wwwroot/uploads
   ```
3. **Backup**: Avatares são salvos localmente, fazer backup periódico
4. **CDN**: Futuramente, migrar para S3/Azure Blob Storage

---

**Status**: Pronto para implementação  
**Tempo Estimado**: 30 minutos  
**Impacto**: Crítico - desbloqueia feature de avatar
