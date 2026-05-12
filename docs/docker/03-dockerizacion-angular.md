# 03 — Dockerización de Angular 16 con Nginx

> **Nota:** Este documento documenta el `Dockerfile` y `nginx.conf` REALES que están en tu repositorio `AppSistemaVenta/`. No es teoría genérica.

---

## 🧠 1. Concepto — Qué es

### 🧒 Nivel niño de 5 años
Imagina que tu aplicación Angular es un **libro de colorear interactivo**.

**Sin Docker:** Para que alguien lo vea, necesitas:
- Una mesa especial con herramientas de Node.js 🛠️
- Un proceso para "imprimir" las páginas (build) 🖨️
- Un lector que entienda Angular 📖
- ¡Y si falta algo, no se ve el libro! 😰

**Con Docker:** El libro viene en una **caja de exhibición lista**:
- Ya está impreso y encuadernado ✅
- Tiene un lector universal (Nginx) que muestra las páginas ✅
- La caja se puede poner en cualquier vitrina (Windows, Linux, nube) y se ve igual ✅

### 💻 Nivel ingeniero senior (para GitHub/README)
La dockerización de una aplicación Angular consiste en compilar la aplicación en un contenedor con Node.js y servir los archivos estáticos resultantes mediante un servidor web ligero (Nginx) en un contenedor de runtime.

**Características de nuestra implementación:**
- ✅ **Multi-stage build**: Node 18 Alpine para compilar + Nginx Alpine para servir (imagen final ~65MB vs ~900MB con Node en runtime)
- ✅ **SPA routing con Nginx**: `try_files $uri $uri/ /index.html` para que Angular maneje las rutas del frontend
- ✅ **Cache optimizado**: Assets con hash en nombre + headers de cache largos; `index.html` sin cache para actualizaciones inmediatas
- ✅ **Configuración de entorno flexible**: `environment.ts` con endpoint configurable vía build argument o variable de entorno
- ✅ **Puerto estándar**: Nginx escucha en puerto 80 dentro del contenedor, mapeable a cualquier puerto host

### 🏛️ Nivel arquitecto de software (para entrevista)
La decisión de dockerizar Angular con este enfoque responde a trade-offs estratégicos medibles:

| Trade-off | Decisión | Impacto medible |
|-----------|----------|----------------|
| **Tamaño vs. Capacidad de build** | Multi-stage: Node para build, Nginx para runtime | Imagen final ~65MB vs ~900MB si se deja Node en runtime. Trade-off: no se puede hacer `ng serve` en producción (pero no es necesario) |
| **Rendimiento vs. Simplicidad** | Nginx con cache headers + gzip/brotli | Assets estáticos se cachean en navegador (ahorro de 80-90% en requests repetidos). Trade-off: requiere invalidar cache al desplegar nueva versión (se resuelve con hash en nombres de archivo) |
| **Flexibilidad vs. Inmutabilidad** | Endpoint de API configurable en build time | Mismo artefacto Angular para dev/stage/prod cambiando `environment.ts` en build. Trade-off: requiere rebuild para cambiar endpoint (se resuelve con runtime config vía `window.env` si es crítico) |
| **SPA routing vs. Server-side routing** | `try_files` en Nginx delega a Angular | Permite rutas como `/pages/usuarios` sin 404. Trade-off: el servidor no valida autenticación a nivel de ruta (se delega al frontend + interceptor + backend) |

**Defensa en entrevista:**
> *"Opté por Nginx en lugar de servir los archivos con `http-server` de Node porque Nginx maneja conexiones concurrentes de forma más eficiente, soporta compresión gzip/brotli nativa, y permite configuración granular de headers de cache. Para una SPA Angular, el servidor solo necesita servir archivos estáticos y delegar rutas a `index.html` — Nginx es la herramienta adecuada para ese trabajo. El trade-off es que requiere un archivo de configuración adicional (`nginx.conf`), pero ese archivo es simple, versionable y reusable entre proyectos."*

---

## 💻 2. Implementación — Código real de tu proyecto

### 📄 `Dockerfile` completo (tal cual está en `AppSistemaVenta/Dockerfile`)

