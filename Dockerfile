# =========================================
# 📦 ETAPA 1: BUILD (Compilación)
# =========================================
# Usamos la imagen SDK de .NET 10 con Alpine (ligera, ~200MB vs ~900MB de la versión completa)
# Alpine es ideal para producción porque reduce superficie de ataque y tiempo de descarga
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build

# Establecemos el directorio de trabajo dentro del contenedor
# Todo lo que hagamos después será relativo a /src
WORKDIR /src

# 🔥 TRUCO DE OPTIMIZACIÓN: Copiamos SOLO los archivos .csproj primero
# ¿Por qué? Porque Docker cachea las capas. Si no cambias las referencias,
# no se vuelve a ejecutar `dotnet restore`, ahorrando minutos en cada build.
COPY APISistemaVenta.sln .
COPY SistemaVenta.API/SistemaVenta.API.csproj         SistemaVenta.API/
COPY SistemaVenta.BLL/SistemaVenta.BLL.csproj         SistemaVenta.BLL/
COPY SistemaVenta.DAL/SistemaVenta.DAL.csproj         SistemaVenta.DAL/
COPY SistemaVenta.DTO/SistemaVenta.DTO.csproj         SistemaVenta.DTO/
COPY SistemaVenta.IOC/SistemaVenta.IOC.csproj         SistemaVenta.IOC/
COPY SistemaVenta.Model/SistemaVenta.Model.csproj     SistemaVenta.Model/
COPY SistemaVenta.Utility/SistemaVenta.Utility.csproj SistemaVenta.Utility/

# Restauramos los paquetes NuGet (dependencias)
# Esta capa se reutiliza mientras no cambies los .csproj
RUN dotnet restore APISistemaVenta.sln

# ✅ Ahora sí, copiamos TODO el código fuente
# Como las dependencias ya están instaladas, esto es más rápido
COPY . .

# Publicamos la aplicación en modo Release
# -o /app/publish: carpeta de salida
# --no-restore: no volvemos a restaurar (ya lo hicimos arriba)
# /p:UseAppHost=false: genera un DLL genérico, no un .exe específico de OS
RUN dotnet publish SistemaVenta.API/SistemaVenta.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# =========================================
# 🚀 ETAPA 2: RUNTIME (Ejecución)
# =========================================
# Usamos la imagen de runtime (más pequeña, ~70MB), NO la SDK
# En producción NO necesitas el compilador, solo ejecutar el DLL
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime

WORKDIR /app

# 🔐 BUENA PRÁCTICA DE SEGURIDAD: Ejecutar como usuario no-root
# Si un atacante compromete el contenedor, tendrá permisos limitados
# adduser -D: crea usuario sin contraseña, sin shell, sin home
RUN adduser -D appuser
USER appuser

# Copiamos SOLO lo publicado desde la etapa de build
# Esto reduce drásticamente el tamaño final de la imagen
COPY --from=build /app/publish .

# =========================================
# ⚙️ CONFIGURACIÓN DE ENTORNO
# =========================================
# ASPNETCORE_URLS: Kestrel escuchará en el puerto 8080 dentro del contenedor
# ASPNETCORE_ENVIRONMENT: Define si usa appsettings.Development.json o Production.json
# DOTNET_RUNNING_IN_CONTAINER: Optimizaciones específicas para contenedores
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true

# 🚪 Exponemos el puerto 8080 para que docker-compose pueda mapearlo
EXPOSE 8080

# 🎯 Comando de entrada: ejecuta la aplicación
# [ ] = formato exec (mejor que shell para señales de Linux como Ctrl+C)
ENTRYPOINT ["dotnet", "SistemaVenta.API.dll"]