USE master;
GO

IF EXISTS (SELECT name FROM sys.databases WHERE name = N'DBVENTAngular')
BEGIN
    ALTER DATABASE [DBVENTAngular] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [DBVENTAngular];
END
GO

CREATE DATABASE [DBVENTAngular];
GO

USE [DBVENTAngular];
GO

-- TABLAS
CREATE TABLE [dbo].[Rol]( [IdRol] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY, [Nombre] [varchar](50) NULL, [FechaRegistro] [datetime] DEFAULT GETDATE() );
GO
CREATE TABLE [dbo].[Menu]( [IdMenu] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY, [Nombre] [varchar](50) NULL, [Icono] [varchar](50) NULL, [Url] [varchar](50) NULL );
GO
CREATE TABLE [dbo].[MenuRol]( [IdMenuRol] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY, [IdMenu] [int] NULL, [IdRol] [int] NULL, FOREIGN KEY ([IdMenu]) REFERENCES [Menu]([IdMenu]), FOREIGN KEY ([IdRol]) REFERENCES [Rol]([IdRol]) );
GO
CREATE TABLE [dbo].[Usuario](
    [IdUsuario] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY, [NombreCompleto] [varchar](100) NULL, [Correo] [varchar](40) NULL, [IdRol] [int] NULL,
    [Clave] [varchar](255) NULL, -- CORREGIDO: 255 para BCrypt
    [UrlFoto] [varchar](255) NULL,
    [EsActivo] [bit] DEFAULT 1, [FechaRegistro] [datetime] DEFAULT GETDATE(),
    FOREIGN KEY ([IdRol]) REFERENCES [Rol]([IdRol])
);
GO
CREATE TABLE [dbo].[RefreshToken](
    [Id] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY, [IdUsuario] [int] NOT NULL, [Token] [varchar](255) NULL,
    [FechaCreacion] [datetime] DEFAULT GETDATE(), [FechaExpiracion] [datetime] NULL,
    [Revocado] [bit] DEFAULT 0, [Activo] [bit] DEFAULT 1, -- CORREGIDO: Columna Activo
    FOREIGN KEY ([IdUsuario]) REFERENCES [Usuario]([IdUsuario]) ON DELETE CASCADE
);
GO
CREATE TABLE [dbo].[Categoria]( [IdCategoria] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY, [Nombre] [varchar](50) NULL, [EsActivo] [bit] DEFAULT 1, [FechaRegistro] [datetime] DEFAULT GETDATE() );
GO
CREATE TABLE [dbo].[Producto]( [IdProducto] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY, [Nombre] [varchar](100) NULL, [IdCategoria] [int] NULL, [Stock] [int] NULL, [Precio] [decimal](18, 2) NULL, [UrlImagen] [varchar](255) NULL, [EsActivo] [bit] DEFAULT 1, [FechaRegistro] [datetime] DEFAULT GETDATE(), FOREIGN KEY ([IdCategoria]) REFERENCES [Categoria]([IdCategoria]) );
GO
CREATE TABLE [dbo].[NumeroDocumento]( [IdNumeroDocumento] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY, [Ultimo_Numero] [int] NOT NULL, [FechaRegistro] [datetime] DEFAULT GETDATE() );
GO
CREATE TABLE [dbo].[Venta]( [IdVenta] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY, [NumeroDocumento] [varchar](40) NULL, [TipoPago] [varchar](50) NULL, [Total] [decimal](18, 2) NULL, [FechaRegistro] [datetime] DEFAULT GETDATE() );
GO
CREATE TABLE [dbo].[DetalleVenta]( [IdDetalleVenta] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY, [IdVenta] [int] NULL, [IdProducto] [int] NULL, [Cantidad] [int] NULL, [Precio] [decimal](18, 2) NULL, [Total] [decimal](18, 2) NULL, FOREIGN KEY ([IdVenta]) REFERENCES [Venta]([IdVenta]), FOREIGN KEY ([IdProducto]) REFERENCES [Producto]([IdProducto]) );
GO

-- DATOS
INSERT INTO Rol (Nombre) VALUES ('Administrador'), ('Supervisor'), ('Empleado');
INSERT INTO Menu (Nombre, Icono, Url) VALUES ('DashBoard','dashboard','/pages/dashboard'), ('Usuarios','group','/pages/usuarios'), ('Productos','collections_bookmark','/pages/productos'), ('Venta','currency_exchange','/pages/venta'), ('Historial Ventas','edit_note','/pages/historial_venta'), ('Reportes','receipt','/pages/reportes');
INSERT INTO MenuRol (IdMenu, IdRol) VALUES (1,1),(2,1),(3,1),(4,1),(5,1),(6,1);
INSERT INTO MenuRol (IdMenu, IdRol) VALUES (4,2),(5,2);
INSERT INTO MenuRol (IdMenu, IdRol) VALUES (3,3),(4,3),(5,3),(6,3);
INSERT INTO Categoria (Nombre) VALUES ('Laptops'),('Monitores'),('Teclados'),('Auriculares'),('Memorias'),('Accesorios');
INSERT INTO NumeroDocumento (Ultimo_Numero) VALUES (0);
GO 

IF NOT EXISTS (SELECT 1 FROM Usuario WHERE Correo = 'admin@sistema.com')
BEGIN
-- SCRIPT USUARIOS INICIALES
-- Generado con BCrypt Work Factor 11

-- Usuario: admin@sistema.com | Clave: Admin2026!
INSERT INTO Usuario (nombreCompleto, Correo, IdRol, Clave, UrlFoto, EsActivo, fechaRegistro)
VALUES ('Administrador Sistema', 'admin@sistema.com', 1, '$2a$11$llxXYZR754F3aA05LRSiWOO1B2WanRt7MbwKJFtPYr9LwfGDhDQJi', '/imagenes/usuarios/Foto001.JPG', 1, '2026-05-25 11:30:19');

-- Usuario: supervisor@sistema.com | Clave: Super2026!
INSERT INTO Usuario (nombreCompleto, Correo, IdRol, Clave, UrlFoto, EsActivo, fechaRegistro)
VALUES ('Supervisor Demo', 'supervisor@sistema.com', 2, '$2a$11$KVgSFMNkLu7OG77vtuFZeu/zc/y//1C7rAyyxUWuAVThyl9Jmwyra', '/imagenes/usuarios/Foto002.JPG', 1, '2026-05-25 11:30:19');

-- Usuario: empleado@sistema.com | Clave: Emple2026!
INSERT INTO Usuario (nombreCompleto, Correo, IdRol, Clave, UrlFoto, EsActivo, fechaRegistro)
VALUES ('Empleado Demo', 'empleado@sistema.com', 3, '$2a$11$bHRIE3znh40z23tryUBP3.SyRQngM1fw/.1fp25tMhk/7XeDbepKq', '/imagenes/usuarios/Foto003.JPG', 1, '2026-05-25 11:30:19');


END 
GO