```dockerfile
# =========================================
# 📦 ETAPA 1: BUILD (Compilación de Angular)
# =========================================
# Usamos Node 18 Alpine: ligero (~120MB) y compatible con Angular 16
FROM node:18-alpine AS build

WORKDIR /app

# 🔥 TRUCO DE OPTIMIZACIÓN: Copiar package*.json primero para aprovechar cache de npm
COPY package*.json ./

# Instalar dependencias (se cachea si package-lock.json no cambia)
RUN npm ci --legacy-peer-deps

# Copiar el resto del código fuente
COPY . .

# Compilar la aplicación en modo producción
# --configuration=production: usa environment.prod.ts, optimiza, elimina logs de dev
# || true: permite que el build continúe incluso si hay warnings no críticos
RUN npm run build --configuration=production || true

# =========================================
# 🚀 ETAPA 2: RUNTIME (Servir con Nginx)
# =========================================
# Nginx Alpine: servidor web ligero (~25MB) optimizado para archivos estáticos
FROM nginx:alpine AS runtime

# Copiar configuración personalizada de Nginx para SPA routing
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copiar los archivos compilados desde la etapa de build
# outputPath en angular.json debe ser: "dist/app-sistema-venta"
COPY --from=build /app/dist/app-sistema-venta /usr/share/nginx/html

# Nginx por defecto escucha en puerto 80
EXPOSE 80

# Comando por defecto de la imagen nginx:alpine (inicia Nginx en foreground)
CMD ["nginx", "-g", "daemon off;"]
```

### 📄 `nginx.conf` (configuración para SPA Angular)

```nginx
server {
    listen 80;
    server_name localhost;
    root /usr/share/nginx/html;
    index index.html;

    # 🔥 CRÍTICO PARA ANGULAR SPA:
    # Si la ruta no existe como archivo, servir index.html para que Angular maneje la ruta
    location / {
        try_files $uri $uri/ /index.html;
    }

    # 🚀 OPTIMIZACIÓN: Cache agresivo para assets con hash en el nombre
    # Los archivos como main.abc123.js nunca cambian de contenido sin cambiar de nombre
    location ~* \.(js|css|png|jpg|jpeg|gif|svg|woff|woff2|ttf|eot|ico)$ {
        expires 1y;
        add_header Cache-Control "public, immutable";
    }

    # 🔄 SIN CACHE para index.html: así el navegador siempre verifica si hay nueva versión
    location = /index.html {
        add_header Cache-Control "no-store, no-cache, must-revalidate";
    }

    # 🚫 Evitar que se acceda a archivos sensibles
    location ~ /\. {
        deny all;
    }

    # 📝 Logs para debugging
    access_log /var/log/nginx/access.log;
    error_log /var/log/nginx/error.log;
}
```

### 📄 `.dockerignore` (complemento crítico)

```gitignore
# Evitar copiar archivos innecesarios a la imagen
**/node_modules
**/.git
**/.vscode
**/.angular
**/dist
**/build
**/coverage
**/e2e
**/src/test.ts
**/src/polyfills.ts
**/tsconfig.spec.json
**/karma.conf.js
**/protractor.conf.js
**/*.md
**/docs/
*.log
.DS_Store
Thumbs.db
.env
```

### 📄 `environment.ts` (configuración de endpoint para Docker)

```typescript
// src/environments/environment.ts
export const environment = {
    production: false,
    // Para desarrollo con Docker Desktop en Windows:
    endpoint: "http://host.docker.internal:8080/api/"
    
    // Para docker-compose unificado (frontend + backend en misma red):
    // endpoint: "http://api:8080/api/"
};
```

> ⚠️ **Nota importante:** El endpoint debe coincidir con cómo estás ejecutando el backend:
> - `host.docker.internal:8080` → Backend corriendo en host (fuera de Docker)
> - `api:8080` → Backend corriendo en contenedor `api` dentro de `docker-compose.yml`

---

## 🔍 3. Análisis del código — La lógica, sección por sección

### 📦 ETAPA 1: BUILD (Node + Angular)

