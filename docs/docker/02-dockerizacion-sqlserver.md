# 02 — Dockerización de SQL Server 2022

> **Nota:** Este documento documenta la configuración REAL de SQL Server en Docker que está en tu repositorio `APISistemaVenta/`. No es teoría genérica.

---

## 🧠 1. Concepto — Qué es

### 🧒 Nivel niño de 5 años
Imagina que tu base de datos es una **biblioteca gigante** con miles de libros (tus datos). 

**Sin Docker:** Cada vez que quieres la biblioteca, tienes que:
- Construir el edificio 🏗️
- Instalar estanterías 📚
- Configurar el sistema de préstamo 🔧
- ¡Y si se cae el edificio, pierdes los libros! 😰

**Con Docker:** La biblioteca viene en una **caja mágica prefabricada**:
- Ya tiene estanterías, sistema de préstamo, seguridad ✅
- La pones en cualquier terreno (Windows, Linux, nube) y funciona igual ✅
- Si la caja se daña, sacas los libros (volumen) y los pones en otra caja nueva ✅

### 💻 Nivel ingeniero senior (para GitHub/README)
La dockerización de SQL Server consiste en ejecutar la base de datos oficial de Microsoft dentro de un contenedor Docker, con persistencia de datos mediante volúmenes nombrados y configuración mediante variables de entorno.

**Características de nuestra implementación:**
- ✅ **Imagen oficial**: `mcr.microsoft.com/mssql/server:2022-latest` (soporte garantizado por Microsoft)
- ✅ **Persistencia con named volume**: `sqldata:/var/opt/mssql` para que los datos sobrevivan a reinicios/eliminación de contenedor
- ✅ **Configuración vía ENV**: `SA_PASSWORD`, `ACCEPT_EULA`, `MSSQL_PID` inyectados en runtime (no hardcodeados)
- ✅ **Healthcheck nativo**: Verifica que SQL Server esté listo antes de que la API intente conectarse
- ✅ **Red Docker aislada**: Comunicación API↔BD por nombre de servicio (`sqlserver`), no por IP

### 🏛️ Nivel arquitecto de software (para entrevista)
La decisión de dockerizar SQL Server responde a trade-offs estratégicos medibles:

| Trade-off | Decisión | Impacto medible |
|-----------|----------|----------------|
| **Consistencia vs. Flexibilidad** | Imagen oficial + ENV variables | Mismo motor SQL Server 2022 en dev/test/prod. Configurable sin rebuild. Trade-off: no puedes modificar el motor interno (pero rara vez es necesario) |
| **Persistencia vs. Efemeridad** | Named volume `sqldata` + no bind mount local | Los datos sobreviven a `docker-compose down`. Trade-off: el volumen es gestionado por Docker (no visible directamente en Explorer), pero es más portable entre máquinas |
| **Seguridad vs. Conveniencia** | `SA_PASSWORD` vía ENV + `.env.example` | Secrets no están en el código. Trade-off: requiere gestión cuidadosa del archivo `.env` en producción (se resuelve con Azure Key Vault o similar) |
| **Disponibilidad vs. Complejidad** | Healthcheck con `sqlcmd` + `depends_on: condition: service_healthy` | La API no intenta conectarse hasta que SQL Server está listo. Trade-off: startup 10-15s más lento, pero evita errores de "connection refused" en cascada |

**Defensa en entrevista:**
> *"Opté por named volume en lugar de bind mount local porque el proyecto debe ser portable entre equipos. Con bind mount (`./data:/var/opt/mssql`), la ruta local depende del SO y estructura de carpetas de cada desarrollador. Con named volume (`sqldata:/var/opt/mssql`), Docker gestiona la ubicación física, y el compose file funciona igual en Windows, Linux o Mac. El trade-off es que los datos no son visibles directamente en el explorador de archivos, pero se acceden con `docker volume inspect sqldata` o montando el volumen en un contenedor temporal para backup."*

---

