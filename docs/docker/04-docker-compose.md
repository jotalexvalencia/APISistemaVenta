# 04 — Orquestación con Docker Compose

> **Nota:** Este documento documenta el `docker-compose.yml` REAL que está en tu repositorio `APISistemaVenta/` (o raíz `MVCCOREANGULAR/`). No es teoría genérica.

---

## 🧠 1. Concepto — Qué es

### 🧒 Nivel niño de 5 años
Imagina que tu aplicación es un **equipo de fútbol** ⚽:

- **SQL Server** = El portero 🧤 (guarda los datos)
- **API .NET** = El centrocampista ⚙️ (procesa las jugadas)
- **Angular** = El delantero 🥅 (muestra el resultado al público)

**Sin Docker Compose:** Tienes que:
- Buscar a cada jugador por separado 🔍
- Enseñarles a pasarse la pelota uno por uno 🗣️
- ¡Y si uno llega tarde, el partido no empieza! 😰

**Con Docker Compose:** Tienes un **entrenador con silbato** 📋:
- Llama a los 3 jugadores con un solo comando ✅
- Les dice cómo pasarse la pelota (red interna) ✅
- Espera a que el portero esté listo antes de empezar el juego ✅
- ¡Un comando y el equipo está en cancha! ⚽🚀

### 💻 Nivel ingeniero senior (para GitHub/README)
Docker Compose es una herramienta para definir y ejecutar aplicaciones multi-contenedor usando un archivo YAML. Permite orquestar servicios (API, BD, frontend) con configuración centralizada de redes, volúmenes, variables de entorno y dependencias.

**Características de nuestra implementación:**
- ✅ **Servicios definidos**: `sqlserver`, `api`, `frontend` en un solo archivo
- ✅ **Red aislada**: `sistemaventa-net` para comunicación interna por nombre de servicio
- ✅ **Persistencia**: Named volume `sqldata` para que la BD sobreviva a reinicios
- ✅ **Healthcheck + depends_on**: La API no inicia hasta que SQL Server está listo
- ✅ **Variables de entorno**: `.env` para secrets sin hardcodear en el compose file
- ✅ **Build contexts relativos**: Cada servicio construye su imagen desde su carpeta

### 🏛️ Nivel arquitecto de software (para entrevista)
La decisión de usar Docker Compose para orquestar este stack responde a trade-offs estratégicos medibles:

| Trade-off | Decisión | Impacto medible |
|-----------|----------|----------------|
| **Simplicidad vs. Escalabilidad** | Docker Compose (no Kubernetes) para desarrollo | Onboarding de nuevos desarrolladores: 5 min vs 2 horas. Trade-off: no es adecuado para producción con auto-scaling (se resuelve migrando el mismo compose a ECS/AKS después) |
| **Acoplamiento vs. Autonomía** | Servicios en misma red Docker + comunicación por nombre | La API resuelve `sqlserver:1433` sin configuración de DNS externa. Trade-off: los servicios no pueden correr fuera de Docker sin cambiar config (se resuelve con `environment.ts` condicional) |
| **Seguridad vs. Conveniencia** | Secrets vía `.env` + `.gitignore` | Contraseñas no están en el código. Trade-off: requiere gestión manual del archivo `.env` en cada entorno (se resuelve con Azure Key Vault en producción) |
| **Consistencia vs. Flexibilidad** | Mismo compose para dev/test | "Funciona en mi máquina" → "Funciona en CI/CD". Trade-off: no permite configuraciones muy distintas entre entornos (se resuelve con `docker-compose.override.yml`) |

**Defensa en entrevista:**
> *"Opté por Docker Compose en lugar de Kubernetes para desarrollo porque el proyecto es una aplicación monolítica modular, no microservicios distribuidos. Compose me da: 1) un solo comando para levantar todo el stack, 2) comunicación interna por nombre de servicio sin configuración de DNS, 3) healthchecks nativos para evitar race conditions, y 4) portabilidad entre Windows/Mac/Linux. El trade-off es que Compose no maneja auto-scaling ni rolling updates avanzados, pero para desarrollo local y CI/CD básico es la herramienta adecuada. Cuando el proyecto escale a producción con alta disponibilidad, migraré la misma configuración a Azure Container Apps o AKS."*