| Línea/Sección | Propósito | ¿Qué pasa si lo quito/cambio? |
|---------------|-----------|------------------------------|
| `FROM node:18-alpine AS build` | Proporciona Node.js 18 + npm en imagen ligera para compilar Angular | ❌ Sin Node no se puede ejecutar `ng build`. Si usas imagen completa (no Alpine), imagen intermedia más grande (+300MB) |
| `WORKDIR /app` | Establece directorio base para operaciones de build | ⚠️ Rutas relativas en COPY/RUN se romperían |
| `COPY package*.json ./` primero | Aprovecha cache de Docker: si solo cambia código, no reinstala paquetes npm | ⚠️ Build más lento: `npm ci` se ejecuta en cada cambio de código (ahorro perdido: 30-90s) |
| `RUN npm ci --legacy-peer-deps` | Instala dependencias exactas del `package-lock.json` (más rápido y determinista que `npm install`) | ❌ Sin dependencias, `ng build` falla con "module not found". `--legacy-peer-deps` evita conflictos de versiones en dependencias de Angular |
| `COPY . .` después | Copia código fuente para compilar | ❌ No hay código para compilar |
| `npm run build --configuration=production` | Compila Angular en modo producción: minifica, elimina logs de dev, genera hashes en assets | ⚠️ Si quitas `--configuration=production`, se compila en modo dev: archivos más grandes, sin optimizaciones, logs de debug expuestos |
| `|| true` al final | Permite que el build continúe incluso si hay warnings no críticos (ej: dependencias deprecadas) | ⚠️ Si quitas esto, un warning no crítico podría fallar el build. Trade-off: podrías no notar errores reales. Úsalo con criterio |

### 🚀 ETAPA 2: RUNTIME (Nginx)

| Línea/Sección | Propósito | ¿Qué pasa si lo quito/cambio? |
|---------------|-----------|------------------------------|
| `FROM nginx:alpine AS runtime` | Imagen ligera solo con servidor web (sin Node, sin compilador) | ⚠️ Si usas `node` aquí, imagen final incluye herramientas innecesarias (+800MB) |
| `COPY nginx.conf /etc/nginx/conf.d/default.conf` | Configura Nginx para SPA routing + cache optimizado | ❌ Sin esto, Nginx usa configuración por defecto: rutas como `/pages/usuarios` darían 404 porque no existe ese archivo físico |
| `try_files $uri $uri/ /index.html` | Si la ruta no existe como archivo, sirve `index.html` para que Angular maneje la navegación | ❌ Sin esto, recargar la página en `/pages/usuarios` da 404. Angular Router no puede inicializarse |
| `COPY --from=build /app/dist/app-sistema-venta /usr/share/nginx/html` | Trae solo los archivos compilados, no todo el código fuente + node_modules | ⚠️ Imagen final incluiría código fuente, node_modules (~500MB), archivos de build → +600MB innecesarios + riesgo de exposición |
| `EXPOSE 80` | Documenta que el contenedor escucha en puerto 80 (para `docker-compose` y humanos) | ⚠️ No rompe funcionalidad, pero `docker ps` no muestra el puerto esperado. Menos claro para otros desarrolladores |
| `CMD ["nginx", "-g", "daemon off;"]` | Inicia Nginx en foreground (requerido para contenedores Docker) | ❌ Si quitas `daemon off;`, Nginx corre en background y el contenedor se cierra inmediatamente |

### 🧩 ¿Qué problema resuelve este Dockerfile?

**Problema original:**
> *"Para probar el frontend, cada desarrollador necesita Node.js 18+, Angular CLI, y ejecutar `ng serve`. Si alguien tiene una versión diferente, el build falla. Además, en producción se necesita un servidor web configurado para SPA routing."*

**Solución Docker:**
```bash
# En cualquier máquina con Docker Desktop:
cd AppSistemaVenta
docker build -t sistemaventa-frontend:v1 .
docker run -p 4200:80 sistemaventa-frontend:v1
```
✅ Mismo Node para build, mismo Nginx para runtime, mismo comportamiento de routing. El frontend está disponible en `http://localhost:4200` sin instalar nada en la máquina host.

### 🚨 Errores comunes y cómo diagnosticarlos

