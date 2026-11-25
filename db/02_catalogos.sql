/*
  02_catalogos.sql · Tarea 1.2
  Carga los catálogos iniciales: aulas y laboratorios, categorías de equipo con su criticidad,
  y usuarios de prueba con los perfiles Reportante y Mantenimiento.

  Ejecútelo después de 01_esquema.sql. Se puede ejecutar varias veces: solo agrega lo que falta.
  Los nombres y correos de usuarios son ficticios, para pruebas.
*/
SET NOCOUNT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

USE AveriasUnphu;
GO

-- Aulas y laboratorios
INSERT INTO dbo.aulas (codigo, nombre, edificio, tipo)
SELECT v.codigo, v.nombre, v.edificio, v.tipo
FROM (VALUES
    (N'A-101',     N'Aula 101',                     N'Edificio A', N'Aula'),
    (N'A-102',     N'Aula 102',                     N'Edificio A', N'Aula'),
    (N'A-201',     N'Aula 201',                     N'Edificio A', N'Aula'),
    (N'B-105',     N'Aula 105',                     N'Edificio B', N'Aula'),
    (N'B-210',     N'Aula 210',                     N'Edificio B', N'Aula'),
    (N'LAB-INF-1', N'Laboratorio de Informática 1', N'Edificio C', N'Laboratorio'),
    (N'LAB-INF-2', N'Laboratorio de Informática 2', N'Edificio C', N'Laboratorio'),
    (N'LAB-RED',   N'Laboratorio de Redes',         N'Edificio C', N'Laboratorio')
) AS v (codigo, nombre, edificio, tipo)
WHERE NOT EXISTS (SELECT 1 FROM dbo.aulas a WHERE a.codigo = v.codigo);

-- Categorías de equipo. Criticidad: qué tan importante es el equipo para dar la clase.
INSERT INTO dbo.categorias_equipo (nombre, criticidad)
SELECT v.nombre, v.criticidad
FROM (VALUES
    (N'Proyector',                     N'Alta'),
    (N'Computadora',                   N'Alta'),
    (N'Red e internet',                N'Alta'),
    (N'Aire acondicionado',            N'Media'),
    (N'Iluminación',                   N'Media'),
    (N'Electricidad y tomacorrientes', N'Media'),
    (N'Audio y micrófono',             N'Media'),
    (N'Mobiliario',                    N'Baja'),
    (N'Pizarra',                       N'Baja')
) AS v (nombre, criticidad)
WHERE NOT EXISTS (SELECT 1 FROM dbo.categorias_equipo c WHERE c.nombre = v.nombre);

-- Usuarios de prueba con cada perfil
INSERT INTO dbo.usuarios (nombre, correo, perfil)
SELECT v.nombre, v.correo, v.perfil
FROM (VALUES
    (N'Docente de prueba',            N'docente.prueba@averias.local',     N'Reportante'),
    (N'Estudiante de prueba',         N'estudiante.prueba@averias.local',  N'Reportante'),
    (N'Técnico de mantenimiento',     N'tecnico.prueba@averias.local',     N'Mantenimiento'),
    (N'Coordinador de mantenimiento', N'coordinador.prueba@averias.local', N'Mantenimiento')
) AS v (nombre, correo, perfil)
WHERE NOT EXISTS (SELECT 1 FROM dbo.usuarios u WHERE u.correo = v.correo);
GO

-- Comprobación: cantidad de registros por catálogo
SELECT N'aulas' AS catalogo, COUNT(*) AS registros FROM dbo.aulas
UNION ALL SELECT N'categorias_equipo', COUNT(*) FROM dbo.categorias_equipo
UNION ALL SELECT N'usuarios (Reportante)', COUNT(*) FROM dbo.usuarios WHERE perfil = N'Reportante'
UNION ALL SELECT N'usuarios (Mantenimiento)', COUNT(*) FROM dbo.usuarios WHERE perfil = N'Mantenimiento';
GO
