/*
  01_esquema.sql · Tarea 1.1
  Crea la base de datos AveriasUnphu y sus tablas: aulas, categorias_equipo, usuarios y solicitudes.

  Cómo ejecutarlo:
    - SSMS: abra el archivo y pulse Ejecutar (F5).
    - sqlcmd: sqlcmd -S localhost -E -i db/01_esquema.sql
      (con SQL Server Express: -S localhost\SQLEXPRESS)

  Advertencia: borra las tablas si ya existen. Úselo solo en desarrollo.
*/
SET NOCOUNT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF DB_ID(N'AveriasUnphu') IS NULL
    CREATE DATABASE AveriasUnphu;
GO

USE AveriasUnphu;
GO

DROP TABLE IF EXISTS dbo.solicitudes;
DROP TABLE IF EXISTS dbo.usuarios;
DROP TABLE IF EXISTS dbo.categorias_equipo;
DROP TABLE IF EXISTS dbo.aulas;
GO

-- Aulas y laboratorios del campus donde se reportan averías.
CREATE TABLE dbo.aulas (
    id        INT IDENTITY(1, 1) NOT NULL,
    codigo    NVARCHAR(20)  NOT NULL,
    nombre    NVARCHAR(100) NOT NULL,
    edificio  NVARCHAR(60)  NOT NULL,
    tipo      NVARCHAR(20)  NOT NULL,
    activo    BIT           NOT NULL CONSTRAINT df_aulas_activo DEFAULT (1),
    CONSTRAINT pk_aulas PRIMARY KEY (id),
    CONSTRAINT uq_aulas_codigo UNIQUE (codigo),
    CONSTRAINT ck_aulas_tipo CHECK (tipo IN (N'Aula', N'Laboratorio'))
);

-- Categorías de equipo. La criticidad se usa para calcular la prioridad (tarea 3.1).
CREATE TABLE dbo.categorias_equipo (
    id          INT IDENTITY(1, 1) NOT NULL,
    nombre      NVARCHAR(60) NOT NULL,
    criticidad  NVARCHAR(10) NOT NULL,
    activo      BIT          NOT NULL CONSTRAINT df_categorias_activo DEFAULT (1),
    CONSTRAINT pk_categorias_equipo PRIMARY KEY (id),
    CONSTRAINT uq_categorias_equipo_nombre UNIQUE (nombre),
    CONSTRAINT ck_categorias_equipo_criticidad CHECK (criticidad IN (N'Alta', N'Media', N'Baja'))
);

-- Usuarios con su perfil: Reportante (docentes y estudiantes) o Mantenimiento.
CREATE TABLE dbo.usuarios (
    id      INT IDENTITY(1, 1) NOT NULL,
    nombre  NVARCHAR(100) NOT NULL,
    correo  NVARCHAR(120) NOT NULL,
    perfil  NVARCHAR(20)  NOT NULL,
    activo  BIT           NOT NULL CONSTRAINT df_usuarios_activo DEFAULT (1),
    CONSTRAINT pk_usuarios PRIMARY KEY (id),
    CONSTRAINT uq_usuarios_correo UNIQUE (correo),
    CONSTRAINT ck_usuarios_perfil CHECK (perfil IN (N'Reportante', N'Mantenimiento'))
);

-- Solicitudes de reparación (una por avería reportada).
CREATE TABLE dbo.solicitudes (
    id                   INT IDENTITY(1, 1) NOT NULL,
    aula_id              INT            NOT NULL,
    categoria_id         INT            NOT NULL,
    usuario_id           INT            NOT NULL,
    descripcion          NVARCHAR(500)  NOT NULL,
    impacto              NVARCHAR(30)   NOT NULL,
    prioridad            NVARCHAR(10)   NOT NULL,
    estado               NVARCHAR(20)   NOT NULL CONSTRAINT df_solicitudes_estado DEFAULT (N'Pendiente'),
    fecha_reporte        DATETIME2(0)   NOT NULL CONSTRAINT df_solicitudes_fecha DEFAULT (SYSDATETIME()),
    fecha_actualizacion  DATETIME2(0)   NULL,
    fecha_resolucion     DATETIME2(0)   NULL,
    CONSTRAINT pk_solicitudes PRIMARY KEY (id),
    CONSTRAINT fk_solicitudes_aulas FOREIGN KEY (aula_id) REFERENCES dbo.aulas (id),
    CONSTRAINT fk_solicitudes_categorias FOREIGN KEY (categoria_id) REFERENCES dbo.categorias_equipo (id),
    CONSTRAINT fk_solicitudes_usuarios FOREIGN KEY (usuario_id) REFERENCES dbo.usuarios (id),
    CONSTRAINT ck_solicitudes_impacto CHECK (impacto IN (N'Impide la clase', N'Afecta parcialmente', N'No afecta')),
    CONSTRAINT ck_solicitudes_prioridad CHECK (prioridad IN (N'Alta', N'Media', N'Baja')),
    CONSTRAINT ck_solicitudes_estado CHECK (estado IN (N'Pendiente', N'En proceso', N'Resuelta')),
    CONSTRAINT ck_solicitudes_descripcion CHECK (LEN(descripcion) > 0)
);
GO

/*
  Regla de duplicidad (RD-01): solo puede haber UNA solicitud activa (Pendiente o En proceso)
  por aula y categoría de equipo. Este índice único filtrado lo garantiza en la base de datos,
  aunque dos personas envíen el mismo reporte al mismo tiempo.
  Una solicitud Resuelta deja de contar, así que la avería se puede volver a reportar (reapertura).
*/
CREATE UNIQUE INDEX ux_solicitudes_una_activa
    ON dbo.solicitudes (aula_id, categoria_id)
    WHERE estado IN (N'Pendiente', N'En proceso');

-- Índice para la bandeja de seguimiento (filtros por estado y orden por fecha).
CREATE INDEX ix_solicitudes_estado_fecha
    ON dbo.solicitudes (estado, fecha_reporte);
GO

PRINT N'Esquema de AveriasUnphu creado: aulas, categorias_equipo, usuarios y solicitudes.';
GO