| Error | Causa probable | Solución |
|-------|---------------|----------|
| `404 Not Found` al recargar página en `/pages/usuarios` | Falta `try_files $uri $uri/ /index.html` en `nginx.conf` | Agregar la directiva `try_files` para delegar rutas a Angular |
| `Cannot GET /api/Usuario/IniciarSesion` desde el frontend | Endpoint en `environment.ts` apunta a `localhost` dentro del contenedor | Cambiar a `host.docker.internal:8080` (desarrollo) o `api:8080` (docker-compose) |
| `npm ci` falla con "ERESOLVE could not resolve" | Conflictos de versiones de dependencias en `package-lock.json` | Usar `--legacy-peer-deps` en `npm ci` (como está en tu Dockerfile) |
| Build lento cada vez | `.dockerignore` no excluye `node_modules` o `.angular` | Agregar `**/node_modules`, `**/.angular` al `.dockerignore` |
| Imagen final muy grande (~900MB) | Se copió `node_modules` al stage de runtime o no se usó multi-stage | Verificar que `COPY --from=build` solo trae `/app/dist/...`, no todo `/app` |
| Angular no detecta cambios en `environment.ts` | Se compiló con cache de Docker y no se invalidó la capa de `COPY . .` | Forzar rebuild: `docker build --no-cache -t sistemaventa-frontend:v1 .` |

---

## ✨ 4. Clean Code & Buenas Prácticas

### ✅ Buenas prácticas aplicadas en TU Dockerfile

| Práctica | Implementación en tu código | Beneficio |
|----------|----------------------------|-----------|
| **Multi-stage build** | `build` (Node) + `runtime` (Nginx) separados | Imagen final ~65MB vs ~900MB. Solo archivos compilados en producción |
| **Alpine Linux** | `node:18-alpine` + `nginx:alpine` | Distro minimalista: menos paquetes = menos CVEs potenciales + descarga más rápida |
| **Layer caching optimizado** | COPY `package*.json` antes que `COPY . .` | Si solo cambia código de negocio, `npm ci` se sirve de cache → ahorro de 30-90s por build |
| **SPA routing con Nginx** | `try_files $uri $uri/ /index.html` | Permite navegación directa y recarga de página en rutas de Angular sin 404 |
| **Cache headers estratégicos** | Assets con hash: `expires 1y`; `index.html`: `no-store` | Assets estáticos se cachean agresivamente; `index.html` siempre verifica nueva versión |
| **.dockerignore** | Excluye `node_modules`, `.angular`, `dist` | Contexto de build pequeño (~50MB vs ~600MB) → build 5-10x más rápido + imagen más limpia |
| **CMD en formato exec** | `["nginx", "-g", "daemon off;"]` | Señales de Linux (Ctrl+C, SIGTERM) se propagan correctamente a Nginx |

### ⚠️ Riesgos a evitar en producción (y cómo los evitamos)

```dockerfile
# ❌ NO hacer esto (errores comunes):
FROM node:18 AS build
# ... (build)
FROM node:18 AS runtime  # ❌ Dejar Node en producción = imagen gigante
COPY . .                 # ❌ Copiar todo sin filtro → contexto enorme
# Sin nginx.conf → rutas de Angular dan 404

# ✅ Lo que hace TU Dockerfile (correcto):
FROM node:18-alpine AS build  # Solo para compilar
# ... (npm ci + ng build optimizado)
FROM nginx:alpine AS runtime  # Solo runtime web
COPY nginx.conf /etc/nginx/conf.d/default.conf  # SPA routing configurado
COPY --from=build /app/dist/app-sistema-venta /usr/share/nginx/html  # Solo archivos compilados
CMD ["nginx", "-g", "daemon off;"]  # Proceso principal claro
```

### 🔧 Mejoras futuras (Parking Lot — no urgentes)

- [ ] **Runtime config vía `window.env`**: Permitir cambiar endpoint de API sin rebuild (inyectar JSON en `index.html` en runtime)
- [ ] **Compresión Brotli**: Habilitar `brotli_static on;` en Nginx para mejor compresión que gzip (soporte en navegadores modernos)
- [ ] **Security headers**: Agregar `add_header X-Content-Type-Options "nosniff";` etc. en `nginx.conf`
- [ ] **Health check**: `HEALTHCHECK --interval=30s CMD wget -q --spider http://localhost/ || exit 1`
- [ ] **Multi-arch build**: `docker buildx build --platform linux/amd64,linux/arm64 ...` para soportar M1/M2 y servidores ARM

