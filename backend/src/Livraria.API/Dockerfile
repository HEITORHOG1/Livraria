# =============================================================================
# Dockerfile para Livraria.API (.NET 9 ASP.NET Core Web API)
# Multi-stage build para otimização de tamanho da imagem
# =============================================================================

# -----------------------------------------------------------------------------
# Stage 1: Build
# -----------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar arquivos de solução e projetos primeiro para cache de layers
COPY Livraria.Backend.sln ./
COPY src/Livraria.Domain/Livraria.Domain.csproj src/Livraria.Domain/
COPY src/Livraria.Application/Livraria.Application.csproj src/Livraria.Application/
COPY src/Livraria.Infrastructure/Livraria.Infrastructure.csproj src/Livraria.Infrastructure/
COPY src/Livraria.API/Livraria.API.csproj src/Livraria.API/

# Restaurar dependências
RUN dotnet restore src/Livraria.API/Livraria.API.csproj

# Copiar todo o código fonte
COPY src/Livraria.Domain/ src/Livraria.Domain/
COPY src/Livraria.Application/ src/Livraria.Application/
COPY src/Livraria.Infrastructure/ src/Livraria.Infrastructure/
COPY src/Livraria.API/ src/Livraria.API/

# Build do projeto
WORKDIR /src/src/Livraria.API
RUN dotnet build -c Release -o /app/build

# -----------------------------------------------------------------------------
# Stage 2: Publish
# -----------------------------------------------------------------------------
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# -----------------------------------------------------------------------------
# Stage 3: Runtime
# -----------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Configurar variáveis de ambiente
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

# Expor porta 80
EXPOSE 80

# Copiar arquivos publicados
COPY --from=publish /app/publish .

# Definir entrypoint
ENTRYPOINT ["dotnet", "Livraria.API.dll"]