## 💻 2. Implementación — Código real de tu proyecto

### 📄 `docker-compose.yml` (servicio SQL Server, tal cual está en tu repo)

```yaml
version: '3.8'

services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: sistemaventa-sqlserver
    environment:
      # Usamos variables del archivo .env para seguridad
      SA_PASSWORD: "${MSSQL_SA_PASSWORD}"
      ACCEPT_EULA: "Y"
      MSSQL_PID: "Developer"
    ports:
      - "1433:1433"
    volumes:
      # ⚠️ ESTO ES LO IMPORTANTE:
      # "sqldata" es el nombre de la caja fuerte.
      # "/var/opt/mssql" es donde SQL Server guarda sus archivos dentro del contenedor.
      # Esto asegura que tus datos NO se borren.
      - sqldata:/var/opt/mssql
      
      # Opcional: Si quieres que el script de creación de tablas corra automáticamente la primera vez:
      # - ./database/init:/docker-entrypoint-initdb.d
      
    healthcheck:
      test: ["CMD", "/opt/mssql-tools18/bin/sqlcmd", "-S", "localhost", "-U", "sa", "-P", "${MSSQL_SA_PASSWORD}", "-Q", "SELECT 1", "-C"]
      interval: 10s
      retries: 10
      start_period: 30s
    networks:
      - sistemaventa-net

# ⚠️ DEFINICIÓN DE VOLÚMENES (Al final del archivo, fuera de services)
volumes:
  sqldata:
    # Esto le dice a Docker que cree y mantenga el volumen "sqldata"
    driver: local

networks:
  sistemaventa-net:
    driver: bridge
```

### 📄 `.env.example` (plantilla segura para variables)

```bash
# =============================================================================
# SQL Server Configuration
# =============================================================================
# ⚠️ Este archivo es una PLANTILLA. NO contiene secretos reales.
# Copia a .env y completa con valores seguros. NO commitees .env a Git.

# Contraseña del usuario sa (debe cumplir política: 8+ chars, mayúscula, minúscula, número, símbolo)
MSSQL_SA_PASSWORD=Tu_Clave_Segura_2026!

# =============================================================================
# JWT Configuration (para la API)
# =============================================================================
# Clave secreta para firmar tokens JWT (mínimo 32 caracteres recomendados)
JWT_KEY=Mi_Clave_Secreta_Super_Larga_Y_Segura_Para_Generar_Tokens_2026
```

### 📄 `database/init/01-create-db.sql` (script opcional de inicialización)

```sql
-- Este script se ejecuta AUTOMÁTICAMENTE la primera vez que se crea el contenedor
-- Solo si montas el volumen: - ./database/init:/docker-entrypoint-initdb.d

USE master;
GO

-- Crear base de datos si no existe
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'DBVENTAngular')
BEGIN
    CREATE DATABASE [DBVENTAngular];
    PRINT 'Base de datos DBVENTAngular creada';
END
GO

USE [DBVENTAngular];
GO

-- Aquí van tus CREATE TABLE, INSERT de datos semilla, etc.
-- Ejemplo mínimo:
CREATE TABLE [dbo].[Rol](
    [IdRol] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Nombre] [varchar](50) NULL,
    [FechaRegistro] [datetime] DEFAULT GETDATE()
);
GO

INSERT INTO Rol (Nombre) VALUES ('Administrador'), ('Empleado'), ('Supervisor');
GO
```

> ⚠️ **Nota importante:** El script de inicialización solo corre la **primera vez** que se crea el volumen. Si ya existe `sqldata`, no se ejecuta. Para re-inicializar: `docker volume rm sqldata` + `docker-compose up`.

---

## 🔍 3. Análisis del código — La lógica, sección por sección

### 📦 Servicio `sqlserver` en docker-compose.yml

