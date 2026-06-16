# syntax=docker/dockerfile:1

# ── Stage 1: Build ────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore — külön lépésben, hogy a Docker cache-t kihasználjuk
COPY ["src/TodoApi/TodoApi.csproj", "src/TodoApi/"]
RUN dotnet restore "src/TodoApi/TodoApi.csproj"

# Build
COPY . .
WORKDIR "/src/src/TodoApi"
RUN dotnet build "TodoApi.csproj" -c Release -o /app/build

# ── Stage 2: Publish ──────────────────────────────────────────────────────────
FROM build AS publish
RUN dotnet publish "TodoApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ── Stage 3: Runtime ──────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# ASP.NET Core .NET 8+ alapértelmezett portja konténerben: 8080
EXPOSE 8080

# Nem root felhasználóval futtatjuk (biztonság)
USER app

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "TodoApi.dll"]
