-- =============================================================================
-- 05-DBVENTAngular-CLAVES-FIJAS.sql
-- Crea BD, Tablas, FKs y FORZA la actualización de claves de usuarios
-- =============================================================================

SET NOCOUNT ON;
GO

USE master;
GO

-- 1. Crear BD si no existe
IF DB_ID(N'DBVENTAngular') IS NULL
BEGIN
    PRINT 'Creando base de datos DBVENTAngular...';
    CREATE DATABASE [DBVENTAngular];
END
GO

USE [DBVENTAngular];
GO

-- Habilitar FullText si existe
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
BEGIN
    EXEC [dbo].[sp_fulltext_database] @action = 'enable';
END
GO

-- =============================================================================
-- TABLAS (Crear solo si no existen)
-- =============================================================================

IF OBJECT_ID(N'dbo.Rol', N'U') IS NULL
BEGIN
    PRINT 'Creando tabla Rol...';
    CREATE TABLE [dbo].[Rol](
        [idRol] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [nombre] [varchar](50) NOT NULL,
        [fechaRegistro] [datetime] DEFAULT GETDATE()
    );
END
GO

IF OBJECT_ID(N'dbo.Menu', N'U') IS NULL
BEGIN
    PRINT 'Creando tabla Menu...';
    CREATE TABLE [dbo].[Menu](
        [idMenu] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [nombre] [varchar](50) NULL,
        [icono] [varchar](50) NULL,
        [url] [varchar](50) NULL
    );
END
GO

IF OBJECT_ID(N'dbo.MenuRol', N'U') IS NULL
BEGIN
    PRINT 'Creando tabla MenuRol...';
    CREATE TABLE [dbo].[MenuRol](
        [idMenuRol] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [idMenu] [int] NOT NULL,
        [idRol] [int] NOT NULL
    );
END
GO

IF OBJECT_ID(N'dbo.Usuario', N'U') IS NULL
BEGIN
    PRINT 'Creando tabla Usuario...';
    CREATE TABLE [dbo].[Usuario](
        [idUsuario] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [nombreCompleto] [varchar](100) NULL,
        [correo] [varchar](40) NULL,
        [idRol] [int] NULL,
        [clave] [varchar](255) NULL,
        [esActivo] [bit] NULL,
        [fechaRegistro] [datetime] NULL
    );
END
GO

IF OBJECT_ID(N'dbo.Categoria', N'U') IS NULL
BEGIN
    PRINT 'Creando tabla Categoria...';
    CREATE TABLE [dbo].[Categoria](
        [idCategoria] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [nombre] [varchar](50) NULL,
        [esActivo] [bit] NULL,
        [fechaRegistro] [datetime] NULL
    );
END
GO

IF OBJECT_ID(N'dbo.Producto', N'U') IS NULL
BEGIN
    PRINT 'Creando tabla Producto...';
    CREATE TABLE [dbo].[Producto](
        [idProducto] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [nombre] [varchar](100) NULL,
        [idCategoria] [int] NULL,
        [stock] [int] NULL,
        [precio] [decimal](10,2) NULL,
        [esActivo] [bit] NULL,
        [fechaRegistro] [datetime] NULL
    );
END
GO

IF OBJECT_ID(N'dbo.Venta', N'U') IS NULL
BEGIN
    PRINT 'Creando tabla Venta...';
    CREATE TABLE [dbo].[Venta](
        [idVenta] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [numeroDocumento] [varchar](40) NULL,
        [tipoPago] [varchar](50) NULL,
        [total] [decimal](10,2) NULL,
        [fechaRegistro] [datetime] NULL
    );
END
GO

IF OBJECT_ID(N'dbo.DetalleVenta', N'U') IS NULL
BEGIN
    PRINT 'Creando tabla DetalleVenta...';
    CREATE TABLE [dbo].[DetalleVenta](
        [idDetalleVenta] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [idVenta] [int] NULL,
        [idProducto] [int] NULL,
        [cantidad] [int] NULL,
        [precio] [decimal](10,2) NULL,
        [total] [decimal](10,2) NULL
    );
END
GO