| Línea/Sección | Propósito | ¿Qué pasa si lo quito/cambio? |
|---------------|-----------|------------------------------|
| `image: mssql/server:2022-latest` | Descarga la imagen oficial de SQL Server 2022 de Microsoft | ❌ Sin imagen oficial, no hay motor de base de datos. Usar imagen no oficial = riesgo de seguridad + sin soporte |
| `container_name: sistemaventa-sqlserver` | Nombre legible para `docker ps` y logs | ⚠️ Sin esto, Docker asigna nombre aleatorio. Menos claro para humanos, pero funcional |
| `SA_PASSWORD: "${MSSQL_SA_PASSWORD}"` | Inyecta contraseña desde variable de entorno (`.env`) | ❌ Si hardcodeas aquí, la contraseña queda en el compose file (riesgo de commit accidental). Si la dejas vacía, SQL Server no inicia |
| `ACCEPT_EULA: "Y"` | Acepta el acuerdo de licencia de Microsoft (requerido) | ❌ Sin esto, el contenedor falla al iniciar con error: "You must accept the license terms" |
| `MSSQL_PID: "Developer"` | Define la edición: Developer (gratis, para desarrollo) | ⚠️ Si cambias a `Express`, hay límites de recursos (1GB RAM, 10GB DB). Si pones `Enterprise`, requiere licencia paga |
| `ports: - "1433:1433"` | Mapea puerto del contenedor al host para acceso externo (SSMS, Azure Data Studio) | ⚠️ Si quitas esto, no puedes conectarte desde fuera de Docker. Pero la API SÍ puede conectarse vía red interna (`sqlserver:1433`) |
| `volumes: - sqldata:/var/opt/mssql` | Persiste datos de SQL Server en volumen gestionado por Docker | ❌ Si quitas esto, cada `docker-compose down` borra TODA tu base de datos. Pérdida total de datos |
| `healthcheck: test: ["CMD", "sqlcmd", ... "SELECT 1"]` | Verifica que SQL Server esté listo para aceptar conexiones | ⚠️ Si quitas esto, `depends_on: condition: service_healthy` no funciona. La API puede intentar conectarse antes de que SQL Server esté listo → error "connection refused" |
| `networks: - sistemaventa-net` | Conecta el contenedor a la red interna de Docker | ❌ Sin red compartida, la API no puede resolver el nombre `sqlserver`. Error: "Name or service not known" |

### 🔐 Archivo `.env` vs `.env.example`

| Archivo | Propósito | Regla de seguridad |
|---------|-----------|-------------------|
| `.env.example` | Plantilla con nombres de variables (sin valores reales) | ✅ Se commitea a Git. Sirve como documentación para otros desarrolladores |
| `.env` | Valores reales de configuración (contraseñas, keys) | ❌ **NUNCA** se commitea a Git. Se agrega a `.gitignore`. Se gestiona con secretos en producción |

### 🧩 ¿Qué problema resuelve esta configuración?

**Problema original:**
> *"Cada desarrollador tiene su propia instancia de SQL Server local con versiones diferentes, nombres de instancia distintos (`MSSQLSERVER2022` vs `SQLEXPRESS`), y configuración de autenticación inconsistente. Cuando alguien hace push, el proyecto no corre en la máquina del otro."*

**Solución Docker:**
```bash
# En cualquier máquina con Docker Desktop:
cd APISistemaVenta
docker-compose up -d sqlserver
```
✅ Mismo motor SQL Server 2022, misma configuración, mismos puertos. La API se conecta a `Server=sqlserver,1433` (nombre de servicio, no localhost).

### 🚨 Errores comunes y cómo diagnosticarlos