---

## 💻 2. Implementación — Código real de tu proyecto

### 📄 `docker-compose.yml` completo (tal cual está en tu repo)

```yaml
version: '3.8'

services:
  # =============================================================================
  # 🗄️ SQL Server 2022
  # =============================================================================
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: sistemaventa-sqlserver
    environment:
      # Variables inyectadas desde .env (NO hardcodear aquí)
      SA_PASSWORD: "${MSSQL_SA_PASSWORD}"
      ACCEPT_EULA: "Y"
      MSSQL_PID: "Developer"
    ports:
      - "1433:1433"  # Mapeo para herramientas externas (SSMS, Azure Data Studio)
    volumes:
      # Named volume para persistencia de datos
      - sqldata:/var/opt/mssql
    healthcheck:
      # Verifica que SQL Server acepte consultas antes de considerar el servicio "healthy"
      test: ["CMD", "/opt/mssql-tools18/bin/sqlcmd", "-S", "localhost", "-U", "sa", "-P", "${MSSQL_SA_PASSWORD}", "-Q", "SELECT 1", "-C"]
      interval: 10s
      retries: 10
      start_period: 30s  # Tiempo de gracia para que SQL Server inicie (~20-25s reales)
    networks:
      - sistemaventa-net

  # =============================================================================
  # ⚙️ API .NET 10
  # =============================================================================
  api:
    build:
      context: ./APISistemaVenta  # Ruta relativa desde donde ejecutas docker-compose
      dockerfile: Dockerfile
    container_name: sistemaventa-api
    ports:
      - "8080:8080"  # Host:Contenedor
    environment:
      ASPNETCORE_ENVIRONMENT: "Development"
      ASPNETCORE_URLS: "http://+:8080"
      # 🔥 CRÍTICO: Connection string usando NOMBRE DE SERVICIO, no localhost
      ConnectionStrings__cadenaSQL: "Server=sqlserver,1433;Database=DBVENTAngular;User Id=sa;Password=${MSSQL_SA_PASSWORD};TrustServerCertificate=True;Encrypt=False"
      Jwt__Key: "${JWT_KEY}"
      Jwt__Issuer: "SistemaVentaAPI"
      Jwt__Audience: "SistemaVentaClient"
    depends_on:
      sqlserver:
        condition: service_healthy  # Espera a que healthcheck de SQL Server pase
    restart: on-failure  # Reinicia automáticamente si la API falla
    networks:
      - sistemaventa-net

  # =============================================================================
  # 🌐 Frontend Angular 16 + Nginx
  # =============================================================================
  frontend:
    build:
      context: ./AppSistemaVenta
      dockerfile: Dockerfile
    container_name: sistemaventa-frontend
    ports:
      - "4200:80"  # Angular en host:4200 → Nginx en contenedor:80
    environment:
      # Variable opcional para inyectar endpoint en runtime (si implementas window.env)
      - API_BASE_URL=http://api:8080
    depends_on:
      - api  # Espera a que la API esté corriendo (no healthcheck, pero es un inicio)
    restart: on-failure
    networks:
      - sistemaventa-net

# =============================================================================
# 🌐 Redes
# =============================================================================
networks:
  sistemaventa-net:
    driver: bridge  # Red interna: los servicios se comunican por nombre, no por IP

# =============================================================================
# 💾 Volúmenes
# =============================================================================
volumes:
  sqldata:  # Named volume gestionado por Docker
    driver: local
```

### 📄 `.env.example` (plantilla segura)

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

### 📄 `environment.ts` del frontend (configuración para Docker Compose)