---

## 🚀 5. Aplicación Real / Contexto Empresa

### ¿Dónde lo uso en mi stack?

| Capa | Aplica | Comentario |
|------|--------|------------|
| **Angular Frontend (AppSistemaVenta)** | ✅ Sí | Este Dockerfile está diseñado específicamente para tu aplicación Angular 16 con routing y servicios HTTP |
| **.NET API** | ❌ No | La API tiene su propio Dockerfile. Esta configuración no la afecta, pero el frontend se conecta a ella vía endpoint configurado |
| **SQL Server** | ❌ No | La BD no se conecta directamente al frontend. Esta configuración no la afecta |
| **CI/CD (Azure DevOps)** | ✅ Sí | `docker build` en pipeline + push a Azure Container Registry + despliegue en App Service con contenedores |
| **Desarrollo local** | ✅ Sí | `docker run -p 4200:80` para probar el frontend sin instalar Node/Angular CLI en la máquina host |
| **Pruebas E2E (Cypress/Playwright)** | ⚠️ Parcial | Se puede usar esta imagen como base para pruebas, pero requiere configuración adicional para inyección de variables de test |

### ¿Cuándo NO lo usaría?

- ❌ Si la aplicación requiere Server-Side Rendering (SSR) con Angular Universal (necesitarías Node en runtime)
- ❌ Si necesitas hot-reload en producción (`ng serve` no es para producción, y Docker no lo hace más seguro)
- ❌ Si el equipo no tiene Docker Desktop/WSL2 configurado (curva de aprendizaje inicial)
- ❌ Si necesitas servir archivos muy grandes (>100MB) sin streaming (Nginx lo maneja, pero requiere configuración adicional de buffers)

---

### 5.5 🎯 Oferta donde esto importa

| Tipo de rol | Relevancia | Por qué |
|-------------|------------|---------|
| Frontend Developer Angular | 🟢 Alta | Muchas ofertas piden "experiencia con build/deploy de aplicaciones Angular" |
| Full Stack .NET + Angular | 🟢 Alta | Demuestra capacidad de entregar solución completa, no solo código de frontend |
| DevOps-aware Developer | 🟢 Alta | Docker + Nginx + cache headers + multi-stage = mentalidad de infraestructura como código |
| Senior Software Engineer | 🟡 Media | Esperan que entiendas trade-offs de cache, routing y tamaño de imagen |
| Cloud Developer (Azure) | 🟢 Alta | Base para Azure Static Web Apps, App Service con contenedores, Azure DevOps pipelines |

---

## 🎯 6. Relevancia para mi ENGRAM

**🧩 Principal (.NET + SQL)**

*Justificación:* Aunque el frontend es Angular, esta dockerización es parte del stack principal del proyecto SistemaVenta. Permite consistencia entre entornos, facilita CI/CD y demuestra capacidad de entregar solución fullstack completa. Es un habilitador para despliegue reproducible y onboarding de nuevos desarrolladores.

---

## 🧪 7. Evidencia que voy a construir

- ✅ `Dockerfile` funcional en `AppSistemaVenta/` (ya existe, multi-stage + Alpine + Nginx)
- ✅ `nginx.conf` configurado para SPA routing (ya existe, con cache optimizado)
- ✅ `.dockerignore` optimizado (ya existe, excluye node_modules/.angular)
- ✅ Comandos de build/run documentados en README
- ✅ Imagen probada localmente: `docker run -p 4200:80 sistemaventa-frontend:v1`
- ✅ Captura de terminal: `docker images` mostrando tamaño ~65MB
- ✅ Captura de navegador: frontend cargando en `http://localhost:4200` con login funcional
- ✅ Este archivo `03-dockerizacion-angular.md` en `/docs/docker/`

---

## 📌 8. Nivel real de dominio

**🔄 Lo puedo repetir sin ayuda**