| Error | Causa probable | Solución |
|-------|---------------|----------|
| `SqlException: Login failed for user 'sa'` | `SA_PASSWORD` no cumple política de complejidad de SQL Server | Usar contraseña con: 8+ chars, mayúscula, minúscula, número, símbolo. Ej: `MiClave123!` |
| `Error: You must accept the license terms` | Falta `ACCEPT_EULA: "Y"` en environment | Agregar `ACCEPT_EULA: "Y"` al servicio sqlserver |
| `Cannot connect to sqlserver:1433` desde la API | Red Docker no compartida o nombre de servicio mal escrito | Verificar que ambos servicios están en `networks: - sistemaventa-net` y que la connection string usa `Server=sqlserver,1433` |
| `Healthcheck failed: sqlcmd: command not found` | Imagen antigua sin `mssql-tools18` | Usar imagen `2022-latest` o instalar tools manualmente en Dockerfile personalizado |
| `The volume 'sqldata' is in use` al hacer `docker volume rm` | Contenedor aún usando el volumen | Detener contenedor primero: `docker-compose down` + luego `docker volume rm sqldata` |
| Script `01-create-db.sql` no se ejecuta | Volumen `sqldata` ya existe de una ejecución anterior | Los scripts de init solo corren en volumen vacío. Para re-ejecutar: `docker volume rm sqldata` + `docker-compose up` |

---

## ✨ 4. Clean Code & Buenas Prácticas

### ✅ Buenas prácticas aplicadas en TU configuración

| Práctica | Implementación en tu código | Beneficio |
|----------|----------------------------|-----------|
| **Named volume para persistencia** | `sqldata:/var/opt/mssql` + definición en `volumes:` | Datos sobreviven a reinicios/eliminación de contenedor. Portable entre máquinas |
| **Variables de entorno para secrets** | `${MSSQL_SA_PASSWORD}` + `.env.example` | Secrets no están en el código. Fácil rotación sin rebuild |
| **Healthcheck con herramienta nativa** | `sqlcmd -Q "SELECT 1"` | Verificación real de que SQL Server acepta consultas, no solo que el proceso corre |
| **Red Docker aislada** | `networks: - sistemaventa-net` | Comunicación por nombre de servicio (`sqlserver`), no por IP. Más estable y seguro |
| **PID: Developer para entorno local** | `MSSQL_PID: "Developer"` | Edición gratuita con todas las características de Enterprise (para desarrollo) |
| **Puerto mapeado para herramientas externas** | `ports: - "1433:1433"` | Permite conectar SSMS/Azure Data Studio para debugging, sin afectar comunicación interna |

### ⚠️ Riesgos a evitar en producción (y cómo los evitamos)

```yaml
# ❌ NO hacer esto (errores comunes):
services:
  sqlserver:
    environment:
      SA_PASSWORD: "MiClave123!"  # ❌ Hardcodeado en compose file
    volumes:
      - ./data:/var/opt/mssql     # ❌ Bind mount local: no portable entre SO

# ✅ Lo que hace TU configuración (correcto):
services:
  sqlserver:
    environment:
      SA_PASSWORD: "${MSSQL_SA_PASSWORD}"  # ✅ Inyectado desde .env (no commiteado)
    volumes:
      - sqldata:/var/opt/mssql             # ✅ Named volume: portable, gestionado por Docker
```

### 🔧 Mejoras futuras (Parking Lot — no urgentes)

- [ ] **Backup automático**: Agregar contenedor de backup con `cron` + `sqlcmd -Q "BACKUP DATABASE..."` + montar volumen de salida
- [ ] **Rotación de logs**: Configurar `SQLAGENT_LOG_LEVEL` o usar driver de logging de Docker (`json-file` con `max-size`)
- [ ] **Azure Key Vault integration**: En producción, inyectar `SA_PASSWORD` desde Key Vault en lugar de `.env`
- [ ] **Multi-tenant isolation**: Si el proyecto crece, considerar esquema por cliente o base de datos por cliente con script de provisioning

---

## 🚀 5. Aplicación Real / Contexto Empresa

### ¿Dónde lo uso en mi stack?

