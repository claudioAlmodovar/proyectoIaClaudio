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
    activo BIT NOT NULL
);
GO

SET IDENTITY_INSERT dbo.Usuarios ON;
INSERT INTO dbo.Usuarios (idUsuarios, usuario, nombre, activo)
VALUES
    (1, N'user1', N'juan perez', 1),
    (2, N'user2', N'luis lopez', 1);
SET IDENTITY_INSERT dbo.Usuarios OFF;
GO
