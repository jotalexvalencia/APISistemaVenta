# Sistema de Ventas (.NET 10 + Angular)

## 📖 Descripción

> API REST robusta para la gestión de ventas, desarrollada con **.NET 10** y arquitectura en capas. Implementa estándares modernos de seguridad y documentación interactiva.

## 🛡️ Arquitectura de Seguridad

Este proyecto implementa un sistema de autenticación stateless basado en **JWT (JSON Web Tokens)** con soporte para  **Refresh Tokens** .

**Flujo de Autenticación:**

1. El usuario inicia sesión y recibe un `accessToken` (dura 1 hora) y un `refreshToken` (dura 7 días).
2. El `accessToken` se usa para acceder a endpoints protegidos.
3. Cuando el `accessToken` expira, el cliente usa el `refreshToken` para obtener un nuevo par de tokens.
4. El Refresh Token antiguo se invalida automáticamente en la base de datos (Rotación de Tokens).

**Características de Seguridad:**

* Hasheado de contraseñas con  **BCrypt** .
* Validación de unique email.
* Tokens con firma HMAC-SHA256.

## 🛠️ Stack Tecnológico

* **Backend:** .NET 10, ASP.NET Core Web API.
* **Base de Datos:** SQL Server.
* **ORM:** Entity Framework Core.
* **Documentación:** Scalar (OpenAPI).
* **Arquitectura:** N-Tier Architecture (API, BLL, DAL, Model, DTO, Utility).

## 🚀 Endpoints Clave

### Autenticación

`POST /api/Usuario/IniciarSesion`

* **Input:** `{ "correo": "string", "clave": "string" }`
* **Output:** `{ "token": "jwt", "refreshToken": "guid" }`

`POST /api/Usuario/RenovarToken`

* **Input:** `"refresh_token_string"` (Raw string)
* **Output:** Nuevo par de tokens.

### Usuario

`GET /api/Usuario/Lista` (Requiere Autorización)`POST /api/Usuario/Guardar``PUT /api/Usuario/Editar`

## ⚙️ Instalación

1. Clonar repositorio.
2. Configurar cadena de conexión en `appsettings.json`.
3. Ejecutar `dotnet restore`.
4. Ejecutar `dotnet run`.
5. Navegar a `/scalar/v1` para ver la documentación.

👤 **Autor**
**Jorge Alexander Valencia**

[Linkedin]() | [GitHub](https://github.com/jotalexvalencia)