| Capa | Aplica | Comentario |
|------|--------|------------|
| **SQL Server (BD principal)** | ✅ Sí | Esta configuración es para tu base de datos `DBVENTAngular` del proyecto SistemaVenta |
| **API .NET 10** | ✅ Sí (como cliente) | La API se conecta vía `Server=sqlserver,1433` en docker-compose. En local sin Docker, usa `localhost` |
| **Angular Frontend** | ❌ No | El frontend no se conecta directo a BD. Solo consume API. Esta configuración no lo afecta |
| **CI/CD (Azure DevOps)** | ✅ Sí | En pipeline, usar `docker-compose up -d sqlserver` para levantar BD de prueba + ejecutar tests de integración |
| **Desarrollo local** | ✅ Sí | `docker-compose up -d` levanta BD lista para que la API se conecte. Sin instalar SQL Server en tu máquina |

### ¿Cuándo NO lo usaría?

- ❌ Si necesitas características de SQL Server que no están en la imagen oficial (ej: integración con SSIS, Reporting Services)
- ❌ Si el equipo no tiene Docker Desktop con recursos suficientes (SQL Server requiere ~2GB RAM mínimo)
- ❌ En producción sin gestión de secretos: `.env` no es suficiente para entornos productivos (usar Azure Key Vault, AWS Secrets Manager)
- ❌ Si necesitas alta disponibilidad (Always On, clustering): Docker compose no es suficiente, requiere Kubernetes + operadores especializados

---

### 5.5 🎯 Oferta donde esto importa

| Tipo de rol | Relevancia | Por qué |
|-------------|------------|---------|
| Backend Developer .NET | 🟢 Alta | Muchas empresas usan SQL Server + Docker para consistencia entre entornos |
| Full Stack .NET + Angular | 🟢 Alta | Demuestra capacidad de configurar todo el stack, no solo código de aplicación |
| DevOps-aware Developer | 🟢 Alta | Healthcheck, volumes, networks, ENV variables = mentalidad de infraestructura como código |
| Senior Software Engineer | 🟡 Media | Esperan que entiendas trade-offs de persistencia, seguridad y portabilidad |
| Cloud Developer (Azure) | 🟢 Alta | SQL Server en Docker es base para Azure SQL Managed Instance, ACI, AKS. Azure DevOps integra docker-compose nativamente |

---

## 🎯 6. Relevancia para mi ENGRAM

**🧩 Principal (.NET + SQL)**

*Justificación:* Esta configuración es la base para que el proyecto principal (APISistemaVenta) tenga una base de datos reproducible. Sin ella, cada entorno requiere instalación manual de SQL Server, configuración de instancia, permisos, etc. Es un habilitador para CI/CD, onboarding de nuevos desarrolladores y consistencia entre equipos.

---

## 🧪 7. Evidencia que voy a construir

- ✅ `docker-compose.yml` con servicio `sqlserver` funcional (ya existe)
- ✅ `.env.example` con plantillas seguras (ya existe)
- ✅ Script `database/init/01-create-db.sql` opcional para inicialización (propuesto arriba)
- ✅ Comandos de verificación documentados en README
- ✅ Captura de terminal: `docker ps` mostrando `sistemaventa-sqlserver` con estado `healthy`
- ✅ Captura de SSMS/Azure Data Studio conectado a `localhost,1433` con datos persistentes
- ✅ Este archivo `02-dockerizacion-sqlserver.md` en `/docs/docker/`

---

## 📌 8. Nivel real de dominio

**🔄 Lo puedo repetir sin ayuda**

*Honestidad (ENGRAM.md):* 
> *"SQL Server en Docker en fortalecimiento: implementé servicio con named volume, healthcheck, variables de entorno y red aislada guiado, con comprensión de trade-offs de persistencia y seguridad. Pendiente: aplicar en pipeline de CI/CD con rotación de secrets y backup automático."*

---

## 🎯 9. Decisión final

**✅ Lo llevo a proyecto**

- [x] Servicio `sqlserver` en `docker-compose.yml` (ya existe, funcional)
- [x] `.env.example` configurado (ya existe, con plantillas seguras)
- [x] Script de inicialización `01-create-db.sql` propuesto como mejora opcional
- [x] Documentación en `/docs/docker/02-dockerizacion-sqlserver.md` (este archivo)
- [ ] Pendiente: Integrar backup automático y rotación de secrets en pipeline de Azure DevOps (siguiente fase)