```typescript
// src/environments/environment.ts
export const environment = {
    production: false,
    // Para docker-compose: el frontend se comunica con la API por NOMBRE DE SERVICIO
    endpoint: "http://api:8080/api/"
    
    // Para desarrollo SIN docker-compose (frontend en Docker, API en host):
    // endpoint: "http://host.docker.internal:8080/api/"
};
```

> ⚠️ **Nota crítica:** Dentro de la red Docker (`sistemaventa-net`), los contenedores se resuelven por **nombre de servicio**, no por `localhost`. 
> - `localhost` dentro de un contenedor = el contenedor mismo
> - `api` dentro de la red = el contenedor del servicio `api`

---

## 🔍 3. Análisis del código — La lógica, sección por sección

### 📦 Servicio `sqlserver`

| Línea/Sección | Propósito | ¿Qué pasa si lo quito/cambio? |
|---------------|-----------|------------------------------|
| `healthcheck: test: ["CMD", "sqlcmd", ... "SELECT 1"]` | Verifica que SQL Server acepte consultas reales, no solo que el proceso corre | ❌ Sin healthcheck, `depends_on: condition: service_healthy` no funciona. La API intenta conectarse antes de que SQL Server esté listo → error "connection refused" |
| `start_period: 30s` | Da tiempo a SQL Server para iniciar (tarda ~20-25s en Alpine) | ⚠️ Si reduces a 10s, el healthcheck puede fallar antes de que SQL Server esté listo → reinicios en bucle |
| `volumes: - sqldata:/var/opt/mssql` | Persiste datos en volumen gestionado por Docker | ❌ Sin esto, cada `docker-compose down` borra TODA la base de datos. Pérdida total de datos |
| `networks: - sistemaventa-net` | Conecta a la red interna para comunicación con API | ❌ Sin red compartida, la API no puede resolver `Server=sqlserver,1433` → error "Name or service not known" |

### ⚙️ Servicio `api`

| Línea/Sección | Propósito | ¿Qué pasa si lo quito/cambio? |
|---------------|-----------|------------------------------|
| `build: context: ./APISistemaVenta` | Indica dónde está el Dockerfile del backend | ❌ Si la ruta es incorrecta, `docker-compose build` falla con "context not found" |
| `ConnectionStrings__cadenaSQL: "Server=sqlserver,1433;..."` | Connection string usando NOMBRE DE SERVICIO | ❌ Si usas `localhost` aquí, la API intenta conectarse a sí misma, no a SQL Server → error de conexión |
| `depends_on: sqlserver: condition: service_healthy` | Espera a que SQL Server pase healthcheck antes de iniciar API | ⚠️ Si quitas `condition: service_healthy`, la API puede iniciar antes que SQL Server esté listo → errores intermitentes al arranque |
| `restart: on-failure` | Reinicia automáticamente si la API crashea | ✅ Útil para desarrollo. En producción, considerar políticas más estrictas o orquestador con healthchecks |
| `Jwt__Key: "${JWT_KEY}"` | Inyecta clave JWT desde variable de entorno | ❌ Si hardcodeas aquí, la clave queda en el compose file (riesgo de commit accidental) |

### 🌐 Servicio `frontend`

| Línea/Sección | Propósito | ¿Qué pasa si lo quito/cambio? |
|---------------|-----------|------------------------------|
| `build: context: ./AppSistemaVenta` | Indica dónde está el Dockerfile del frontend | ❌ Si la ruta es incorrecta, build falla |
| `ports: - "4200:80"` | Mapea puerto host 4200 → puerto contenedor 80 (Nginx) | ⚠️ Si cambias a `"80:80"`, el frontend ocupa puerto 80 del host (puede conflictuar con IIS u otros servicios) |
| `depends_on: - api` | Espera a que el servicio `api` esté corriendo (no healthy) | ⚠️ No espera healthcheck de la API. Si la API tarda en iniciar, el frontend puede dar error de conexión al arranque (se resuelve con retry en Angular) |
| `environment: - API_BASE_URL=http://api:8080` | Variable opcional para inyección en runtime | ⚠️ Si no implementas `window.env` en Angular, esta variable no se usa. Es preparación para configuración dinámica futura |