IF OBJECT_ID(N'dbo.NumeroDocumento', N'U') IS NULL
BEGIN
    PRINT 'Creando tabla NumeroDocumento...';
    CREATE TABLE [dbo].[NumeroDocumento](
        [idNumeroDocumento] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [ultimo_Numero] [int] NOT NULL,
        [fechaRegistro] [datetime] NULL
    );
END
GO

IF OBJECT_ID(N'dbo.RefreshToken', N'U') IS NULL
BEGIN
    PRINT 'Creando tabla RefreshToken...';
    CREATE TABLE [dbo].[RefreshToken](
        [Id] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [IdUsuario] [int] NOT NULL,
        [Token] [varchar](255) NOT NULL,
        [FechaCreacion] [datetime] NOT NULL,
        [FechaExpiracion] [datetime] NOT NULL,
        [Revocado] [bit] NOT NULL,
        [Activo] [bit] NULL
    );
END
GO

-- =============================================================================
-- CONSTRAINTS Y DEFAULTS
-- =============================================================================

IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE parent_object_id = OBJECT_ID('dbo.Categoria') AND name = 'DF_Categoria_esActivo')
    ALTER TABLE [dbo].[Categoria] ADD DEFAULT ((1)) FOR [esActivo];
IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE parent_object_id = OBJECT_ID('dbo.Categoria') AND name = 'DF_Categoria_fechaRegistro')
    ALTER TABLE [dbo].[Categoria] ADD DEFAULT (getdate()) FOR [fechaRegistro];

IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE parent_object_id = OBJECT_ID('dbo.Producto') AND name = 'DF_Producto_esActivo')
    ALTER TABLE [dbo].[Producto] ADD DEFAULT ((1)) FOR [esActivo];
IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE parent_object_id = OBJECT_ID('dbo.Producto') AND name = 'DF_Producto_fechaRegistro')
    ALTER TABLE [dbo].[Producto] ADD DEFAULT (getdate()) FOR [fechaRegistro];

IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE parent_object_id = OBJECT_ID('dbo.Usuario') AND name = 'DF_Usuario_esActivo')
    ALTER TABLE [dbo].[Usuario] ADD DEFAULT ((1)) FOR [esActivo];
IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE parent_object_id = OBJECT_ID('dbo.Usuario') AND name = 'DF_Usuario_fechaRegistro')
    ALTER TABLE [dbo].[Usuario] ADD DEFAULT (getdate()) FOR [fechaRegistro];

IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE parent_object_id = OBJECT_ID('dbo.NumeroDocumento') AND name = 'DF_NumeroDocumento_fechaRegistro')
    ALTER TABLE [dbo].[NumeroDocumento] ADD DEFAULT (getdate()) FOR [fechaRegistro];

IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE parent_object_id = OBJECT_ID('dbo.RefreshToken') AND name = 'DF_RT_FechaCreacion')
    ALTER TABLE [dbo].[RefreshToken] ADD DEFAULT (getdate()) FOR [FechaCreacion];
IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE parent_object_id = OBJECT_ID('dbo.RefreshToken') AND name = 'DF_RT_Revocado')
    ALTER TABLE [dbo].[RefreshToken] ADD DEFAULT ((0)) FOR [Revocado];
IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE parent_object_id = OBJECT_ID('dbo.RefreshToken') AND name = 'DF_RT_Activo')
    ALTER TABLE [dbo].[RefreshToken] ADD DEFAULT ((1)) FOR [Activo];
GO

-- Claves Foráneas
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Producto_Categoria')
    ALTER TABLE [dbo].[Producto] ADD CONSTRAINT FK_Producto_Categoria FOREIGN KEY([idCategoria]) REFERENCES [dbo].[Categoria]([idCategoria]);
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_DetalleVenta_Venta')
    ALTER TABLE [dbo].[DetalleVenta] ADD CONSTRAINT FK_DetalleVenta_Venta FOREIGN KEY([idVenta]) REFERENCES [dbo].[Venta]([idVenta]);
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_DetalleVenta_Producto')
    ALTER TABLE [dbo].[DetalleVenta] ADD CONSTRAINT FK_DetalleVenta_Producto FOREIGN KEY([idProducto]) REFERENCES [dbo].[Producto]([idProducto]);
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_MenuRol_Menu')
    ALTER TABLE [dbo].[MenuRol] ADD CONSTRAINT FK_MenuRol_Menu FOREIGN KEY([idMenu]) REFERENCES [dbo].[Menu]([idMenu]);
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_MenuRol_Rol')
    ALTER TABLE [dbo].[MenuRol] ADD CONSTRAINT FK_MenuRol_Rol FOREIGN KEY([idRol]) REFERENCES [dbo].[Rol]([idRol]);
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Usuario_Rol')
    ALTER TABLE [dbo].[Usuario] ADD CONSTRAINT FK_Usuario_Rol FOREIGN KEY([idRol]) REFERENCES [dbo].[Rol]([idRol]);
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_RT_Usuario')
    ALTER TABLE [dbo].[RefreshToken] ADD CONSTRAINT FK_RT_Usuario FOREIGN KEY([IdUsuario]) REFERENCES [dbo].[Usuario]([idUsuario]) ON DELETE CASCADE;
