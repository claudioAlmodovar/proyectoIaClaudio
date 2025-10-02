-- Script para crear la base de datos, la tabla de usuarios y el procedimiento de login
IF DB_ID(N'UsuariosDB') IS NULL
BEGIN
    CREATE DATABASE UsuariosDB;
END
GO

USE UsuariosDB;
GO

IF OBJECT_ID(N'dbo.Usuarios', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Usuarios
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Usuario NVARCHAR(50) NOT NULL UNIQUE,
        Contrasena NVARCHAR(255) NOT NULL
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WHERE Usuario = N'admin')
BEGIN
    INSERT INTO dbo.Usuarios (Usuario, Contrasena)
    VALUES (N'admin', N'123');
END
GO

IF OBJECT_ID(N'dbo.procLogin', N'P') IS NOT NULL
BEGIN
    DROP PROCEDURE dbo.procLogin;
END
GO

CREATE PROCEDURE dbo.procLogin
    @pUsuario     NVARCHAR(50),
    @pContrasena  NVARCHAR(255),
    @pResultado   BIT OUTPUT,
    @pMensaje     VARCHAR(300) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @storedPassword NVARCHAR(255);

    IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WHERE Usuario = @pUsuario)
    BEGIN
        SET @pResultado = 0;
        SET @pMensaje = 'El usuario especificado no existe.';
        RETURN;
    END;

    SELECT @storedPassword = Contrasena
    FROM dbo.Usuarios
    WHERE Usuario = @pUsuario;

    IF @storedPassword = @pContrasena
    BEGIN
        SET @pResultado = 1;
        SET @pMensaje = NULL;
    END
    ELSE
    BEGIN
        SET @pResultado = 0;
        SET @pMensaje = 'Las credenciales proporcionadas son incorrectas.';
    END;
END;
GO
