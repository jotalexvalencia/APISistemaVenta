# 🗄️ Inicialización Automática de Base de Datos y Datos Semilla en Docker

Este documento detalla el diagnóstico del problema con la carga automática de usuarios semilla con contraseñas cifradas en BCrypt, la solución implementada para poblar correctamente las tablas del sistema (incluyendo menús y categorías), y una guía detallada sobre cómo documentar este tipo de cambios bajo el estándar de **Git (Conventional Commits)**.

---

## 🔍 1. Diagnóstico del Problema

### El Síntoma
* Al levantar los contenedores mediante `docker compose up`, la API se ejecutaba pero la tabla `Usuario` contenía registros incorrectos o valores `NULL` en el usuario administrador (`idUsuario: 1`).
* Adicionalmente, al iniciar sesión desde la aplicación Angular, la interfaz entraba correctamente pero **no se visualizaban los menús** (Dashboard, Ventas, Reportes, etc.) ni las categorías.
* El sistema solo funcionaba de manera esperada si se corría de forma manual un script de reset en la base de datos local.

### La Causa Raíz
1. **Volumen de base de datos persistente (`sqldata`)**:
   Docker almacena físicamente los archivos MDF y LDF de SQL Server en un volumen persistente. Si en el primer arranque de contenedores ocurrió un error o se usó un script a medias, ese estado corrupto (el usuario ID 1 con valores `NULL`) se guardó físicamente y se mantenía en cada reinicio.
2. **Mapeo Confuso del script en `docker-compose.yml`**:
   El servicio `db-init` montaba el archivo de reset (`99-reset-db.sql`) sobre el nombre temporal `/scripts/01-create-db.sql`. Esto causaba una desconexión semántica de qué script se estaba ejecutando en realidad.
3. **Falta de Datos Semilla en el Script de Inicialización**:
   El script oficial de creación [01-create-db.sql](file:///d:/02-tic/repos/MVCCOREANGULAR/APISistemaVenta/database/init/01-create-db.sql) creaba las tablas y los usuarios, pero **no insertaba los menús, las relaciones menú-rol, las categorías ni el número de documento inicial**. Por esta razón, el sistema iniciaba sesión correctamente pero la aplicación frontend no tenía menús que mostrar.

---

## 🛠️ 2. Solución Implementada

### Cambios en Docker Compose
Se modificó la sección de volúmenes de `db-init` en [docker-compose.yml](file:///d:/02-tic/repos/MVCCOREANGULAR/APISistemaVenta/docker-compose.yml) para apuntar directamente al script definitivo de inicialización:
```yaml
    volumes:
      - ./database/init/01-create-db.sql:/scripts/01-create-db.sql:ro
```

### Enriquecimiento de `01-create-db.sql`
Se incorporaron los datos semilla completos en [01-create-db.sql](file:///d:/02-tic/repos/MVCCOREANGULAR/APISistemaVenta/database/init/01-create-db.sql) utilizando condicionales de seguridad `IF NOT EXISTS`. Ahora el script de inicialización realiza:
* Creación de la base de datos, tablas, llaves primarias y foráneas de forma limpia.
* Inserción dinámica de los roles semilla (`Administrador`, `Supervisor`, `Empleado`).
* Inserción de usuarios semilla con sus hashes cifrados en **BCrypt** de 60 caracteres (factor de trabajo 11).
* **[NUEVO]** Carga de los 6 Menús del sistema (`DashBoard`, `Usuarios`, etc.) y sus correspondientes accesos por rol (`MenuRol`).
* **[NUEVO]** Carga de las 6 Categorías iniciales de productos (`Laptops`, `Monitores`, etc.).
* **[NUEVO]** Configuración inicial del correlativo de facturas (`NumeroDocumento` en `0`).

---

## 📜 3. Guía de Git y Commits Estandarizados (Conventional Commits)

Para llevar un historial limpio y profesional de cambios en Git, se utiliza el estándar **Conventional Commits**. Esto permite a otros desarrolladores entender al instante el propósito de cada cambio y automatizar notas de lanzamiento (changelogs).

La estructura de un mensaje de commit bajo este estándar es:
```
<tipo>(<alcance opcional>): <descripción corta en minúsculas>

[cuerpo opcional con más detalles]
```

### Lista de Tipos de Commit Estándar

| Tipo | Cuándo Usarlo | Ejemplo Real |
| :--- | :--- | :--- |
| **`feat`** | Cuando agregas una nueva funcionalidad al software. | `feat(auth): add refresh token rotation` |
| **`fix`** | Cuando corriges un error o bug del sistema. | `fix(db): add missing menu seed data to initialization script` |
| **`docs`** | Cuando modificas solo archivos de documentación. | `docs(docker): add guide for automatic db initialization` |
| **`style`** | Cambios visuales o de formato de código (espacios, comas, etc.) sin cambiar lógica. | `style(sql): normalize column casing to PascalCase` |
| **`refactor`**| Cambios de código que no corrigen errores ni añaden funciones (ej. optimizaciones).| `refactor(repo): optimize generic repository queries` |
| **`chore`** | Tareas repetitivas o de configuración que no tocan código de la app (ej. gitignore, npm). | `chore(deps): update dotnet sdk version in dockerfile` |
| **`test`** | Añadir o modificar pruebas unitarias o de integración. | `test(user): add unit tests for login validation` |

---

## 🚀 4. Instrucciones para tu Commit y Git Push

Para registrar y subir de manera profesional esta solución a tu repositorio en GitHub u Azure DevOps, ejecuta los siguientes comandos en tu terminal:

### Paso A: Registrar cambios en la zona de preparación (Staging)
```bash
# 1. Comprobar qué archivos se modificaron
git status

# 2. Agregar los archivos modificados
git add docker-compose.yml
git add database/init/01-create-db.sql
git add docs/docker/06-inicializacion-automatica-db.md
git add README.md
```

### Paso B: Realizar el Commit con el mensaje estandarizado
Para este cambio, el tipo más adecuado es **`fix`** (porque arregla el bug de la inicialización de menús y claves) o podemos separar en dos commits para mayor limpieza:

```bash
# Commit de corrección del bug
git commit -m "fix(db): automatically seed menus and categories during docker startup" -m "Corrects docker-compose volume mapping to run 01-create-db.sql and adds missing seed data (Menu, MenuRol, Categoria, Documento) with IF NOT EXISTS guards."

# Commit de documentación
git commit -m "docs(docker): add guide for automatic database initialization and conventional commits"
```

### Paso C: Subir los cambios a tu repositorio remoto
```bash
# Sube tus cambios a la rama principal (reemplaza 'main' por tu rama si es diferente)
git push origin main
```