GO

-- =============================================================================
-- DATOS SEMILLA: ROLES Y USUARIOS (FORZANDO ACTUALIZACIÓN DE CLAVES)
-- =============================================================================

PRINT 'Insertando Roles...';
IF NOT EXISTS (SELECT 1 FROM Rol WHERE nombre = 'Administrador')
    INSERT INTO Rol (nombre) VALUES ('Administrador');
IF NOT EXISTS (SELECT 1 FROM Rol WHERE nombre = 'Supervisor')
    INSERT INTO Rol (nombre) VALUES ('Supervisor');
IF NOT EXISTS (SELECT 1 FROM Rol WHERE nombre = 'Empleado')
    INSERT INTO Rol (nombre) VALUES ('Empleado');
GO

DECLARE @IdAdmin INT = (SELECT idRol FROM Rol WHERE nombre = 'Administrador');
DECLARE @IdSupervisor INT = (SELECT idRol FROM Rol WHERE nombre = 'Supervisor');
DECLARE @IdEmpleado INT = (SELECT idRol FROM Rol WHERE nombre = 'Empleado');

PRINT 'Forzando actualización de claves de usuarios...';

-- 1. ADMIN: Siempre actualizar clave
IF NOT EXISTS (SELECT 1 FROM Usuario WHERE correo = 'admin@sistema.com')
BEGIN
    PRINT '  -> Creando usuario Admin con clave...';
    INSERT INTO Usuario (nombreCompleto, correo, idRol, clave, esActivo, fechaRegistro)
    VALUES ('Administrador Sistema', 'admin@sistema.com', @IdAdmin, 
            '$2a$11$.lBfPiI91jtze27gdVm8V.ewtUupuRH5oNC6LNMvwkOjYNvOF7vAu', 1, GETDATE());
END
ELSE
BEGIN
    PRINT '  -> Actualizando clave de usuario Admin (independientemente de su valor actual)...';
    UPDATE Usuario 
    SET clave = '$2a$11$.lBfPiI91jtze27gdVm8V.ewtUupuRH5oNC6LNMvwkOjYNvOF7vAu'
    WHERE correo = 'admin@sistema.com';
END

-- 2. SUPERVISOR: Siempre actualizar clave
IF NOT EXISTS (SELECT 1 FROM Usuario WHERE correo = 'supervisor@sistema.com')
BEGIN
    PRINT '  -> Creando usuario Supervisor con clave...';
    INSERT INTO Usuario (nombreCompleto, correo, idRol, clave, esActivo, fechaRegistro)
    VALUES ('Supervisor Demo', 'supervisor@sistema.com', @IdSupervisor, 
            '$2a$11$HRRUXVai56rDDasKtLRxFOp2Fho6YaPlS7A3JnUXU4f35LXscbv2C', 1, GETDATE());
END
ELSE
BEGIN
    PRINT '  -> Actualizando clave de usuario Supervisor (independientemente de su valor actual)...';
    UPDATE Usuario 
    SET clave = '$2a$11$HRRUXVai56rDDasKtLRxFOp2Fho6YaPlS7A3JnUXU4f35LXscbv2C'
    WHERE correo = 'supervisor@sistema.com';
END

