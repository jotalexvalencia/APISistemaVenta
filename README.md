# Sistema de Ventas (.NET 10 + Angular)

## 📖 Descripción

> Sistema de gestión de ventas desarrollado con arquitectura en capas (N-Tier Architecture). Implementa autenticación moderna mediante **JWT (JSON Web Tokens)** y documentación interactiva con **Scalar**.

## 🛠️ Stack Tecnológico

* **Backend**: .NET 10, ASP.NET Core Web API, Entity Framework Core.
* **Frontend**: Angular 17+ (En desarrollo).
* **Base de Datos**: SQL Server.
* **Documentación**: Scalar (OpenAPI).
* **Seguridad**: JWT, Refresh Token (Pendiente).

## **🚀 Características Principales**

* **Autenticación Segura**: Generación de Tokens JWT firmados con HMAC-SHA256.
* **Autorización por Roles**: Soporte para Administrador, Supervisor y Empleado.
* **Documentación Interactiva**: Explora la API directamente desde el navegador con Scalar.
* **Arquitectura Limpia**: Separación de responsabilidades (API, BLL, DAL, Model, DTO).

## ⚙️ Instalación y Ejecución

1. **Clonar el repositorio**:
   `git clone https://github.com/jotalexvalencia/APISistemaVenta.git`
2. **Configurar la cadena de conexión**:Modificar appsettings.json con tu instancia de SQL Server.
3. **Ejecutar la API**:
   `dotnet run`
4. **Acceder a la documentación**:Navegar a https://localhost:PUERTO/scalar/v1.

## 🔐 Endpoint de Prueba (Login)

POST /api/Usuario/IniciarSesion

`{  "correo": "admin@example.com",  "clave": "tu_clave"}`


👤 **Autor**
**Jorge Alexander Valencia**

[Linkedin]() | [GitHub](https://github.com/jotalexvalencia)
