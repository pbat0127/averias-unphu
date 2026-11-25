/*
  03_datos_prueba.sql · Tarea 2.1
  Carga solicitudes de prueba con distintos estados para probar la consulta de duplicados
  (04_consulta_solicitudes_activas.sql) y la bandeja de seguimiento.

  Ejecútelo después de 02_catalogos.sql.
  Advertencia: BORRA todas las solicitudes antes de cargar los datos. Úselo solo en desarrollo.

  Prioridad = criticidad de la categoría × impacto en la clase (ver docs/reglas-de-negocio.md).
*/
SET NOCOUNT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

USE AveriasUnphu;
GO

DELETE FROM dbo.solicitudes;

INSERT INTO dbo.solicitudes
    (aula_id, categoria_id, usuario_id, descripcion, impacto, prioridad, estado,
     fecha_reporte, fecha_actualizacion, fecha_resolucion)
SELECT a.id, c.id, u.id, v.descripcion, v.impacto, v.prioridad, v.estado,
       DATEADD(HOUR, -v.horas, SYSDATETIME()),
       CASE WHEN v.estado = N'Pendiente' THEN NULL ELSE DATEADD(HOUR, -v.horas + 2, SYSDATETIME()) END,
       CASE WHEN v.estado = N'Resuelta'  THEN DATEADD(HOUR, -v.horas + 2, SYSDATETIME()) END
FROM (VALUES
    -- aula,        categoría,             correo del reportante,              descripción,                                   impacto,               prioridad, estado,        horas atrás
    (N'A-101',     N'Proyector',          N'docente.prueba@averias.local',    N'El proyector no enciende.',                  N'Impide la clase',     N'Alta',  N'Pendiente',   3),
    (N'LAB-INF-1', N'Computadora',        N'estudiante.prueba@averias.local', N'La computadora 12 no enciende.',             N'Afecta parcialmente', N'Alta',  N'En proceso', 26),
    (N'A-201',     N'Aire acondicionado', N'docente.prueba@averias.local',    N'El aire hace ruido y no enfría.',            N'Afecta parcialmente', N'Media', N'Resuelta',   50),
    (N'A-102',     N'Iluminación',        N'estudiante.prueba@averias.local', N'Dos lámparas fundidas al fondo del aula.',   N'Afecta parcialmente', N'Media', N'Pendiente',   5),
    (N'B-105',     N'Pizarra',            N'docente.prueba@averias.local',    N'La pizarra está manchada y no se borra bien.', N'No afecta',         N'Baja',  N'Pendiente',   8)
) AS v (aula, categoria, correo, descripcion, impacto, prioridad, estado, horas)
JOIN dbo.aulas AS a             ON a.codigo = v.aula
JOIN dbo.categorias_equipo AS c ON c.nombre = v.categoria
JOIN dbo.usuarios AS u          ON u.correo = v.correo;
GO

SELECT s.id, a.codigo AS aula, c.nombre AS categoria, s.impacto, s.prioridad, s.estado, s.fecha_reporte
FROM dbo.solicitudes AS s
JOIN dbo.aulas AS a ON a.id = s.aula_id
JOIN dbo.categorias_equipo AS c ON c.id = s.categoria_id
ORDER BY s.id;
GO