-- 3. EMPLEADO: Siempre actualizar clave
IF NOT EXISTS (SELECT 1 FROM Usuario WHERE correo = 'empleado@sistema.com')
BEGIN
    PRINT '  -> Creando usuario Empleado con clave...';
    INSERT INTO Usuario (nombreCompleto, correo, idRol, clave, esActivo, fechaRegistro)
    VALUES ('Empleado Demo', 'empleado@sistema.com', @IdEmpleado, 
            '$2a$11$pV2ojpPvROV4Ht5ZkyRhpOi94lonEwwYPLoB8hIay3knUjH7TTkEe', 1, GETDATE());
END
ELSE
BEGIN
    PRINT '  -> Actualizando clave de usuario Empleado (independientemente de su valor actual)...';
    UPDATE Usuario 
    SET clave = '$2a$11$pV2ojpPvROV4Ht5ZkyRhpOi94lonEwwYPLoB8hIay3knUjH7TTkEe'
    WHERE correo = 'empleado@sistema.com';
END
GO

-- =============================================================================
-- DATOS SEMILLA ADICIONALES: MENUS, CATEGORIAS Y NUMERO DOCUMENTO
-- =============================================================================

-- 4. MENUS
PRINT 'Insertando Menus...';
IF NOT EXISTS (SELECT 1 FROM Menu WHERE nombre = 'DashBoard')
    INSERT INTO Menu (nombre, icono, url) VALUES ('DashBoard','dashboard','/pages/dashboard');
IF NOT EXISTS (SELECT 1 FROM Menu WHERE nombre = 'Usuarios')
    INSERT INTO Menu (nombre, icono, url) VALUES ('Usuarios','group','/pages/usuarios');
IF NOT EXISTS (SELECT 1 FROM Menu WHERE nombre = 'Productos')
    INSERT INTO Menu (nombre, icono, url) VALUES ('Productos','collections_bookmark','/pages/productos');
IF NOT EXISTS (SELECT 1 FROM Menu WHERE nombre = 'Venta')
    INSERT INTO Menu (nombre, icono, url) VALUES ('Venta','currency_exchange','/pages/venta');
IF NOT EXISTS (SELECT 1 FROM Menu WHERE nombre = 'Historial Ventas')
    INSERT INTO Menu (nombre, icono, url) VALUES ('Historial Ventas','edit_note','/pages/historial_venta');
IF NOT EXISTS (SELECT 1 FROM Menu WHERE nombre = 'Reportes')
    INSERT INTO Menu (nombre, icono, url) VALUES ('Reportes','receipt','/pages/reportes');
GO

-- 5. RELACIÓN MENU ROL
PRINT 'Insertando Relación Menu-Rol...';
DECLARE @IdMenuDB INT = (SELECT idMenu FROM Menu WHERE nombre = 'DashBoard');
DECLARE @IdMenuUsuarios INT = (SELECT idMenu FROM Menu WHERE nombre = 'Usuarios');
DECLARE @IdMenuProductos INT = (SELECT idMenu FROM Menu WHERE nombre = 'Productos');
DECLARE @IdMenuVenta INT = (SELECT idMenu FROM Menu WHERE nombre = 'Venta');
DECLARE @IdMenuHistorial INT = (SELECT idMenu FROM Menu WHERE nombre = 'Historial Ventas');
DECLARE @IdMenuReportes INT = (SELECT idMenu FROM Menu WHERE nombre = 'Reportes');

DECLARE @IdAdmin INT = (SELECT idRol FROM Rol WHERE nombre = 'Administrador');
DECLARE @IdSupervisor INT = (SELECT idRol FROM Rol WHERE nombre = 'Supervisor');
DECLARE @IdEmpleado INT = (SELECT idRol FROM Rol WHERE nombre = 'Empleado');

-- Admin tiene todos los accesos
IF NOT EXISTS (SELECT 1 FROM MenuRol WHERE idMenu = @IdMenuDB AND idRol = @IdAdmin)
    INSERT INTO MenuRol (idMenu, idRol) VALUES (@IdMenuDB, @IdAdmin);
IF NOT EXISTS (SELECT 1 FROM MenuRol WHERE idMenu = @IdMenuUsuarios AND idRol = @IdAdmin)
    INSERT INTO MenuRol (idMenu, idRol) VALUES (@IdMenuUsuarios, @IdAdmin);
