# AGENTS.md — APISistemaVenta

Stack: .NET 10 / ASP.NET Core Web API (Controllers, not Minimal API) / EF Core 10 / SQL Server 2022 / JWT + BCrypt + Refresh Tokens.

## Architecture

8 projects in `APISistemaVenta.sln`: API → BLL → DAL → Model + DTO + IOC + Utility + Tests.

- **DI wiring**: `SistemaVenta.IOC/Dependencia.cs` — registers DbContext, repositories, AutoMapper, JWT service, all service interfaces.
- **AutoMapper profile**: `SistemaVenta.Utility/AutoMapperProfile.cs` (all entity↔DTO mappings).
- **JWT impl**: `SistemaVenta.Utility/Seguridad/JwtService.cs`.
- **API entry**: `SistemaVenta.API/Program.cs` — middleware order: Security Headers → CORS → RateLimiter → Auth → Authorization → Controllers.
- **Frontend**: Separate repo `AppSistemaVenta`, referenced only via Docker Compose `context: ../AppSistemaVenta`.

## Commands

```powershell
dotnet restore
dotnet build --configuration Release
dotnet test                          # xUnit + Moq + coverlet (1 test file: VentaServiceTests.cs)
dotnet run                           # from SistemaVenta.API/ → http://localhost:5018/scalar/v1
```

## Docker

Already containerized. Dockerfile (multi-stage, Alpine+ICU, ~180MB) + docker-compose.yml (api, sqlserver, db-init, frontend).

```powershell
docker-compose up -d                 # full stack from repo root
docker-compose down -v               # ⚠️ destroys sqldata volume
```

Connection string inside compose: `Server=sqlserver,1433` (not localhost).

## Environment

`.env` (gitignored) with `MSSQL_SA_PASSWORD` and `JWT_KEY`. Copy `.env.example` to get started.  
JWT also reads `Jwt:Issuer` / `Jwt:Audience` from `appsettings.json` or env.

## CI / Only Azure Pipelines

`azure-pipelines.yml` — restore + build only (no test, no deploy). Triggers on `main`.

## No lint/formatter tooling

No `.editorconfig`, no StyleCop, no ESLint, no Prettier. C# code has no automated style enforcement.

## Database

Init scripts: `database/init/01-create-db.sql` (idempotent — `IF NOT EXISTS`).  
Connection string key: `ConnectionStrings:cadenaSQL` in `appsettings.json`.

## CORS

Two policies in `Program.cs`: `DesarrolloLocal` (localhost:4200) and `Produccion` (Azure URL). Selected by `ASPNETCORE_ENVIRONMENT`.

## Rate limiting

100 req/min per client, fixed window, rejection at 429. Applied via `RequireRateLimiting("ApiLimit")` on controllers.

## Existing AI docs

`docs/ai/` contains older dockerization-focused agent instructions (AGENTS.md, MCP.md, etc.). Prefer this root AGENTS.md for general development guidance.
