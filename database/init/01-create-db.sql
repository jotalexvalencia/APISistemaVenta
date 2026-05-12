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
CREATE TABLE [dbo].[Producto]( [IdProducto] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY, [Nombre] [varchar](100) NULL, [IdCategoria] [int] NULL, [Stock] [int] NULL, [Precio] [decimal](18, 2) NULL, [EsActivo] [bit] DEFAULT 1, [FechaRegistro] [datetime] DEFAULT GETDATE(), FOREIGN KEY ([IdCategoria]) REFERENCES [Categoria]([IdCategoria]) );
GO
CREATE TABLE [dbo].[NumeroDocumento]( [IdNumeroDocumento] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY, [Ultimo_Numero] [int] NOT NULL, [FechaRegistro] [datetime] DEFAULT GETDATE() );
GO
CREATE TABLE [dbo].[Venta]( [IdVenta] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY, [NumeroDocumento] [varchar](40) NULL, [TipoPago] [varchar](50) NULL, [Total] [decimal](18, 2) NULL, [FechaRegistro] [datetime] DEFAULT GETDATE() );
GO
CREATE TABLE [dbo].[DetalleVenta]( [IdDetalleVenta] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY, [IdVenta] [int] NULL, [IdProducto] [int] NULL, [Cantidad] [int] NULL, [Precio] [decimal](18, 2) NULL, [Total] [decimal](18, 2) NULL, FOREIGN KEY ([IdVenta]) REFERENCES [Venta]([IdVenta]), FOREIGN KEY ([IdProducto]) REFERENCES [Producto]([IdProducto]) );
GO

-- DATOS
INSERT INTO Rol (Nombre) VALUES ('Administrador'), ('Empleado'), ('Supervisor');
INSERT INTO Menu (Nombre, Icono, Url) VALUES ('DashBoard','dashboard','/pages/dashboard'), ('Usuarios','group','/pages/usuarios'), ('Productos','collections_bookmark','/pages/productos'), ('Venta','currency_exchange','/pages/venta'), ('Historial Ventas','edit_note','/pages/historial_venta'), ('Reportes','receipt','/pages/reportes');
INSERT INTO MenuRol (IdMenu, IdRol) VALUES (1,1),(2,1),(3,1),(4,1),(5,1),(6,1);
INSERT INTO MenuRol (IdMenu, IdRol) VALUES (4,2),(5,2);
INSERT INTO MenuRol (IdMenu, IdRol) VALUES (3,3),(4,3),(5,3),(6,3);
INSERT INTO Categoria (Nombre) VALUES ('Laptops'),('Monitores'),('Teclados'),('Auriculares'),('Memorias'),('Accesorios');
INSERT INTO Producto (Nombre, IdCategoria, Stock, Precio) VALUES ('Laptop HP',1,10,2500000);
INSERT INTO NumeroDocumento (Ultimo_Numero) VALUES (0);
GO