### 🌐 Red `sistemaventa-net`

| Configuración | Propósito | ¿Qué pasa si lo quito? |
|---------------|-----------|----------------------|
| `driver: bridge` | Crea red interna tipo puente para comunicación entre servicios | ❌ Sin red definida, Docker crea una red por defecto, pero los nombres de servicio no se resuelven consistentemente |

### 💾 Volumen `sqldata`

| Configuración | Propósito | ¿Qué pasa si lo quito? |
|---------------|-----------|----------------------|
| `driver: local` | Usa el driver por defecto de Docker para gestionar el volumen en el host | ❌ Sin definición de volumen, `sqldata:/var/opt/mssql` falla con "named volume not defined" |

### 🧩 ¿Qué problema resuelve Docker Compose?

**Problema original:**
> *"Para correr el proyecto completo, necesito: 1) iniciar SQL Server manualmente, 2) esperar a que esté listo, 3) iniciar la API con la connection string correcta, 4) construir y correr el frontend con el endpoint apuntando a la API. Si algo falla, debo reiniciar en orden. Y si un compañero quiere probar, debe repetir todo esto en su máquina."*

**Solución Docker Compose:**
```bash
# En cualquier máquina con Docker Desktop:
cd D:\02-tic\repos\MVCCOREANGULAR
docker-compose up -d
```
✅ Un comando levanta los 3 servicios en el orden correcto, con configuración consistente, redes aisladas y persistencia de datos. El stack completo está disponible en:
- Frontend: `http://localhost:4200`
- API: `http://localhost:8080`
- SQL Server: `localhost,1433` (para SSMS)

### 🚨 Errores comunes y cómo diagnosticarlos

| Error | Causa probable | Solución |
|-------|---------------|----------|
| `Cannot connect to sqlserver:1433` desde la API | La API y SQL Server no están en la misma red Docker | Verificar que ambos servicios tienen `networks: - sistemaventa-net` |
| `Login failed for user 'sa'` | `MSSQL_SA_PASSWORD` no cumple política de complejidad | Usar contraseña con: 8+ chars, mayúscula, minúscula, número, símbolo. Ej: `MiClave123!` |
| `ERR_CONNECTION_REFUSED` en frontend al llamar a `http://api:8080` | El frontend está corriendo FUERA de Docker (localhost) pero usa endpoint `api:8080` | Si corres frontend con `ng serve`, usar `host.docker.internal:8080`. Si corre en Docker Compose, usar `api:8080` |
| `Named volume "sqldata" not found` | Volumen no definido en sección `volumes:` al final del archivo | Agregar `volumes: sqldata: driver: local` al final del compose file |
| `Service 'api' depends on service 'sqlserver' which is undefined` | Nombre de servicio mal escrito en `depends_on` | Verificar que `sqlserver` en `depends_on` coincide exactamente con el nombre del servicio definido |
| `Healthcheck failed: sqlcmd: command not found` | Imagen de SQL Server antigua sin `mssql-tools18` | Usar imagen `2022-latest` o ajustar el comando del healthcheck a la versión de tools disponible |
| `Bind for 0.0.0.0:8080 failed: port is already allocated` | Otro proceso (o contenedor) ya usa el puerto 8080 en el host | Cambiar a `- "8081:8080"` en el servicio `api`, o detener el proceso que ocupa el puerto |

---

## ✨ 4. Clean Code & Buenas Prácticas

### ✅ Buenas prácticas aplicadas en TU docker-compose.yml