IF NOT EXISTS (SELECT 1 FROM MenuRol WHERE idMenu = @IdMenuProductos AND idRol = @IdAdmin)
    INSERT INTO MenuRol (idMenu, idRol) VALUES (@IdMenuProductos, @IdAdmin);
IF NOT EXISTS (SELECT 1 FROM MenuRol WHERE idMenu = @IdMenuVenta AND idRol = @IdAdmin)
    INSERT INTO MenuRol (idMenu, idRol) VALUES (@IdMenuVenta, @IdAdmin);
IF NOT EXISTS (SELECT 1 FROM MenuRol WHERE idMenu = @IdMenuHistorial AND idRol = @IdAdmin)
    INSERT INTO MenuRol (idMenu, idRol) VALUES (@IdMenuHistorial, @IdAdmin);
IF NOT EXISTS (SELECT 1 FROM MenuRol WHERE idMenu = @IdMenuReportes AND idRol = @IdAdmin)
    INSERT INTO MenuRol (idMenu, idRol) VALUES (@IdMenuReportes, @IdAdmin);

-- Supervisor (Venta e Historial)
IF NOT EXISTS (SELECT 1 FROM MenuRol WHERE idMenu = @IdMenuVenta AND idRol = @IdSupervisor)
    INSERT INTO MenuRol (idMenu, idRol) VALUES (@IdMenuVenta, @IdSupervisor);
IF NOT EXISTS (SELECT 1 FROM MenuRol WHERE idMenu = @IdMenuHistorial AND idRol = @IdSupervisor)
    INSERT INTO MenuRol (idMenu, idRol) VALUES (@IdMenuHistorial, @IdSupervisor);

-- Empleado (Productos, Venta, Historial y Reportes)
IF NOT EXISTS (SELECT 1 FROM MenuRol WHERE idMenu = @IdMenuProductos AND idRol = @IdEmpleado)
    INSERT INTO MenuRol (idMenu, idRol) VALUES (@IdMenuProductos, @IdEmpleado);
IF NOT EXISTS (SELECT 1 FROM MenuRol WHERE idMenu = @IdMenuVenta AND idRol = @IdEmpleado)
    INSERT INTO MenuRol (idMenu, idRol) VALUES (@IdMenuVenta, @IdEmpleado);
IF NOT EXISTS (SELECT 1 FROM MenuRol WHERE idMenu = @IdMenuHistorial AND idRol = @IdEmpleado)
    INSERT INTO MenuRol (idMenu, idRol) VALUES (@IdMenuHistorial, @IdEmpleado);
IF NOT EXISTS (SELECT 1 FROM MenuRol WHERE idMenu = @IdMenuReportes AND idRol = @IdEmpleado)
    INSERT INTO MenuRol (idMenu, idRol) VALUES (@IdMenuReportes, @IdEmpleado);
GO

-- 6. CATEGORIAS
PRINT 'Insertando Categorias Semilla...';
IF NOT EXISTS (SELECT 1 FROM Categoria WHERE nombre = 'Laptops')
    INSERT INTO Categoria (nombre) VALUES ('Laptops');
IF NOT EXISTS (SELECT 1 FROM Categoria WHERE nombre = 'Monitores')
    INSERT INTO Categoria (nombre) VALUES ('Monitores');
IF NOT EXISTS (SELECT 1 FROM Categoria WHERE nombre = 'Teclados')
    INSERT INTO Categoria (nombre) VALUES ('Teclados');
IF NOT EXISTS (SELECT 1 FROM Categoria WHERE nombre = 'Auriculares')
    INSERT INTO Categoria (nombre) VALUES ('Auriculares');
IF NOT EXISTS (SELECT 1 FROM Categoria WHERE nombre = 'Memorias')
    INSERT INTO Categoria (nombre) VALUES ('Memorias');
IF NOT EXISTS (SELECT 1 FROM Categoria WHERE nombre = 'Accesorios')
    INSERT INTO Categoria (nombre) VALUES ('Accesorios');
GO

-- 7. NUMERO DOCUMENTO
PRINT 'Insertando Numero Documento Inicial...';
IF NOT EXISTS (SELECT 1 FROM NumeroDocumento)
    INSERT INTO NumeroDocumento (ultimo_Numero) VALUES (0);
GO

PRINT '>>> Finalizado. Claves forzadas y datos semilla adicionales cargados.';
GO