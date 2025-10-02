GO
IF DB_ID(N'bdmedica') IS NULL
BEGIN
    CREATE DATABASE bdmedica;
END;
GO

USE bdmedica;
GO

IF OBJECT_ID(N'dbo.Usuarios', N'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.Usuarios;
END;
GO

CREATE TABLE dbo.Usuarios
(
    idUsuarios INT IDENTITY(1,1) PRIMARY KEY,
    usuario NVARCHAR(50) NOT NULL,
    nombre NVARCHAR(100) NOT NULL,
    contrasena NVARCHAR(100) NOT NULL,
    activo BIT NOT NULL
);
GO

SET IDENTITY_INSERT dbo.Usuarios ON;
INSERT INTO dbo.Usuarios (idUsuarios, usuario, nombre, contrasena, activo)
VALUES
    (1, N'recepcion', N'Laura Sánchez', N'recepcion123', 1),
    (2, N'doctor1', N'Dr. Jorge Medina', N'consulta2024', 1),
    (3, N'admin', N'Administración', N'admin2024', 0);
SET IDENTITY_INSERT dbo.Usuarios OFF;
GO