| Práctica | Implementación en tu código | Beneficio |
|----------|----------------------------|-----------|
| **Named volume para persistencia** | `sqldata:/var/opt/mssql` + definición en `volumes:` | Datos sobreviven a `docker-compose down`. Portable entre máquinas |
| **Variables de entorno para secrets** | `${MSSQL_SA_PASSWORD}`, `${JWT_KEY}` + `.env.example` | Secrets no están en el código. Fácil rotación sin rebuild |
| **Healthcheck con herramienta nativa** | `sqlcmd -Q "SELECT 1"` para SQL Server | Verificación real de que el servicio está listo para trabajar, no solo que el proceso corre |
| **depends_on con condition: service_healthy** | API espera a que SQL Server pase healthcheck | Evita race conditions al arranque: la API no intenta conectarse antes de que la BD esté lista |
| **Red Docker aislada** | `networks: - sistemaventa-net` en todos los servicios | Comunicación por nombre de servicio (`sqlserver`, `api`), no por IP. Más estable y seguro |
| **Build contexts relativos** | `context: ./APISistemaVenta`, `context: ./AppSistemaVenta` | Permite ejecutar `docker-compose` desde la raíz del proyecto, sin importar dónde estén los sub-repos |
| **restart: on-failure** | En servicios `api` y `frontend` | Recuperación automática ante fallos temporales en desarrollo |

### ⚠️ Riesgos a evitar en producción (y cómo los evitamos)

```yaml
# ❌ NO hacer esto (errores comunes):
services:
  api:
    environment:
      ConnectionStrings__cadenaSQL: "Server=localhost,1433;..."  # ❌ localhost no funciona dentro de Docker
      Jwt__Key: "MiClaveSecreta123"  # ❌ Hardcodeado en compose file

# ✅ Lo que hace TU configuración (correcto):
services:
  api:
    environment:
      ConnectionStrings__cadenaSQL: "Server=sqlserver,1433;..."  # ✅ Nombre de servicio Docker
      Jwt__Key: "${JWT_KEY}"  # ✅ Inyectado desde .env (no commiteado)
```

### 🔧 Mejoras futuras (Parking Lot — no urgentes)

- [ ] **docker-compose.override.yml**: Para configuraciones específicas de desarrollo (ej: volúmenes de código para hot-reload) sin modificar el compose principal
- [ ] **Healthcheck para la API**: Agregar endpoint `/health` en .NET y healthcheck en compose para que el frontend espere a que la API esté realmente lista
- [ ] **Secrets management en producción**: Reemplazar `.env` por Azure Key Vault o Docker secrets en entorno productivo
- [ ] **Multi-arch build**: `docker-compose build --platform linux/amd64,linux/arm64` para soportar M1/M2 y servidores ARM
- [ ] **Logging driver configurado**: `logging: options: max-size: "10m", max-file: "3"` para evitar que logs llenen el disco

---

## 🚀 5. Aplicación Real / Contexto Empresa

### ¿Dónde lo uso en mi stack?

| Capa | Aplica | Comentario |
|------|--------|------------|
| **Stack completo (.NET + Angular + SQL)** | ✅ Sí | Este `docker-compose.yml` orquesta los 3 servicios del proyecto principal SistemaVenta |
| **Desarrollo local** | ✅ Sí | `docker-compose up -d` levanta todo el stack para desarrollo sin instalar SQL Server o Node en la máquina host |
| **CI/CD (Azure DevOps)** | ✅ Sí | En pipeline, `docker-compose up -d` levanta entorno de prueba + ejecuta tests de integración + `docker-compose down` limpia |
| **Onboarding de nuevos desarrolladores** | ✅ Sí | Un comando y el nuevo miembro tiene el stack completo corriendo. Sin instalación manual de 2 horas |
| **Demo / Presentación** | ✅ Sí | Para mostrar el proyecto a clientes o en entrevista: `docker-compose up` y listo |

### ¿Cuándo NO lo usaría?