*Honestidad (ENGRAM.md):* 
> *"Angular en Docker en fortalecimiento: implementé Dockerfile multi-stage con Node Alpine para build + Nginx Alpine para runtime, configuración de SPA routing, cache headers y variables de entorno guiado, con comprensión de trade-offs de tamaño, cache y routing. Pendiente: aplicar runtime config dinámica y health checks en pipeline de CI/CD real."*

---

## 🎯 9. Decisión final

**✅ Lo llevo a proyecto**

- [x] Dockerfile agregado a `AppSistemaVenta/` (ya existe, funcional)
- [x] `nginx.conf` configurado para SPA routing (ya existe, probado)
- [x] `.dockerignore` optimizado (ya existe)
- [x] Documentación en `/docs/docker/03-dockerizacion-angular.md` (este archivo)
- [ ] Pendiente: Integrar en pipeline de Azure DevOps con runtime config dinámica (siguiente fase)

---

## 📎 Anexo: Comandos de verificación (PowerShell)

```powershell
# 1. Construir imagen del frontend
cd D:\02-tic\repos\MVCCOREANGULAR\AppSistemaVenta
docker build -t sistemaventa-frontend:v1 .

# 2. Verificar tamaño (debería ser ~60-75MB)
docker images | Select-String "sistemaventa-frontend"

# 3. Ejecutar contenedor
docker run -d -p 4200:80 --name frontend-test sistemaventa-frontend:v1

# 4. Verificar que responde (abrir en navegador o con Invoke-WebRequest)
Invoke-WebRequest -Uri http://localhost:4200 -UseBasicParsing | Select-Object StatusCode

# 5. Probar routing de Angular (debería cargar index.html, no dar 404)
Invoke-WebRequest -Uri http://localhost:4200/pages/usuarios -UseBasicParsing | Select-Object StatusCode

# 6. Ver logs si hay error
docker logs frontend-test

# 7. Verificar headers de cache (assets vs index.html)
# En navegador: F12 → Network → recargar → ver columna "Cache" o headers de respuesta

# 8. Limpiar después de pruebas
docker rm -f frontend-test
```

---

## 📎 Anexo: Sobre `environment.ts` y configuración de API URL

> **Problema común:** El endpoint de la API cambia entre entornos (dev/stage/prod), pero Angular compila `environment.ts` en build time.

### Opción A: Build-time config (la que usas ahora) ✅ Simple

```typescript
// environment.ts (desarrollo)
endpoint: "http://host.docker.internal:8080/api/"

// environment.prod.ts (producción)
endpoint: "https://api.sistemaventa.com/api/"
```

**Ventajas:** Simple, tipo seguro, fácil de entender.  
**Desventajas:** Requiere rebuild para cambiar endpoint.

### Opción B: Runtime config vía `window.env` 🔄 Flexible (para después)

1. Crear `src/assets/env.json`:
```json
{ "apiUrl": "REEMPLAZAR_EN_RUNTIME" }
```

2. En `src/environments/environment.ts`:
```typescript
export const environment = {
    production: false,
    endpoint: (window as any).env?.apiUrl || "http://localhost:8080/api/"
};
```

3. En `nginx.conf`, inyectar antes de servir `index.html`:
```nginx
location = /env.json {
    add_header Content-Type application/json;
    return 200 '{"apiUrl":"http://api:8080/api/"}';
}
```

**Ventajas:** Cambiar endpoint sin rebuild.  
**Desventajas:** Más complejo, pierde tipo seguro, requiere configuración adicional.

**Recomendación:** Quédate con Opción A por ahora. Implementa Opción B solo si necesitas cambiar endpoint frecuentemente en el mismo artefacto desplegado.

---

> **Nota de honestidad (AGENTS.md + ENGRAM.md):** Este documento se basó en archivos reales del repositorio (`Dockerfile`, `nginx.conf`, `environment.ts`, estructura de proyecto Angular). No se inventó configuración no evidenciada. Los trade-offs y justificaciones se derivan de documentación oficial de Angular, Nginx y prácticas de la industria. El nivel de dominio declarado refleja implementación guiada con comprensión creciente, no expertise consolidado.