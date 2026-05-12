```markdown
# MCP.md — Manual de Contexto para OpenCode / Qwen Local

## Propósito

Este archivo define cómo debe operar la IA local dentro del repositorio.

No es un archivo de configuración MCP JSON. Es una guía de contexto, seguridad y límites.

## Proyecto

Dockerización de proyecto:

```text
.NET 10 + Angular 16 + SQL Server
```

## Objetivo principal

Crear un entorno reproducible con Docker:

```text
Backend API + SQL Server + Frontend Angular
```

## Reglas de seguridad

1. No ejecutar comandos destructivos sin confirmación.
2. No borrar carpetas del usuario.
3. No ejecutar `docker system prune -a` sin confirmar.
4. No modificar `.env` con secretos reales.
5. No commitear passwords.
6. No usar `latest` como tag final de producción.
7. No introducir Kubernetes todavía.
8. No cambiar versiones del stack sin aprobación.
9. No usar `dotnet publish /t:PublishContainer` si hay junctions.
10. Preferir Dockerfile multi-stage.

## Comandos permitidos sin pedir confirmación

Solo sugerir, no ejecutar automáticamente:

```powershell
docker version
docker info
docker images
docker ps
docker ps --all
dotnet --info
dotnet build
dotnet test
npm --version
node --version
```

## Comandos que requieren confirmación

```powershell
docker rm
docker rmi
docker volume rm
docker system prune
docker compose down -v
git reset
git clean
Remove-Item
```

## Flujo correcto

### Fase 0 — Auditoría

Revisar:

- estructura del repo
- solución `.sln`
- `.csproj`
- `Program.cs`
- `appsettings.json`
- estructura Angular
- package.json
- scripts SQL
- existencia de Dockerfile
- existencia de docker-compose

### Fase 1 — Backend

Crear:

- `Dockerfile`
- `.dockerignore`
- comandos de build y run
- documentación

### Fase 2 — SQL Server

Crear:

- servicio `sqlserver` en `docker-compose.yml`
- named volume
- variables de entorno
- `.env.example`

### Fase 3 — API + SQL Server

Conectar API con SQL Server usando:

```text
Server=sqlserver,1433
```

No usar `localhost` dentro del contenedor.

### Fase 4 — Angular

Crear Dockerfile Angular:

- build con Node
- runtime con Nginx
- configuración de rutas Angular
- proxy o variable de API URL

### Fase 5 — Compose completo

Crear:

```text
docker-compose.yml
```

Con servicios:

- api
- sqlserver
- frontend

### Fase 6 — Documentación

Actualizar:

- README.md
- docs/docker
- evidencia
- troubleshooting

## Formato de salida obligatorio

Cada respuesta debe incluir:

```markdown
## Qué revisé

## Qué encontré

## Qué haré ahora

## Archivos a modificar

## Comandos sugeridos

## Cómo verificar

## Riesgos

## Nota para documentación
```