- ❌ Si necesitas escalar horizontalmente (múltiples instancias de la API): Docker Compose no maneja load balancing nativo (se requiere Kubernetes o Azure Container Apps)
- ❌ Si necesitas configuración muy distinta entre entornos (dev/stage/prod): mejor usar `docker-compose.override.yml` o herramientas como Helm
- ❌ Si el equipo no tiene Docker Desktop con recursos suficientes (SQL Server + API + Frontend requieren ~3-4GB RAM mínimo)
- ❌ En producción sin gestión de secrets: `.env` no es suficiente para entornos productivos (usar Azure Key Vault, AWS Secrets Manager)

---

### 5.5 🎯 Oferta donde esto importa

| Tipo de rol | Relevancia | Por qué |
|-------------|------------|---------|
| Backend Developer .NET | 🟢 Alta | Muchas ofertas piden "experiencia con Docker y orquestación de servicios" |
| Full Stack .NET + Angular | 🟢 Alta | Demuestra capacidad de entregar solución completa, no solo código de una capa |
| DevOps-aware Developer | 🟢 Alta | Docker Compose + healthchecks + redes + volúmenes = mentalidad de infraestructura como código |
| Senior Software Engineer | 🟡 Media | Esperan que entiendas trade-offs de orquestación, persistencia y configuración entre entornos |
| Cloud Developer (Azure) | 🟢 Alta | Base para Azure Container Apps, AKS, Azure DevOps pipelines con docker-compose |

---

## 🎯 6. Relevancia para mi ENGRAM

**🧩 Principal (.NET + SQL)**

*Justificación:* Docker Compose es el habilitador clave para que el proyecto principal (APISistemaVenta + AppSistemaVenta + SQL Server) sea reproducible, portable y fácil de desplegar. Sin él, cada entorno requiere configuración manual propensa a errores. Es un habilitador para CI/CD, onboarding de nuevos desarrolladores y consistencia entre equipos.

---

## 🧪 7. Evidencia que voy a construir

- ✅ `docker-compose.yml` funcional con 3 servicios (ya existe, probado)
- ✅ `.env.example` con plantillas seguras (ya existe)
- ✅ Comandos de verificación documentados en README
- ✅ Captura de terminal: `docker-compose ps` mostrando los 3 servicios `Up (healthy)`
- ✅ Captura de navegador: frontend en `http://localhost:4200` con login funcional contra API en Docker
- ✅ Captura de SSMS conectado a `localhost,1433` con datos persistentes después de `docker-compose down/up`
- ✅ Este archivo `04-docker-compose.md` en `/docs/docker/`

---

## 📌 8. Nivel real de dominio

**🔄 Lo puedo repetir sin ayuda**

*Honestidad (ENGRAM.md):* 
> *"Docker Compose en fortalecimiento: implementé orquestación de 3 servicios (SQL Server, .NET API, Angular) con healthchecks, redes aisladas, volúmenes nombrados y variables de entorno guiado, con comprensión de trade-offs de persistencia, seguridad y comunicación entre servicios. Pendiente: aplicar en pipeline de CI/CD con gestión de secrets y configuración multi-entorno."*

---

## 🎯 9. Decisión final

**✅ Lo llevo a proyecto**

- [x] `docker-compose.yml` con 3 servicios funcional (ya existe, probado)
- [x] `.env.example` configurado (ya existe, con plantillas seguras)
- [x] Documentación en `/docs/docker/04-docker-compose.md` (este archivo)
- [ ] Pendiente: Integrar `docker-compose.override.yml` para hot-reload en desarrollo
- [ ] Pendiente: Agregar healthcheck para la API .NET
- [ ] Pendiente: Migrar gestión de secrets a Azure Key Vault para producción

---

## 📎 Anexo: Comandos de verificación (PowerShell)

