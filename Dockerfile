# =========================================
# 📦 ETAPA 1: BUILD (Compilación)
# =========================================
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

# 🔥 TRUCO DE OPTIMIZACIÓN: Copiamos SOLO los archivos .csproj primero
COPY APISistemaVenta.sln .
COPY SistemaVenta.API/SistemaVenta.API.csproj         SistemaVenta.API/
COPY SistemaVenta.BLL/SistemaVenta.BLL.csproj         SistemaVenta.BLL/
COPY SistemaVenta.DAL/SistemaVenta.DAL.csproj         SistemaVenta.DAL/
COPY SistemaVenta.DTO/SistemaVenta.DTO.csproj         SistemaVenta.DTO/
COPY SistemaVenta.IOC/SistemaVenta.IOC.csproj         SistemaVenta.IOC/
COPY SistemaVenta.Model/SistemaVenta.Model.csproj     SistemaVenta.Model/
COPY SistemaVenta.Utility/SistemaVenta.Utility.csproj SistemaVenta.Utility/

# Restauramos los paquetes NuGet (dependencias)
RUN dotnet restore APISistemaVenta.sln

# ✅ Ahora sí, copiamos TODO el código fuente
COPY . .

# Publicamos la aplicación en modo Release
RUN dotnet publish SistemaVenta.API/SistemaVenta.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# =========================================
# 🚀 ETAPA 2: RUNTIME (Ejecución)
# =========================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
WORKDIR /app

# 🔐 ICU para globalización (requerido por Microsoft.Data.SqlClient en Alpine)
RUN apk add --no-cache icu-libs icu-data-full \
    && adduser -D appuser

# Copiamos SOLO lo publicado desde la etapa de build, asignando dueño correcto
COPY --from=build --chown=appuser:appuser /app/publish .

# =========================================
# ⚙️ CONFIGURACIÓN DE ENTORNO
# =========================================
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# 🚪 Exponemos el puerto 8080
EXPOSE 8080

# 🎯 Comando de entrada: ejecuta la aplicación como usuario no-root
USER appuser
ENTRYPOINT ["dotnet", "SistemaVenta.API.dll"]