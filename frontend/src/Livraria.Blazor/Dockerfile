# =============================================================================
# Dockerfile para Livraria.Blazor (Blazor WebAssembly Standalone)
# Multi-stage build: .NET SDK para build, nginx para servir arquivos estáticos
# =============================================================================

# -----------------------------------------------------------------------------
# Stage 1: Build
# -----------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar arquivos de solução e projeto primeiro para cache de layers
COPY Livraria.Frontend.sln ./
COPY src/Livraria.Blazor/Livraria.Blazor.csproj src/Livraria.Blazor/

# Restaurar dependências
RUN dotnet restore src/Livraria.Blazor/Livraria.Blazor.csproj

# Copiar todo o código fonte
COPY src/Livraria.Blazor/ src/Livraria.Blazor/

# Build do projeto
WORKDIR /src/src/Livraria.Blazor
RUN dotnet build -c Release -o /app/build

# -----------------------------------------------------------------------------
# Stage 2: Publish
# -----------------------------------------------------------------------------
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# -----------------------------------------------------------------------------
# Stage 3: Runtime (nginx)
# -----------------------------------------------------------------------------
FROM nginx:alpine AS final

# Remover configuração padrão do nginx
RUN rm /etc/nginx/conf.d/default.conf 2>/dev/null || true

# Copiar arquivos publicados do Blazor para nginx
COPY --from=publish /app/publish/wwwroot /usr/share/nginx/html

# Copiar configuração customizada do nginx para SPA routing
# Nota: O build context é o diretório frontend/
COPY src/Livraria.Blazor/nginx.conf /etc/nginx/nginx.conf

# Expor porta 80
EXPOSE 80

# nginx já tem entrypoint padrão
CMD ["nginx", "-g", "daemon off;"]