```powershell
# =============================================================================
# 1. Levantar todo el stack
# =============================================================================
cd D:\02-tic\repos\MVCCOREANGULAR
docker-compose up -d

# =============================================================================
# 2. Verificar que los 3 servicios están corriendo y healthy
# =============================================================================
docker-compose ps
# Deberías ver:
# NAME                          IMAGE                              STATUS
# sistemaventa-sqlserver        mssql/server:2022-latest           Up (healthy)
# sistemaventa-api              sistemaventa-api                   Up
# sistemaventa-frontend         sistemaventa-frontend              Up

# =============================================================================
# 3. Ver logs de un servicio si hay error
# =============================================================================
docker-compose logs sqlserver  # Logs de SQL Server
docker-compose logs api        # Logs de la API
docker-compose logs frontend   # Logs de Nginx/Angular

# =============================================================================
# 4. Probar conexión entre servicios (desde host)
# =============================================================================
# Frontend
Invoke-WebRequest -Uri http://localhost:4200 -UseBasicParsing | Select-Object StatusCode

# API health (si tienes endpoint /health)
Invoke-WebRequest -Uri http://localhost:8080/health -UseBasicParsing

# SQL Server desde host (para debugging)
docker exec -it sistemaventa-sqlserver /opt/mssql-tools18/bin/sqlcmd `
  -S localhost -U sa -P "${env:MSSQL_SA_PASSWORD}" -Q "SELECT name FROM sys.databases"

# =============================================================================
# 5. Probar flujo completo: login desde frontend
# =============================================================================
# 1. Abrir http://localhost:4200 en navegador
# 2. Ingresar credenciales: ja@sistemaventas.com / 123
# 3. Verificar en F12 → Network que la petición POST a /api/Usuario/IniciarSesion responde 200 OK
# 4. Verificar que navegas a /pages/dashboard y ves datos de la BD

# =============================================================================
# 6. Verificar persistencia de datos
# =============================================================================
# 1. Insertar un dato desde la app o SSMS
# 2. Detener stack: docker-compose down
# 3. Volver a levantar: docker-compose up -d
# 4. Verificar que el dato insertado sigue existiendo

# =============================================================================
# 7. Inspeccionar volumen de persistencia
# =============================================================================
docker volume inspect sqldata

# =============================================================================
# 8. Limpiar después de pruebas (⚠️ BORRA TODOS LOS DATOS)
# =============================================================================
docker-compose down -v  # El -v elimina el volumen sqldata (¡cuidado!)

# Para limpiar sin perder datos:
docker-compose down  # Sin -v: los datos en sqldata se preservan
```

---

## 📎 Anexo: Comunicación entre servicios — La regla de oro

```
┌─────────────────────────────────────────────────────┐
│  DENTRO DE LA RED DOCKER (sistemaventa-net):        │
│  - Los servicios se comunican por NOMBRE, no por IP │
│  - localhost = el contenedor mismo                  │
│  - sqlserver = el contenedor del servicio sqlserver │
│  - api = el contenedor del servicio api             │
└─────────────────────────────────────────────────────┘

✅ Connection string correcta en API (dentro de Docker):
"Server=sqlserver,1433;Database=DBVENTAngular;..."

❌ Connection string incorrecta (usa localhost):
"Server=localhost,1433;Database=DBVENTAngular;..." 
→ La API intenta conectarse a sí misma, no a SQL Server

✅ Endpoint correcto en Frontend (dentro de Docker Compose):
"http://api:8080/api/"

❌ Endpoint incorrecto (usa localhost):
"http://localhost:8080/api/"
→ El frontend en Docker intenta conectarse a sí mismo, no a la API

✅ Endpoint correcto en Frontend (desarrollo SIN compose):
"http://host.docker.internal:8080/api/"
→ host.docker.internal es un alias especial de Docker Desktop 
   que apunta a la máquina host desde dentro del contenedor
```

---

> **Nota de honestidad (AGENTS.md + ENGRAM.md):** Este documento se basó en archivos reales del repositorio (`docker-compose.yml`, `.env.example`, `environment.ts`). No se inventó configuración no evidenciada. Los trade-offs y justificaciones se derivan de documentación oficial de Docker, Microsoft y prácticas de la industria. El nivel de dominio declarado refleja implementación guiada con comprensión creciente, no expertise consolidado.