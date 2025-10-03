USE [bdmedica];
GO

IF OBJECT_ID('dbo.consultas', 'U') IS NOT NULL
    DROP TABLE dbo.consultas;
GO

IF OBJECT_ID('dbo.usuarios', 'U') IS NOT NULL
    DROP TABLE dbo.usuarios;
GO

IF OBJECT_ID('dbo.pacientes', 'U') IS NOT NULL
    DROP TABLE dbo.pacientes;
GO

IF OBJECT_ID('dbo.medicos', 'U') IS NOT NULL
    DROP TABLE dbo.medicos;
GO

CREATE TABLE dbo.medicos
(
    id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    primer_nombre NVARCHAR(100) NOT NULL,
    segundo_nombre NVARCHAR(100) NULL,
    apellido_paterno NVARCHAR(100) NOT NULL,
    apellido_materno NVARCHAR(100) NULL,
    cedula NVARCHAR(50) NOT NULL,
    telefono NVARCHAR(20) NULL,
    especialidad NVARCHAR(150) NULL,
    email NVARCHAR(150) NULL,
    activo BIT NOT NULL DEFAULT (1),
    fecha_creacion DATETIME2 NOT NULL DEFAULT (SYSUTCDATETIME())
);
GO

CREATE UNIQUE INDEX IX_medicos_cedula ON dbo.medicos (cedula);
GO

CREATE UNIQUE INDEX IX_medicos_email ON dbo.medicos (email) WHERE email IS NOT NULL;
GO

CREATE TABLE dbo.pacientes
(
    id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    primer_nombre NVARCHAR(100) NOT NULL,
    segundo_nombre NVARCHAR(100) NULL,
    apellido_paterno NVARCHAR(100) NOT NULL,
    apellido_materno NVARCHAR(100) NULL,
    telefono NVARCHAR(20) NULL,
    activo BIT NOT NULL DEFAULT (1),
    fecha_creacion DATETIME2 NOT NULL DEFAULT (SYSUTCDATETIME())
);
GO

CREATE TABLE dbo.usuarios
(
    id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    correo NVARCHAR(150) NOT NULL,
    password NVARCHAR(512) NOT NULL,
    nombre_completo NVARCHAR(200) NOT NULL,
    id_medico INT NULL,
    activo BIT NOT NULL DEFAULT (1),
    fecha_creacion DATETIME2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT FK_usuarios_medicos FOREIGN KEY (id_medico) REFERENCES dbo.medicos(id) ON DELETE SET NULL
);
GO

CREATE UNIQUE INDEX IX_usuarios_correo ON dbo.usuarios (correo);
GO

CREATE TABLE dbo.consultas
(
    id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    id_medico INT NOT NULL,
    id_paciente INT NOT NULL,
    fecha_consulta DATETIME2 NOT NULL,
    sintomas NVARCHAR(500) NULL,
    recomendaciones NVARCHAR(500) NULL,
    diagnostico NVARCHAR(500) NULL,
    CONSTRAINT FK_consultas_medicos FOREIGN KEY (id_medico) REFERENCES dbo.medicos(id) ON DELETE NO ACTION,
    CONSTRAINT FK_consultas_pacientes FOREIGN KEY (id_paciente) REFERENCES dbo.pacientes(id) ON DELETE NO ACTION
);
GO