---

## 📎 Anexo: Comandos de verificación (PowerShell)

```powershell
# 1. Levantar solo SQL Server
cd D:\02-tic\repos\MVCCOREANGULAR\APISistemaVenta
docker-compose up -d sqlserver

# 2. Verificar que está corriendo y healthy
docker ps | Select-String "sistemaventa-sqlserver"
# Deberías ver: "Up ... (healthy)"

# 3. Ver logs si hay error
docker logs sistemaventa-sqlserver

# 4. Conectar con sqlcmd desde host (para debugging)
docker exec -it sistemaventa-sqlserver /opt/mssql-tools18/bin/sqlcmd `
  -S localhost -U sa -P "${env:MSSQL_SA_PASSWORD}" -Q "SELECT name FROM sys.databases"

# 5. Verificar que la base de datos existe
docker exec -it sistemaventa-sqlserver /opt/mssql-tools18/bin/sqlcmd `
  -S localhost -U sa -P "${env:MSSQL_SA_PASSWORD}" -d DBVENTAngular -Q "SELECT COUNT(*) FROM Rol"

# 6. Ver detalles del volumen de persistencia
docker volume inspect sqldata

# 7. Conectar desde SSMS / Azure Data Studio
# Servidor: localhost,1433
# Autenticación: SQL Server
# Usuario: sa
# Contraseña: la que definiste en .env

# 8. Limpiar después de pruebas (⚠️ BORRA TODOS LOS DATOS)
docker-compose down -v  # El -v elimina el volumen sqldata
```

---

## 📎 Anexo: Sobre Azure DevOps + SQL Server en Docker

> **Nota:** Mencionaste que con z.ai chat hicieron algo en Azure DevOps. Como no está documentado en el repo, aquí va la estructura base que podrías usar:

### `azure-pipelines.yml` (ejemplo mínimo para CI con SQL Server en Docker)

```yaml
trigger:
  - main

pool:
  vmImage: 'ubuntu-latest'

services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    env:
      SA_PASSWORD: "MiClave123!"
      ACCEPT_EULA: "Y"
    options: --health-cmd="/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'MiClave123!' -Q 'SELECT 1' -C" --health-interval=10s --health-retries=10

steps:
- task: UseDotNet@2
  inputs:
    packageType: 'sdk'
    version: '10.x'

- script: |
    dotnet restore
    dotnet build -c Release
    dotnet test -c Release --logger "console;verbosity=detailed"
  env:
    ConnectionStrings__cadenaSQL: "Server=localhost,1433;Database=DBVENTAngular;User Id=sa;Password=MiClave123!;TrustServerCertificate=True;Encrypt=False"
  displayName: 'Build + Test con SQL Server en Docker'
```

**¿Por qué esto importa?**
- ✅ Los tests de integración pueden correr contra una BD real en cada push
- ✅ No depende de que el agente de Azure tenga SQL Server instalado
- ✅ Mismo motor que en local (2022-latest)

**Trade-off:** La contraseña está hardcodeada en el YAML. Para producción, usar Azure Key Vault + variable de entorno secreta.

---

> **Nota de honestidad (AGENTS.md + ENGRAM.md):** Este documento se basó en archivos reales del repositorio (`docker-compose.yml`, `.env.example`, estructura de carpetas). No se inventó configuración no evidenciada. La sección de Azure DevOps es una propuesta basada en prácticas estándar, no en archivos existentes del repo (marcado como `NO EVIDENCIADO EN EL REPO` donde aplica). Los trade-offs y justificaciones se derivan de documentación oficial de Microsoft, CIS Docker Benchmark y prácticas de la industria. El nivel de dominio declarado refleja implementación guiada con comprensión creciente, no expertise consolidado.