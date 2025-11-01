FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

# Instalar curl para health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Aure.API/Aure.API.csproj", "src/Aure.API/"]
COPY ["src/Aure.Application/Aure.Application.csproj", "src/Aure.Application/"]
COPY ["src/Aure.Domain/Aure.Domain.csproj", "src/Aure.Domain/"]
COPY ["src/Aure.Infrastructure/Aure.Infrastructure.csproj", "src/Aure.Infrastructure/"]
RUN dotnet restore "src/Aure.API/Aure.API.csproj"
COPY . .
WORKDIR "/src/src/Aure.API"
RUN dotnet build "Aure.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Aure.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

# Criar diretórios necessários
RUN mkdir -p /app/logs /app/uploads /app/certificates /app/wwwroot/uploads/avatars && \
    chmod -R 755 /app/wwwroot && \
    chmod -R 755 /app/uploads

# Copiar aplicação
COPY --from=publish /app/publish .

# Configurar variáveis de ambiente
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD curl -f http://localhost:5000/health || exit 1

ENTRYPOINT ["dotnet", "Aure.API.dll"]