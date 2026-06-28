# syntax=docker/dockerfile:1

# ── Stage 1: Build ────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Tailwind CLI Linux bináris letöltése
RUN curl -fsSL https://github.com/tailwindlabs/tailwindcss/releases/latest/download/tailwindcss-linux-x64 \
    -o /usr/local/bin/tailwindcss && chmod +x /usr/local/bin/tailwindcss


# Restore — külön lépésben, hogy a Docker cache-t kihasználjuk
COPY ["src/TodoApi/TodoApi.csproj", "src/TodoApi/"]
RUN dotnet restore "src/TodoApi/TodoApi.csproj"

# Build
COPY . .
WORKDIR "/src/src/TodoApi"
# CSS generálás (Linux CLI-vel, MSBuild target megkerülve)
RUN tailwindcss -i Styles/input.css -o wwwroot/css/site.css --minify
RUN dotnet build "TodoApi.csproj" -c Release -o /app/build -p:SkipTailwind=true

# ── Stage 2: Publish ──────────────────────────────────────────────────────────
FROM build AS publish
RUN dotnet publish "TodoApi.csproj" -c Release -o /app/publish /p:UseAppHost=false -p:SkipTailwind=true

# ── Stage 3: Runtime ──────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# ASP.NET Core .NET 8+ alapértelmezett portja konténerben: 8080
EXPOSE 8080

# Nem root felhasználóval futtatjuk (biztonság)
USER app

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "TodoApi.dll"]
