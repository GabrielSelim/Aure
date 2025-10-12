FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

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
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Aure.API.dll"]